# Parola Politikasý Tasarým Dokümaný (PasswordPolicy)

Revizyon: v2.2 (2025-11-28)
Önceki sürüm: v2.1 (2025-11-26)

Bu doküman v2.1 içeriðini TAM olarak korur + "Parametre Kayýtlarý" (ApplicationId=1) kapsamý için kalan iþ kalemlerini ve ilerleme yüzdesini ekler.

---
## 0. Revizyon Notlarý (v2.2)
Tamamlanan çekirdek (v2.1):
- Tek JSON model (Group=Security, Key=PasswordPolicy, ParameterDataTypeId=15)
- Provider + IMemoryCache + Invalidate akýþý
- Validator (uzunluk, kategori, farklý karakter, tekrar sekansý, blok liste)
- Argon2id hashing + PBKDF2-SHA512 fallback + pepperEnabled bayraðý
- Yönetim Razor Page ile JSON görüntüleme / düzenleme / doðrulama / önizleme

Parametre Kayýtlarý bölümü gerçekleþme oraný: ~%50

Kalan özel iþler (sadece Parametre Kayýtlarý açýsýndan):
1. Çoklu ApplicationId seed/migration senaryolarý (varsayýlan dýþý politikalar).
2. Otomatik migration: Politika kaydý yoksa idempotent insert (baþlangýç kontrolü).
3. Þema validation (server-side) – required alan + tür doðrulama (JSON Schema / custom).
4. allowedSymbols senkronizasyonu: UI örneði ile parametre içeriðinin tutarlýlýk kontrolü.
5. Audit trail: Eski / yeni JSON + kullanýcý + zaman (ayrý PasswordPolicyAudit tablosu).
6. Concurrency kontrolü: RowVersion veya ETag mantýðý ile yarýþ durumlarý engelleme.
7. Versiyonlama stratejisi: version alaný için yükseltme hook’larý (future schema evolution).
8. PepperEnabled=true & ortam deðiþkeni yok senaryosu için kayýt düzeyinde uyarý/log.
9. Bütünlük / rollback: Geçersiz JSON kaydý durumunda önceki deðeri koruma (transaction + validation önce).
10. Ýmza / bütünlük doðrulamasý (opsiyonel): JSON üzerinde HMAC veya checksum kolon.
11. Ýzleme metrikleri: parametre okuma sayýsý, invalidate çaðrý sayýsý.
12. Yönetim ekranýnda normalize/minify: Kaydetmeden önce satýr sonu + boþluk standardizasyonu.

Önerilen kýsa vadeli sýrala: (1)–(3) temel; sonra (5)(6)(7); ardýndan (8)(9)(11); opsiyonel (10)(12).

Backlog durum etiketleri:
- PLAN: Henüz baþlanmadý
- WIP: Çalýþýlýyor
- DONE: Tamamlandý

| ID | Ýþ | Durum | Not |
|----|-----|-------|-----|
| PK-01 | Çoklu ApplicationId seed | PLAN | Migration + fallback lookup |
| PK-02 | Otomatik insert / idempotent migration | PLAN | Startup kontrolü |
| PK-03 | Server-side schema validation | PLAN | FluentValidation / System.Text.Json node traversal |
| PK-04 | allowedSymbols konsistens kontrolü | PLAN | UI vs parametre diff |
| PK-05 | Audit trail tablosu | PLAN | (Id, AppId, OldJson, NewJson, UserId, Utc) |
| PK-06 | Concurrency (RowVersion) | PLAN | EF Core concurrency token |
| PK-07 | Schema version upgrade hook | PLAN | Strategy class / version dispatcher |
| PK-08 | PepperEnabled env uyarýsý | PLAN | Logger + health check |
| PK-09 | Rollback mekanizmasý | PLAN | Validation önce transaction |
| PK-10 | HMAC bütünlük | PLAN | Opsiyonel güvenlik sertleþtirme |
| PK-11 | Parametre metrikleri | PLAN | Prometheus counters |
| PK-12 | Normalize/minify JSON | PLAN | Strip trailing spaces + stable ordering |

Ýlerleme (Parametre Kayýtlarý kýsmi yüzdesi): 50% (çekirdek kayýt/okuma/güncelleme tamam, yukarýdaki backlog açýk).

---
## 1. Amaç / Kapsam
Parola güvenliði, veritabanýndaki JSON parametreleri ile yönetilir; deðiþiklik için deploy gerekmez. Uygulama, `Parameters` tablosundan politikayý okur ve bellekte önbellekler.

## 2. Parametre Kayýtlarý (ApplicationId=1)
- Parola politikasý (tekleþtirilmiþ model):
  - Group: `Security`, Key: `PasswordPolicy`, ParameterDataTypeId: `15 (Json)`
- Ýkili doðrulama (bilgi amaçlý):
  - Group: `TwoFactor`, Key: `Options`, varsayýlan `Value`: `{"defaultChannel":"Sms"}`

Not: Eski “Group=PasswordPolicy / Key=Options,Argon2” yaklaþýmý yerine tek JSON altýnda `hash` bölümü bulunan yapý kullanýlýr.

## 3. PasswordPolicy JSON Þemasý (Kesin)
Alanlar:
- version: number
- minLength: number
- maxLength: number
- requireUpper: boolean
- requireLower: boolean
- requireDigit: boolean
- requireSymbol: boolean
- allowedSymbols: string
- minDistinctChars: number
- maxRepeatedSequence: number
- blockList: string[]
- historyCount: number
- lockoutThreshold: number
- lockoutSeconds: number
- hash: object
  - algorithm: "Argon2id"
  - memoryKb: number
  - parallelism: number
  - iterations: number
  - saltLength: number
  - hashLength: number
  - fallback: object
    - algorithm: "PBKDF2-SHA512"
    - iterations: number
  - pepperEnabled: boolean

Örnek Value:
```json
{ "version": 1, "minLength": 12, "maxLength": 128, "requireUpper": true, "requireLower": true, "requireDigit": true, "requireSymbol": true, "allowedSymbols": "!@#$%^&*_-+=:?.,;", "minDistinctChars": 5, "maxRepeatedSequence": 3, "blockList": ["password", "123456", "qwerty", "admin"], "historyCount": 10, "lockoutThreshold": 5, "lockoutSeconds": 900, "hash": { "algorithm": "Argon2id", "memoryKb": 65536, "parallelism": 2, "iterations": 3, "saltLength": 16, "hashLength": 32, "fallback": { "algorithm": "PBKDF2-SHA512", "iterations": 210000 }, "pepperEnabled": false } }
```

## 4. DTO Sýnýflarý (Gerçek Kod)
```json
{ "version": 1, "minLength": 12, "maxLength": 128, "requireUpper": true, "requireLower": true, "requireDigit": true, "requireSymbol": true, "allowedSymbols": "!@#$%^&*_-+=:?.,;", "minDistinctChars": 5, "maxRepeatedSequence": 3, "blockList": ["password", "123456", "qwerty", "admin"], "historyCount": 10, "lockoutThreshold": 5, "lockoutSeconds": 900, "hash": { "algorithm": "Argon2id", "memoryKb": 65536, "parallelism": 2, "iterations": 3, "saltLength": 16, "hashLength": 32, "fallback": { "algorithm": "PBKDF2-SHA512", "iterations": 210000 }, "pepperEnabled": false } }
```

## 5. Saðlayýcý (Provider) ve Önbellek
Gerçek arayüz:
```csharp
public interface IPasswordPolicyProvider
{
    ValueTask<PasswordPolicyOptions> GetAsync(int applicationId = 1, CancellationToken ct = default);
    void Invalidate(int applicationId = 1);
}
```
Davranýþ:
- `GetAsync` ilk çaðrýda DB’den okur, `IMemoryCache` ile önbelleðe alýr.
- Politika güncellendiðinde `Invalidate(appId)` çaðrýlýr; sonraki `GetAsync` yeniden yükler.

## 6. Doðrulama (Validator)
- Ýmza: `IReadOnlyList<string> Validate(string password, PasswordPolicyOptions policy)`
- Hata kodlarý: `EMPTY`, `MIN_LENGTH`, `MAX_LENGTH`, `REQ_UPPER`, `REQ_LOWER`, `REQ_DIGIT`, `REQ_SYMBOL`, `MIN_DISTINCT`, `REPEAT_SEQ`, `BLOCK_LIST`
- Sýra: uzunluk ? kategori kontrolleri ? ayýrt edici karakter ? tekrar sekansý ? blok liste
- Not: HIBP/Pwned ve yaþ/geçmiþ kontrolleri yol haritasýndadýr.

## 7. Hashleme
- Algoritma: Argon2id (Isopoh.Cryptography.Argon2)
- Çýktý: Standart Argon2 encoded string
- Pepper: `ARCHIX_PEPPER` ortam deðiþkeni (pepperEnabled true ise eklenir)
- Fallback: PBKDF2-SHA512

## 8. Seed / Migration
- Relational DB: Migration örn. TwoFactorDefaultChannelSms.
- InMemory: `HasData` tohumlarý.

## 9. Güncelleme Akýþý
1) `Parameters.Value` güncellenir  
2) `IPasswordPolicyProvider.Invalidate(appId)` çaðrýlýr  
3) Sonraki `GetAsync` yeni deðeri yükler

## 10. Ýzleme / Metrikler (Öneri)
- `password_validation_total`, `password_validation_error_total{code}`
- `password_hash_duration_ms`
- `password_hash_algorithm_info{algo="argon2id", memKb=..., it=..., p=...}`

## 11. Güvenlik Notlarý
- Düz metin parola saklanmaz
- Pepper gizli tutulmalý
- Sabit zamanlý karþýlaþtýrma
- Lockout uygulama katmaný

## 12. Yol Haritasý (Genel)
- Pwned Passwords kontrolü
- Blacklist geniþletme / parametrik yönetim
- `UserPasswordHistory` + son N parolanýn reddi
- Ek yönetim ekranlarý

---
## 13. Oturum Özeti (v2.2 Ek)
Bu revizyon Parametre Kayýtlarý backlog’unu listeledi; henüz yeni fonksiyon eklenmedi. Bir sonraki sürüm (v2.3) hedefi: PK-01, PK-02, PK-03 tamamlanmasý.

---
## 14. Sürüm Takibi
| Sürüm | Tarih | Ýçerik |
|-------|-------|--------|
| v2.1 | 2025-11-26 | Temel politika, provider, validator, hashing |
| v2.2 | 2025-11-28 | Backlog / ilerleme, Parametre iþleri detaylandýrýldý |


1. KODLARI HÝÇ BÝR ZAAN YAZIÞMA EDÝTÖRÜ ÞEKLÝNDE VERME.  BU KOPYALA YAPIÞTIRMADA HATAYA SEBEP OLUYOR. KOD EDÝTÖRÜNDE ÝNDENTLÝ VE RENKLÝ OLAN FORMATTA VERÝRSEN SORUN OLMUYOR. 

2. KOD VERECEÐÝN ZAMAN DAÝMA 1 TANE TAM KOD VERECEKSÝN. BEN ONU ÇAÞILTIRACAÐIM. BENÝM DONÜÞÜME GÖFE SONRAKÝ KODA GEÇECEKSÝN
3. BENÝMLE MUHAKKAK TÜRKÇE YAZIÞ LÜTFEN. ÇÜNKÜ BEN ÝNGÝLÝZCE BÝLMÝYORUM. LÜTFEN...
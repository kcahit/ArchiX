# Parola Politikası Tasarım Dokümanı (PasswordPolicy)

Revizyon: v2.2 (2025-11-28)
Önceki sürüm: v2.1 (2025-11-26)

Bu doküman v2.1 içeriğini TAM olarak korur + "Parametre Kayıtları" (ApplicationId=1) kapsamı için kalan iş kalemlerini ve ilerleme yüzdesini ekler.

---
## 0. Revizyon Notları (v2.2)
Tamamlanan çekirdek (v2.1):
- Tek JSON model (Group=Security, Key=PasswordPolicy, ParameterDataTypeId=15)
- Provider + IMemoryCache + Invalidate akışı
- Validator (uzunluk, kategori, farklı karakter, tekrar sekansı, blok liste)
- Argon2id hashing + PBKDF2-SHA512 fallback + pepperEnabled bayrağı
- Yönetim Razor Page ile JSON görüntüleme / düzenleme / doğrulama / önizleme

Parametre Kayıtları bölümü gerçekleşme oranı: ~%50

Kalan özel işler (sadece Parametre Kayıtları açısından):
1. Çoklu ApplicationId seed/migration senaryoları (varsayılan dışı politikalar).
2. Otomatik migration: Politika kaydı yoksa idempotent insert (başlangıç kontrolü).
3. Şema validation (server-side) – required alan + tür doğrulama (JSON Schema / custom).
4. allowedSymbols senkronizasyonu: UI örneği ile parametre içeriğinin tutarlılık kontrolü.
5. Audit trail: Eski / yeni JSON + kullanıcı + zaman (ayrı PasswordPolicyAudit tablosu).
6. Concurrency kontrolü: RowVersion veya ETag mantığı ile yarış durumları engelleme.
7. Versiyonlama stratejisi: version alanı için yükseltme hook’ları (future schema evolution).
8. PepperEnabled=true & ortam değişkeni yok senaryosu için kayıt düzeyinde uyarı/log.
9. Bütünlük / rollback: Geçersiz JSON kaydı durumunda önceki değeri koruma (transaction + validation önce).
10. İmza / bütünlük doğrulaması (opsiyonel): JSON üzerinde HMAC veya checksum kolon.
11. İzleme metrikleri: parametre okuma sayısı, invalidate çağrı sayısı.
12. Yönetim ekranında normalize/minify: Kaydetmeden önce satır sonu + boşluk standardizasyonu.

Önerilen kısa vadeli sırala: (1)–(3) temel; sonra (5)(6)(7); ardından (8)(9)(11); opsiyonel (10)(12).

Backlog durum etiketleri:
- PLAN: Henüz başlanmadı
- WIP: Çalışılıyor
- DONE: Tamamlandı

| ID | İş | Durum | Not |
|----|-----|-------|-----|
| PK-01 | Çoklu ApplicationId seed | PLAN | Migration + fallback lookup |
| PK-02 | Otomatik insert / idempotent migration | PLAN | Startup kontrolü |
| PK-03 | Server-side schema validation | PLAN | FluentValidation / System.Text.Json node traversal |
| PK-04 | allowedSymbols konsistens kontrolü | PLAN | UI vs parametre diff |
| PK-05 | Audit trail tablosu | PLAN | (Id, AppId, OldJson, NewJson, UserId, Utc) |
| PK-06 | Concurrency (RowVersion) | PLAN | EF Core concurrency token |
| PK-07 | Schema version upgrade hook | PLAN | Strategy class / version dispatcher |
| PK-08 | PepperEnabled env uyarısı | PLAN | Logger + health check |
| PK-09 | Rollback mekanizması | PLAN | Validation önce transaction |
| PK-10 | HMAC bütünlük | PLAN | Opsiyonel güvenlik sertleştirme |
| PK-11 | Parametre metrikleri | PLAN | Prometheus counters |
| PK-12 | Normalize/minify JSON | PLAN | Strip trailing spaces + stable ordering |

İlerleme (Parametre Kayıtları kısmi yüzdesi): 50% (çekirdek kayıt/okuma/güncelleme tamam, yukarıdaki backlog açık).

•	PK-02: Startup idempotent insert’i prod ortamlarında da doğrula (loglar ve tek kayıt).
•	PK-06: Concurrency testi (iki istemciyle aynı kaydı değiştirme → çakışma mesajı).
•	PK-08: ARCHIX_PEPPER ayarla ve uyarının kaybolduğunu kontrol et.
•	PK-01: Çoklu ApplicationId için seed stratejisi (liste üzerinden başlangıç politikaları).
•	PK-07: version alanı için upgrade hook taslağı (v1→v2 dönüştürücü).

---
## 1. Amaç / Kapsam
Parola güvenliği, veritabanındaki JSON parametreleri ile yönetilir; değişiklik için deploy gerekmez. Uygulama, `Parameters` tablosundan politikayı okur ve bellekte önbellekler.

## 2. Parametre Kayıtları (ApplicationId=1)
- Parola politikası (tekleştirilmiş model):
  - Group: `Security`, Key: `PasswordPolicy`, ParameterDataTypeId: `15 (Json)`
- İkili doğrulama (bilgi amaçlı):
  - Group: `TwoFactor`, Key: `Options`, varsayılan `Value`: `{"defaultChannel":"Sms"}`

Not: Eski “Group=PasswordPolicy / Key=Options,Argon2” yaklaşımı yerine tek JSON altında `hash` bölümü bulunan yapı kullanılır.

## 3. PasswordPolicy JSON Şeması (Kesin)
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

## 4. DTO Sınıfları (Gerçek Kod)
```json
{ "version": 1, "minLength": 12, "maxLength": 128, "requireUpper": true, "requireLower": true, "requireDigit": true, "requireSymbol": true, "allowedSymbols": "!@#$%^&*_-+=:?.,;", "minDistinctChars": 5, "maxRepeatedSequence": 3, "blockList": ["password", "123456", "qwerty", "admin"], "historyCount": 10, "lockoutThreshold": 5, "lockoutSeconds": 900, "hash": { "algorithm": "Argon2id", "memoryKb": 65536, "parallelism": 2, "iterations": 3, "saltLength": 16, "hashLength": 32, "fallback": { "algorithm": "PBKDF2-SHA512", "iterations": 210000 }, "pepperEnabled": false } }
```

## 5. Sağlayıcı (Provider) ve Önbellek
Gerçek arayüz:
```csharp
public interface IPasswordPolicyProvider
{
    ValueTask<PasswordPolicyOptions> GetAsync(int applicationId = 1, CancellationToken ct = default);
    void Invalidate(int applicationId = 1);
}
```
Davranış:
- `GetAsync` ilk çağrıda DB’den okur, `IMemoryCache` ile önbelleğe alır.
- Politika güncellendiğinde `Invalidate(appId)` çağrılır; sonraki `GetAsync` yeniden yükler.

## 6. Doğrulama (Validator)
- İmza: `IReadOnlyList<string> Validate(string password, PasswordPolicyOptions policy)`
- Hata kodları: `EMPTY`, `MIN_LENGTH`, `MAX_LENGTH`, `REQ_UPPER`, `REQ_LOWER`, `REQ_DIGIT`, `REQ_SYMBOL`, `MIN_DISTINCT`, `REPEAT_SEQ`, `BLOCK_LIST`
- Sıra: uzunluk ? kategori kontrolleri ? ayırt edici karakter ? tekrar sekansı ? blok liste
- Not: HIBP/Pwned ve yaş/geçmiş kontrolleri yol haritasındadır.

## 7. Hashleme
- Algoritma: Argon2id (Isopoh.Cryptography.Argon2)
- Çıktı: Standart Argon2 encoded string
- Pepper: `ARCHIX_PEPPER` ortam değişkeni (pepperEnabled true ise eklenir)
- Fallback: PBKDF2-SHA512

## 8. Seed / Migration
- Relational DB: Migration örn. TwoFactorDefaultChannelSms.
- InMemory: `HasData` tohumları.

## 9. Güncelleme Akışı
1) `Parameters.Value` güncellenir  
2) `IPasswordPolicyProvider.Invalidate(appId)` çağrılır  
3) Sonraki `GetAsync` yeni değeri yükler

## 10. İzleme / Metrikler (Öneri)
- `password_validation_total`, `password_validation_error_total{code}`
- `password_hash_duration_ms`
- `password_hash_algorithm_info{algo="argon2id", memKb=..., it=..., p=...}`

## 11. Güvenlik Notları
- Düz metin parola saklanmaz
- Pepper gizli tutulmalı
- Sabit zamanlı karşılaştırma
- Lockout uygulama katmanı

## 12. Yol Haritası (Genel)
- Pwned Passwords kontrolü
- Blacklist genişletme / parametrik yönetim
- `UserPasswordHistory` + son N parolanın reddi
- Ek yönetim ekranları

---
## 13. Oturum Özeti (v2.2 Ek)
Bu revizyon Parametre Kayıtları backlog’unu listeledi; henüz yeni fonksiyon eklenmedi. Bir sonraki sürüm (v2.3) hedefi: PK-01, PK-02, PK-03 tamamlanması.

---
## 14. Sürüm Takibi
| Sürüm | Tarih | İçerik |
|-------|-------|--------|
| v2.1 | 2025-11-26 | Temel politika, provider, validator, hashing |
| v2.2 | 2025-11-28 | Backlog / ilerleme, Parametre işleri detaylandırıldı |


1. KODLARI HİÇ BİR ZAAN YAZIŞMA EDİTÖRÜ ŞEKLİNDE VERME.  BU KOPYALA YAPIŞTIRMADA HATAYA SEBEP OLUYOR. KOD EDİTÖRÜNDE İNDENTLİ VE RENKLİ OLAN FORMATTA VERİRSEN SORUN OLMUYOR. 

2. KOD VERECEĞİN ZAMAN DAİMA 1 TANE TAM KOD VERECEKSİN. BEN ONU ÇAŞILTIRACAĞIM. BENİM DONÜŞÜME GÖFE SONRAKİ KODA GEÇECEKSİN
3. BENİMLE MUHAKKAK TÜRKÇE YAZIŞ LÜTFEN. ÇÜNKÜ BEN İNGİLİZCE BİLMİYORUM. LÜTFEN...
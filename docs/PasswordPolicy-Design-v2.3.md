# Parola Politikası Tasarım Dokümanı (PasswordPolicy)

Revizyon: v2.3 (2025-11-29)
Önceki sürüm: v2.2 (2025-11-28)

Bu doküman v2.2 içeriğini TAM olarak korur + PK-01 - PK-12 işlerinin tamamlanma durumunu günceller.

---
## 0. Revizyon Notları (v2.3)

### Tamamlanan Çekirdek (v2.1'den devam)
- Tek JSON model (Group=Security, Key=PasswordPolicy, ParameterDataTypeId=15)
- Provider + IMemoryCache + Invalidate akışı
- Validator (uzunluk, kategori, farklı karakter, tekrar sekansı, blok liste)
- Argon2id hashing + PBKDF2-SHA512 fallback + pepperEnabled bayrağı
- Yönetim Razor Page ile JSON görüntüleme / düzenleme / doğrulama / önizleme

### v2.3 Yeni Tamamlananlar (2025-11-29)
**Parametre Kayıtları bölümü gerçekleşme oranı: %100 ✅**

Tüm PK-01 → PK-12 işleri tamamlandı:
- **PK-01**: Çoklu ApplicationId seed stratejisi
- **PK-02**: Startup idempotent insert
- **PK-03**: Server-side schema validation
- **PK-04**: allowedSymbols konsistens kontrolü
- **PK-05**: Audit trail tablosu (PasswordPolicyAudit entity, migration)
- **PK-06**: Concurrency kontrolü (RowVersion)
- **PK-07**: Version upgrade hook
- **PK-08**: PepperEnabled uyarısı
- **PK-09**: Rollback mekanizması (zaten UpdateAsync içinde mevcut)
- **PK-10**: HMAC bütünlük kontrolü
- **PK-11**: İzleme metrikleri
- **PK-12**: Normalize/minify JSON

### Eklenen Dosyalar (v2.3)
**Kaynak Kodlar:**
- PasswordPolicyStartup.cs
- PasswordPolicyMultiAppSeed.cs
- PasswordPolicySchemaValidator.cs
- PasswordPolicySymbolsConsistencyChecker.cs
- PasswordPolicyVersionUpgrader.cs
- IPasswordPolicyVersionUpgrader.cs
- PasswordPolicyIntegrityChecker.cs
- PasswordPolicyMetrics.cs

**Test Dosyaları:**
- PasswordPolicyStartupTests.cs
- PasswordPolicyMultiAppSeedTests.cs
- PasswordPolicySchemaValidatorTests.cs
- PasswordPolicySymbolsConsistencyCheckerTests.cs
- PasswordPolicyVersionUpgraderTests.cs
- PasswordPolicyIntegrityCheckerTests.cs
- PasswordPolicyMetricsTests.cs
- JsonTextFormatterTests.cs
- PasswordPolicyConcurrencyTests.cs

**Toplam: 10 test dosyası, tüm testler geçti ✅**

### Backlog Durum Güncellemesi

| ID | İş | Durum | Açıklama |
|----|-----|-------|----------|
| PK-01 | Çoklu ApplicationId seed | ✅ DONE | Multi-app seed stratejisi |
| PK-02 | Otomatik insert / idempotent migration | ✅ DONE | Startup kontrolü |
| PK-03 | Server-side schema validation | ✅ DONE | Required alan + tür doğrulama |
| PK-04 | allowedSymbols konsistens kontrolü | ✅ DONE | UI vs parametre diff |
| PK-05 | Audit trail tablosu | ✅ DONE | OldJson, NewJson, UserId |
| PK-06 | Concurrency (RowVersion) | ✅ DONE | EF Core concurrency token |
| PK-07 | Schema version upgrade hook | ✅ DONE | v1→v2 dönüştürücü |
| PK-08 | PepperEnabled env uyarısı | ✅ DONE | Logger + warning |
| PK-09 | Rollback mekanizması | ✅ DONE | Validation + transaction |
| PK-10 | HMAC bütünlük | ✅ DONE | HMAC-SHA256 imza |
| PK-11 | Parametre metrikleri | ✅ DONE | OpenTelemetry counters |
| PK-12 | Normalize/minify JSON | ✅ DONE | JsonTextFormatter.Minify |

---
## 1. Amaç / Kapsam
Parola güvenliği, veritabanındaki JSON parametreleri ile yönetilir; değişiklik için deploy gerekmez. Uygulama, Parameters tablosundan politikayı okur ve bellekte önbellekler.

## 2. Parametre Kayıtları (ApplicationId=1)
- Parola politikası (tekleştirilmiş model):
  - Group: Security, Key: PasswordPolicy, ParameterDataTypeId: 15 (Json)
- İkili doğrulama (bilgi amaçlı):
  - Group: TwoFactor, Key: Options, varsayılan Value: {"defaultChannel":"Sms"}

Not: Eski "Group=PasswordPolicy / Key=Options,Argon2" yaklaşımı yerine tek JSON altında hash bölümü bulunan yapı kullanılır.

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
- blockList: string array
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
  - fallback: object (algorithm: "PBKDF2-SHA512", iterations: number)
  - pepperEnabled: boolean

Örnek Value yapısı:
<!-- 
{
  "version": 1,
  "minLength": 12,
  "maxLength": 128,
  "requireUpper": true,
  "requireLower": true,
  "requireDigit": true,
  "requireSymbol": true,
  "allowedSymbols": "!@#$%^&*_-+=:?.,;",
  "minDistinctChars": 5,
  "maxRepeatedSequence": 3,
  "blockList": ["password", "123456", "qwerty", "admin"],
  "historyCount": 10,
  "lockoutThreshold": 5,
  "lockoutSeconds": 900,
  "hash": {
    "algorithm": "Argon2id",
    "memoryKb": 65536,
    "parallelism": 2,
    "iterations": 3,
    "saltLength": 16,
    "hashLength": 32,
    "fallback": {
      "algorithm": "PBKDF2-SHA512",
      "iterations": 210000
    },
    "pepperEnabled": false
  }
}
-->

## 4. DTO Sınıfları
PasswordPolicyOptions modeli tüm yukarıdaki alanları içerir. JSON deserialize edilerek kullanılır.

## 5. Sağlayıcı (Provider) ve Önbellek
Gerçek arayüz:
<!--
public interface IPasswordPolicyProvider
{
    ValueTask<PasswordPolicyOptions> GetAsync(int applicationId = 1, CancellationToken ct = default);
    void Invalidate(int applicationId = 1);
}
-->

Davranış:
- GetAsync ilk çağrıda DB'den okur, IMemoryCache ile önbelleğe alır.
- Politika güncellendiğinde Invalidate(appId) çağrılır; sonraki GetAsync yeniden yükler.

## 6. Doğrulama (Validator)
- İmza: IReadOnlyList<string> Validate(string password, PasswordPolicyOptions policy)
- Hata kodları: EMPTY, MIN_LENGTH, MAX_LENGTH, REQ_UPPER, REQ_LOWER, REQ_DIGIT, REQ_SYMBOL, MIN_DISTINCT, REPEAT_SEQ, BLOCK_LIST
- Sıra: uzunluk → kategori kontrolleri → ayırt edici karakter → tekrar sekansı → blok liste
- Not: HIBP/Pwned ve yaş/geçmiş kontrolleri yol haritasındadır.

## 7. Hashleme
- Algoritma: Argon2id (Isopoh.Cryptography.Argon2)
- Çıktı: Standart Argon2 encoded string
- Pepper: ARCHIX_PEPPER ortam değişkeni (pepperEnabled true ise eklenir)
- Fallback: PBKDF2-SHA512

## 8. Seed / Migration
- Relational DB: Migration ile (örn. TwoFactorDefaultChannelSms)
- InMemory: HasData tohumları

## 9. Güncelleme Akışı
1. Parameters.Value güncellenir  
2. IPasswordPolicyProvider.Invalidate(appId) çağrılır  
3. Sonraki GetAsync yeni değeri yükler

## 10. İzleme / Metrikler
OpenTelemetry uyumlu metrikler:
- password_policy.read.total (app_id, from_cache)
- password_policy.invalidate.total (app_id)
- password_policy.update.total (app_id, success)
- password_policy.validation_error.total (app_id, error_type)

## 11. Güvenlik Notları
- Düz metin parola saklanmaz
- Pepper gizli tutulmalı
- Sabit zamanlı karşılaştırma (CryptographicOperations.FixedTimeEquals)
- Lockout uygulama katmanında

## 12. Yol Haritası (Genel)
- Pwned Passwords kontrolü (k-anonymity)
- Blacklist genişletme / parametrik yönetim
- UserPasswordHistory + son N parolanın reddi
- Ek yönetim ekranları

---
## 13. Oturum Özeti (v2.3)
Bu revizyon tüm Parametre Kayıtları backlog'unu tamamladı (%100).

Öne çıkanlar:
- Çoklu ApplicationId desteği
- Schema validation (required alan + tür kontrolü)
- Concurrency kontrolü (RowVersion)
- Version upgrade stratejisi (v1→v2)
- HMAC bütünlük doğrulaması (ARCHIX_POLICY_HMAC_KEY)
- Metrics (OpenTelemetry uyumlu)
- Startup idempotent insert
- Pepper uyarı mekanizması

---
## 15. Kalan İşler (Backlog - Gelecek Sürümler)

Parametre Kayıtları bölümü tamamlandı (%100). Aşağıda genel yol haritasındaki kalan işler listelenmiştir:
güncelleme tarihi: 2025-12-05

| ID | İş | Öncelik | Açıklama | Tahmini Süre |
|----|-----|---------|----------|--------------|
| RL-01 | Pwned Passwords kontrolü | Yüksek | HIBP API entegrasyonu (k-anonymity, prefix cache) | 2-3 gün |
| RL-02 | UserPasswordHistory tablosu | Yüksek | Son N parolanın saklanması ve kontrolü | 1-2 gün |
| RL-03 | Blacklist genişletme | Orta | Parametrik blacklist yönetimi (admin UI) | 1 gün |
| RL-04 | Parola yaşlandırma | Orta | maxPasswordAgeDays kontrolü ve zorunlu değişim | 1-2 gün |
| RL-05 | Yönetim UI genişletme | Orta | Policy görüntüle/güncelle Razor Page iyileştirmeleri | 2 gün |
| RL-06 | History temizleme job'ı | Düşük | Eski history kayıtlarını otomatik temizleme | 0.5 gün |
| RL-07 | Entropy kontrolü | Düşük | Parola karmaşıklığı skoru hesaplama | 1 gün |
| RL-08 | Dictionary attack koruması | Düşük | Yaygın kelime sözlüğü kontrolü | 1 gün |
| RL-09 | Rate limiting | Orta | Parola değişim/deneme hız sınırlama | 1 gün |
| RL-10 | Çoklu dil desteği | Düşük | Hata mesajlarında çoklu dil | 0.5 gün |

**Toplam Tahmini:** ~12-15 gün

**Bir sonraki sprint önerisi:** RL-01, RL-02, RL-04 (kritik güvenlik özellikleri)

---
## 16. Sürüm Takibi
| Sürüm | Tarih | İçerik |
|-------|-------|--------|
| v2.1 | 2025-11-26 | Temel politika, provider, validator, hashing |
| v2.2 | 2025-11-28 | Backlog / ilerleme, Parametre işleri detaylandırıldı |
| v2.3 | 2025-11-29 | PK-01 - PK-12 tamamlandı (%100), 10 test dosyası eklendi |
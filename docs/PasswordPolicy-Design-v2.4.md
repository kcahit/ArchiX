# Parola Politikası Tasarım Dokümanı (PasswordPolicy)

Revizyon: v2.4 (2025-12-05)
Önceki sürüm: v2.3 (2025-11-29)

Bu doküman v2.3 içeriğini TAM olarak korur + RL-01, RL-02 işlerinin tamamlanma durumunu günceller.

---
## 0. Revizyon Notları (v2.4)

### Tamamlanan Çekirdek (v2.1'den devam)
- Tek JSON model (Group=Security, Key=PasswordPolicy, ParameterDataTypeId=15)
- Provider + IMemoryCache + Invalidate akışı
- Validator (uzunluk, kategori, farklı karakter, tekrar sekansı, blok liste)
- Argon2id hashing + PBKDF2-SHA512 fallback + pepperEnabled bayrağı
- Yönetim Razor Page ile JSON görüntüleme / düzenleme / doğrulama / önizleme

### v2.3 Tamamlananlar (2025-11-29)
**Parametre Kayıtları bölümü: %100 ✅**

Tüm PK-01 → PK-12 işleri tamamlandı:
- **PK-01**: Çoklu ApplicationId seed stratejisi
- **PK-02**: Startup idempotent insert
- **PK-03**: Server-side schema validation
- **PK-04**: allowedSymbols konsistens kontrolü
- **PK-05**: Audit trail tablosu (PasswordPolicyAudit entity, migration)
- **PK-06**: Concurrency kontrolü (RowVersion)
- **PK-07**: Version upgrade hook
- **PK-08**: PepperEnabled uyarısı
- **PK-09**: Rollback mekanizması
- **PK-10**: HMAC bütünlük kontrolü
- **PK-11**: İzleme metrikleri
- **PK-12**: Normalize/minify JSON

### v2.4 Yeni Tamamlananlar (2025-12-05)
**Runtime Logic bölümü: RL-01, RL-02 ✅**

Tamamlanan işler:
- **RL-01**: Pwned Passwords kontrolü (HIBP API + k-anonymity + prefix cache)
- **RL-02**: UserPasswordHistory tablosu (son N parolanın saklanması ve kontrolü)

### Eklenen Dosyalar (v2.4)

**Kaynak Kodlar (RL-01):**
- `IPasswordPwnedChecker.cs` (Abstractions/Security)
- `PasswordPwnedChecker.cs` (Runtime/Security)
- `PasswordValidationService.cs` (Runtime/Security)
- `PasswordSecurityServiceCollectionExtensions.cs` (güncellendi - DI kaydı)

**Kaynak Kodlar (RL-02):**
- `IPasswordHistoryService.cs` (Abstractions/Security)
- `PasswordHistoryService.cs` (Runtime/Security)
- `UserPasswordHistory.cs` (Entities)
- `UserPasswordHistoryConfiguration.cs` (Entities/Configurations)
- Migration: `AddUserPasswordHistory.cs`
- Migration: `EnforceRestrictDeleteBehavior.cs`
- `ModelBuilderExtensions.cs` (güncellendi - ApplyRestrictDeleteBehavior)

**Test Dosyaları:**
- `PasswordPwnedCheckerTests.cs` (6 test - HIBP mock)
- `PasswordValidationServiceTests.cs` (11 test - entegrasyon)
- `PasswordHistoryServiceTests.cs` (6 test - history logic)

**Toplam: 3 test dosyası, 23 adet test ✅**

### Backlog Durum Güncellemesi (v2.4)

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
| **RL-01** | **Pwned Passwords kontrolü** | **✅ DONE** | **HIBP API (k-anonymity, 30dk cache)** |
| **RL-02** | **UserPasswordHistory tablosu** | **✅ DONE** | **Son N parolanın saklanması/kontrolü** |
| RL-03 | Blacklist genişletme | ⏳ TODO | Parametrik blacklist yönetimi |
| RL-04 | Parola yaşlandırma | ⏳ TODO | maxPasswordAgeDays kontrolü |
| RL-05 | Yönetim UI genişletme | ⏳ TODO | Razor Page iyileştirmeleri |
| RL-06 | History temizleme job'ı | ⏳ TODO | Otomatik temizleme |
| RL-07 | Entropy kontrolü | ⏳ TODO | Karmaşıklık skoru |
| RL-08 | Dictionary attack koruması | ⏳ TODO | Kelime sözlüğü kontrolü |
| RL-09 | Rate limiting | ⏳ TODO | Hız sınırlama |
| RL-10 | Çoklu dil desteği | ⏳ TODO | Hata mesajları i18n |

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

- İmza: `IReadOnlyList<string> Validate(string password, PasswordPolicyOptions policy)`
- Hata kodları: `EMPTY`, `MIN_LENGTH`, `MAX_LENGTH`, `REQ_UPPER`, `REQ_LOWER`, `REQ_DIGIT`, `REQ_SYMBOL`, `MIN_DISTINCT`, `REPEAT_SEQ`, `BLOCK_LIST`, **`PWNED`**, **`HISTORY`**
- Sıra: uzunluk → kategori kontrolleri → ayırt edici karakter → tekrar sekansı → blok liste → **pwned** → **history**
- **✅ v2.4:** HIBP/Pwned ve history kontrolleri artık aktif!

## 7. Hashleme
- Algoritma: Argon2id (Isopoh.Cryptography.Argon2)
- Çıktı: Standart Argon2 encoded string
- Pepper: ARCHIX_PEPPER ortam değişkeni (pepperEnabled true ise eklenir)
- Fallback: PBKDF2-SHA512

## 8. Seed / Migration
- Relational DB: Migration ile (örn. TwoFactorDefaultChannelSms)
- InMemory: HasData tohumları
- **✅ v2.4:** UserPasswordHistories tablosu eklendi

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
- **✅ v2.4:** Tüm FK'lar Restrict (audit trail korunur)

## 12. Yol Haritası (Genel)
- ~~Pwned Passwords kontrolü (k-anonymity)~~ ✅ DONE (v2.4)
- ~~UserPasswordHistory + son N parolanın reddi~~ ✅ DONE (v2.4)
- Blacklist genişletme / parametrik yönetim (RL-03)
- Ek yönetim ekranları (RL-05)

---
## 13. Oturum Özeti (v2.4)
Bu revizyon RL-01 ve RL-02 işlerini tamamladı.

Öne çıkanlar:
- **RL-01:** HIBP API entegrasyonu (k-anonymity, prefix cache 30 dakika, fail-open)
- **RL-02:** UserPasswordHistory tablosu (son N parola, otomatik limit temizleme)
- **PasswordValidationService:** Policy + Pwned + History entegrasyonu
- **Global FK Restrict Policy:** Tüm FK'lar artık Restrict (audit trail korunur)
- **23 adet test:** PasswordPwnedChecker (6), PasswordValidationService (11), PasswordHistoryService (6)

---
## 15. Kalan İşler (Backlog - Gelecek Sürümler)

Parametre Kayıtları bölümü tamamlandı (%100).  
RL-01, RL-02 tamamlandı (%100).

Aşağıda genel yol haritasındaki kalan işler listelenmiştir:

**Güncelleme tarihi: 2025-12-05**

| ID | İş | Öncelik | Durum | Açıklama | Tahmini Süre |
|----|-----|---------|-------|----------|--------------|
| RL-01 | Pwned Passwords kontrolü | Yüksek | ✅ **DONE** | HIBP API (k-anonymity, prefix cache) | ~~2-3 gün~~ |
| RL-02 | UserPasswordHistory tablosu | Yüksek | ✅ **DONE** | Son N parolanın saklanması ve kontrolü | ~~1-2 gün~~ |
| RL-03 | Blacklist genişletme | Orta | ⏳ TODO | Parametrik blacklist yönetimi (admin UI) | 1 gün |
| RL-04 | Parola yaşlandırma | Orta | ⏳ TODO | maxPasswordAgeDays kontrolü ve zorunlu değişim | 1-2 gün |
| RL-05 | Yönetim UI genişletme | Orta | ⏳ TODO | Policy görüntüle/güncelle Razor Page iyileştirmeleri | 2 gün |
| RL-06 | History temizleme job'ı | Düşük | ⏳ TODO | Eski history kayıtlarını otomatik temizleme | 0.5 gün |
| RL-07 | Entropy kontrolü | Düşük | ⏳ TODO | Parola karmaşıklığı skoru hesaplama | 1 gün |
| RL-08 | Dictionary attack koruması | Düşük | ⏳ TODO | Yaygın kelime sözlüğü kontrolü | 1 gün |
| RL-09 | Rate limiting | Orta | ⏳ TODO | Parola değişim/deneme hız sınırlama | 1 gün |
| RL-10 | Çoklu dil desteği | Düşük | ⏳ TODO | Hata mesajlarında çoklu dil | 0.5 gün |

**Kalan Toplam Tahmini:** ~8-12 gün (2 iş tamamlandı)

**Bir sonraki sprint önerisi:** RL-04 (parola yaşlandırma), RL-03 (blacklist), RL-05 (UI) (kritik güvenlik + kullanılabilirlik)

---
## 16. Sürüm Takibi
| Sürüm | Tarih | İçerik |
|-------|-------|--------|
| v2.1 | 2025-11-26 | Temel politika, provider, validator, hashing |
| v2.2 | 2025-11-28 | Backlog / ilerleme, Parametre işleri detaylandırıldı |
| v2.3 | 2025-11-29 | PK-01 - PK-12 tamamlandı (%100), 10 test dosyası eklendi |
| v2.4 | 2025-12-05 | RL-01, RL-02 tamamlandı (Pwned + History), 23 test eklendi |

---
## 17. RL-01: Pwned Passwords Detayları (v2.4)

### Özellikler:
- **k-Anonymity:** Sadece SHA-1 hash'inin ilk 5 karakteri HIBP API'ye gönderilir
- **Prefix Cache:** API sonuçları 30 dakika cache'lenir (IMemoryCache)
- **Fail-Open:** API hatası durumunda parola güvenli kabul edilir (kullanıcı deneyimi)
- **User-Agent:** `ArchiX-PasswordPolicy/1.0`

### Interface:
<!--
public interface IPasswordPwnedChecker
{
    Task<bool> IsPwnedAsync(string password, CancellationToken ct = default);
    Task<int> GetPwnedCountAsync(string password, CancellationToken ct = default);
}
-->

### Kullanım:
<!--
var isPwned = await _pwnedChecker.IsPwnedAsync("password123");
if (isPwned) 
{
    return Error("Bu parola daha önce sızdırılmış!");
}
-->

### Testler:
1. `IsPwnedAsync_ReturnsTrue_WhenPasswordIsPwned`
2. `IsPwnedAsync_ReturnsFalse_WhenPasswordIsNotPwned`
3. `GetPwnedCountAsync_ReturnsCorrectCount`
4. `GetPwnedCountAsync_ReturnsZero_WhenPasswordIsEmpty`
5. `GetPwnedCountAsync_UsesCacheOnSecondCall`
6. `GetPwnedCountAsync_ReturnsZero_OnHttpError`

---
## 18. RL-02: UserPasswordHistory Detayları (v2.4)

### Özellikler:
- **History Limit:** Son N parola saklanır (policy.HistoryCount)
- **Otomatik Temizleme:** Limit aşıldığında en eski kayıtlar silinir
- **Hash Depolama:** Argon2id hash + algoritma adı
- **Audit Trail:** BaseEntity alanları (Created, Updated, Status)

### Interface:
<!--
public interface IPasswordHistoryService
{
    Task<bool> IsPasswordInHistoryAsync(int userId, string passwordHash, int historyCount, CancellationToken ct = default);
    Task AddToHistoryAsync(int userId, string passwordHash, string algorithm, int historyCount, CancellationToken ct = default);
}
-->

### Database Şeması:
<!--
CREATE TABLE UserPasswordHistories (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    PasswordHash NVARCHAR(300) NOT NULL,
    HashAlgorithm NVARCHAR(20) NOT NULL,
    CreatedAtUtc DATETIMEOFFSET(4) NOT NULL,
    -- BaseEntity alanları
    INDEX IX_UserPasswordHistories_UserId (UserId),
    INDEX IX_UserPasswordHistories_UserId_CreatedAtUtc (UserId, CreatedAtUtc DESC)
);
-->

### Kullanım:
<!--
// Kontrol et
var inHistory = await _historyService.IsPasswordInHistoryAsync(
    userId: 1, 
    passwordHash: hash, 
    historyCount: 10);

if (inHistory)
{
    return Error("Bu parolayı son 10 seferinde kullandınız!");
}

// Ekle
await _historyService.AddToHistoryAsync(
    userId: 1, 
    passwordHash: hash, 
    algorithm: "Argon2id", 
    historyCount: 10);
-->

### Testler:
1. `IsPasswordInHistoryAsync_ReturnsFalse_WhenNoHistory`
2. `IsPasswordInHistoryAsync_ReturnsTrue_WhenPasswordExists`
3. `IsPasswordInHistoryAsync_ChecksOnlyLastNPasswords`
4. `AddToHistoryAsync_AddsNewEntry`
5. `AddToHistoryAsync_RemovesOldestWhenLimitExceeded`
6. `AddToHistoryAsync_PreservesCorrectCount`

---
## 19. PasswordValidationService (v2.4)

Tam doğrulama servisi: **Policy + Pwned + History**

### Interface:
<!--
public class PasswordValidationService
{
    public async Task<PasswordValidationResult> ValidateAsync(
        string password, 
        int userId, 
        int applicationId = 1, 
        CancellationToken ct = default);
}

public record PasswordValidationResult(bool IsValid, IReadOnlyList<string> Errors);
-->

### Akış:
1. **Policy kuralları** (senkron): MIN_LENGTH, REQ_UPPER, vb.
2. **Pwned kontrolü** (async): HIBP API sorgusu
3. **History kontrolü** (async): Son N parola karşılaştırması

### Hata Kodları:
- Policy: `EMPTY`, `MIN_LENGTH`, `MAX_LENGTH`, `REQ_UPPER`, `REQ_LOWER`, `REQ_DIGIT`, `REQ_SYMBOL`, `MIN_DISTINCT`, `REPEAT_SEQ`, `BLOCK_LIST`
- RL-01: `PWNED`
- RL-02: `HISTORY`

### Testler (11):
1. `ValidateAsync_ValidPassword_ReturnsSuccess`
2. `ValidateAsync_TooShort_ReturnsMinLengthError`
3. `ValidateAsync_MissingUppercase_ReturnsReqUpperError`
4. `ValidateAsync_Pwned_ReturnsPwnedError`
5. `ValidateAsync_InHistory_ReturnsHistoryError`
6. `ValidateAsync_HistoryCountZero_SkipsHistoryCheck`
7. `ValidateAsync_MultipleErrors_ReturnsAllErrors`
8. `ValidateAsync_PolicyErrorsStopsPwnedCheck`
9. `ValidateAsync_BlockedWord_ReturnsBlockListError`
10. `ValidateAsync_RepeatedSequence_ReturnsRepeatSeqError`
11. `ValidateAsync_CallsHashWithCorrectPolicy`

---
## 20. Global FK Restrict Policy (v2.4)

### Özellik:
Tüm foreign key'ler artık **DeleteBehavior.Restrict** kullanır (audit trail korunur).

### Implementation:
<!--
// ModelBuilderExtensions.cs
public static void ApplyRestrictDeleteBehavior(this ModelBuilder modelBuilder)
{
    foreach (var relationship in modelBuilder.Model.GetEntityTypes()
        .SelectMany(e => e.GetForeignKeys()))
    {
        relationship.DeleteBehavior = DeleteBehavior.Restrict;
    }
}

// AppDbContext.cs
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // ... diğer yapılandırmalar
    
    modelBuilder.ApplySoftDeleteFilters();
    modelBuilder.ApplyRestrictDeleteBehavior(); // ✅ EN SON!
}
-->

### Doğrulama:
<!--
-- Cascade FK olmamalı
SELECT 
    OBJECT_NAME(fk.parent_object_id) AS TableName,
    fk.name AS ForeignKeyName,
    fk.delete_referential_action_desc AS DeleteAction
FROM sys.foreign_keys AS fk
WHERE fk.delete_referential_action_desc <> 'NO_ACTION';
-- Sonuç: NULL (tüm FK'lar Restrict)
-->

---
## 21. DI Kaydı (v2.4)
<!--
public static class PasswordSecurityServiceCollectionExtensions
{
    public static IServiceCollection AddPasswordSecurity(this IServiceCollection services)
    {
        services.AddSingleton<IPasswordPolicyProvider, PasswordPolicyProvider>();
        services.AddSingleton<IPasswordHasher, Argon2PasswordHasher>();
        services.AddSingleton<IPasswordPolicyAdminService, PasswordPolicyAdminService>();
        
        // ✅ RL-01: Pwned Passwords checker (HIBP API)
        services.AddHttpClient<IPasswordPwnedChecker, PasswordPwnedChecker>();
        
        // ✅ RL-02: Password history service
        services.AddScoped<IPasswordHistoryService, PasswordHistoryService>();
        
        // ✅ Tam doğrulama servisi (policy + pwned + history)
        services.AddScoped<PasswordValidationService>();
        
        return services;
    }
}
-->

---
## Sonuç (v2.4)

**Parametre Kayıtları (PK):** %100 ✅  
**Runtime Logic (RL):** %20 ✅ (2/10 tamamlandı)

**Tamamlanan:**
- PK-01 → PK-12 (12 iş)
- RL-01: Pwned Passwords ✅
- RL-02: UserPasswordHistory ✅

**Kalan:** 2025-12-05 17:15 itibarıyla;
- RL-03 → RL-10 (8 iş)
- işlerin sırası ve önceliği aşağıdaki tabloda gösterilmiştir:
- 
Sıra	İş	Öncelik	Bağımlılık	Süre	Kümülatif
1	RL-04	🔴 Yüksek	Yok	1-2 gün	1-2 gün
2	RL-03	🟡 Orta	Yok	1 gün	2-3 gün
3	RL-05	🟡 Orta	RL-03 (opsiyonel)	2 gün	4-5 gün
4	RL-09	🟡 Orta	Yok	1 gün	5-6 gün
5	RL-06	🟢 Düşük	RL-02 ✅	0.5 gün	5.5-6.5 gün
6	RL-07	🟢 Düşük	Yok	1 gün	6.5-7.5 gün
7	RL-08	🟢 Düşük	RL-03 (opsiyonel)	1 gün	7.5-8.5 gün
8	RL-10	⚪ En Düşük	Yok	0.5 gün	8-9 gün

-- RL-04 için alınan notlar: 2025-12-05 17:20 itibarıyla aşağıdaki notlar alındı.
- UserRole eklenmeyek. Çünkü o Application bazında ayrı bir çalışma
- User class ında ApplicationId yok. UserAplication clasında one to many ilişkisi var zaten.
- Token Türü	Nerede?	Neden?
Email Doğrulama (6 haneli kod)	✅ Cache	Kısa süreli (15 dk), çoklu token
SMS Doğrulama (6 haneli kod)	✅ Cache	Kısa süreli (5 dk), çoklu token
2FA Kodu (6 haneli kod)	✅ Cache	Çok kısa süreli (3 dk)
Şifre Sıfırlama (GUID link)	✅ DB	Uzun süreli (24 saat), audit gerekir
API Token'ları	✅ DB	Kalıcı, audit gerekir

-------- 2025-12-07 12:15 RL-04 kalan işler--------
23. RL-04: Parola Yaşlandırma (Password Aging) - v2.5
Güncelleme Tarihi: 2025-12-06 10:45 (Türkiye Saati) Başlangıç Tarihi: 2025-12-06 06:05 Durum: 🚧 IN PROGRESS
---
Genel Bakış
Parolaların belirli bir süre sonra zorunlu olarak değiştirilmesini sağlayan mekanizma. MaxPasswordAgeDays parametresi (PasswordPolicy JSON) ve User.PasswordChangedAtUtc kolonu üzerinden yönetilir.
---
1. Entity Değişiklikleri
1.1 User Entity - Yeni Kolon
Dosya: src/ArchiX.Library/Entities/User.cs
Eklenecek property:
•	PasswordChangedAtUtc (DateTimeOffset? - nullable)
•	Column attribute: datetimeoffset(4)
•	Açıklama: Parolanın son değiştirilme tarihi (UTC). NULL = hiç değiştirilmemiş (süresi dolmaz)
1.2 PasswordPolicyOptions - Yeni Property
Dosya: src/ArchiX.Library/Options/PasswordPolicyOptions.cs
✅ DONE: Property zaten eklendi
•	MaxPasswordAgeDays (int? - nullable)
•	Açıklama: Parolanın maksimum yaşı (gün cinsinden). NULL = unlimited (süresi dolmaz)
---
2. Database Migration
Dosya: src/ArchiX.Library/Migrations/20251206_AddPasswordChangedAtUtcToUser.cs
Migration içeriği:
•	AddColumn metodu: PasswordChangedAtUtc, Users tablosu, datetimeoffset(4) türü, nullable, comment eklenecek
•	CreateIndex metodu: IX_Users_PasswordChangedAtUtc, Users tablosu, PasswordChangedAtUtc kolonu
---
3. Interface Tasarımı
Dosya: src/ArchiX.Library/Abstractions/Security/IPasswordExpirationService.cs
Interface adı: IPasswordExpirationService
Metot imzaları:
•	IsExpired(User user, PasswordPolicyOptions policy, DateTimeOffset? now = null) → bool
•	GetDaysUntilExpiration(User user, PasswordPolicyOptions policy, DateTimeOffset? now = null) → int?
•	GetExpirationDate(User user, PasswordPolicyOptions policy) → DateTimeOffset?
---
4. Service Implementasyonu
Dosya: src/ArchiX.Library/Runtime/Security/PasswordExpirationService.cs
Sınıf adı: PasswordExpirationService Interface: IPasswordExpirationService
Mantık:
•	MaxPasswordAgeDays null ise → unlimited (false döndür)
•	PasswordChangedAtUtc null ise → hiç değişmedi (false döndür)
•	MaxPasswordAgeDays sıfır veya negatif ise → InvalidOperationException fırla
•	PasswordChangedAtUtc + MaxPasswordAgeDays < Now ise → süresi dolmuş (true döndür)
Metodlar:
•	IsExpired: Parolanın süresi dolup dolmadığını kontrol eder
•	GetDaysUntilExpiration: Kalan gün sayısını hesaplar, null ise süresi dolmuş
•	GetExpirationDate: Expiration tarihini hesaplar
---
5. PasswordValidationService Güncellemesi
Dosya: src/ArchiX.Library/Runtime/Security/PasswordValidationService.cs
Güncellemeler:
•	IPasswordExpirationService inject edilecek (constructor'a eklenecek)
•	ValidateAsync metotu güncellenecek
•	Yeni hata kodu: EXPIRED
Akış sırası:
1.	Policy kuralları (senkron) → EMPTY, MIN_LENGTH, MAX_LENGTH, REQ_UPPER, vb.
2.	Parola Yaşlandırma Kontrolü (senkron) → IsExpired(user, policy) çağrısı → EXPIRED hata kodu
3.	Pwned kontrolü (async) → HIBP API sorgusu → PWNED hata kodu
4.	History kontrolü (async) → Son N parola → HISTORY hata kodu
---
6. DI Kaydı
Dosya: src/ArchiX.Library/Extensions/PasswordSecurityServiceCollectionExtensions.cs
Eklenecek kayıt:
•	services.AddScoped<IPasswordExpirationService, PasswordExpirationService>()
Yer: AddPasswordSecurity metotu içinde, diğer kayıtlardan sonra
---
7. Unit Test Tasarımı
Dosya: tests/ArchiX.Library.Tests/SecurityTests/PasswordExpirationServiceTests.cs
Test sınıfı adı: PasswordExpirationServiceTests
12 test senaryosu:
IsExpired metodu testleri:
1.	IsExpired_ReturnsFalse_WhenMaxAgeDaysIsNull
2.	IsExpired_ReturnsFalse_WhenPasswordChangedAtUtcIsNull
3.	IsExpired_ReturnsFalse_WhenPasswordIsStillValid
4.	IsExpired_ReturnsTrue_WhenPasswordIsExpired
5.	IsExpired_ReturnsTrue_WhenPasswordJustExpired
6.	IsExpired_ThrowsException_WhenMaxAgeDaysInvalid
GetDaysUntilExpiration metodu testleri: 7. GetDaysUntilExpiration_ReturnsNull_WhenMaxAgeDaysIsNull 8. GetDaysUntilExpiration_ReturnsCorrectValue 9. GetDaysUntilExpiration_ReturnsZero_WhenExpired
GetExpirationDate metodu testleri: 10. GetExpirationDate_ReturnsCorrectDate 11. GetExpirationDate_ReturnsNull_WhenPolicyNull 12. GetExpirationDate_ReturnsNull_WhenPasswordChangedAtUtcNull
---
8. Kenar Durumlar ve Validasyonlar
MaxPasswordAgeDays = null → Unlimited (süresi dolmaz) MaxPasswordAgeDays = 0 → InvalidOperationException fırla (geçersiz policy) MaxPasswordAgeDays < 0 → InvalidOperationException fırla (geçersiz policy) PasswordChangedAtUtc = null → Not Expired (false) - hiç değişmedi PasswordChangedAtUtc + MaxDays < Now → Expired (true) - doğru davranış
---
9. Yapılacaklar (Sıralı)
İş 1: User entity güncelle
•	Dosya: User.cs
•	Durum: ⏳ TODO
İş 2: Migration oluştur
•	Dosya: 20251206_AddPasswordChangedAtUtcToUser.cs
•	Durum: ⏳ TODO
İş 3: Interface oluştur
•	Dosya: IPasswordExpirationService.cs
•	Durum: ⏳ TODO
İş 4: Service uygula
•	Dosya: PasswordExpirationService.cs
•	Durum: ⏳ TODO
İş 5: PasswordValidationService güncelle
•	Dosya: PasswordValidationService.cs
•	Durum: ⏳ TODO
İş 6: DI kaydı ekle
•	Dosya: PasswordSecurityServiceCollectionExtensions.cs
•	Durum: ⏳ TODO
İş 7: Unit testler yaz
•	Dosya: PasswordExpirationServiceTests.cs
•	Durum: ⏳ TODO
---
10. Özet
✅ Tamamlanan:
•	PasswordPolicyOptions'a MaxPasswordAgeDays eklendi
•	TwoFactor default channel Email'e değiştirildi
⏳ Yapılacak: 7 iş
Tahmini Süre: 1-2 gün
---
Doküman Hazırlama Tarihi: 2025-12-06 10:45 (Türkiye Saati)
-------- 2025-12-07 12:15 RL-04 kalan işler notları bitti--------


-------- 2025-12-11 09:17 RL-04 kalan işler--------

GERÇEK DURUM (Doğru Değerlendirme - Kod İncelendikten Sonra)
✅ TAMAMLANDı:
1.	User.cs - PasswordChangedAtUtc ve MaxPasswordAgeDays property'leri var (✅)
2.	PasswordPolicyOptions.cs - MaxPasswordAgeDays property var (✅)
3.	Migration - 20251206051831_AddPasswordAgingToUser.cs oluşturulmuş (✅)
4.	IPasswordExpirationService - Interface tanımlandı (✅)
5.	PasswordExpirationService - Service implementasyonu yapılmış (✅)
 
❌ TAMAMLANMAMIŞ (Kalan 3 İş):
1.	DI Kaydında IPasswordExpirationService eksik
•	PasswordSecurityServiceCollectionExtensions.cs'de AddScoped<IPasswordExpirationService, PasswordExpirationService>() YOK
2.	PasswordValidationService EXPIRED kontrolü YOK
•	Constructor'da IPasswordExpirationService inject edilmemiş
•	ValidateAsync'de akış sırası yanlış: Şu anki = Policy → Pwned → History
•	Olması gereken = Policy → Expiration → Pwned → History
3.	PasswordExpirationServiceTests.cs dosyası hiç yok - 12 test yazılmamış


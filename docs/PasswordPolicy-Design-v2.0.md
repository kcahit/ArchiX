# Parola Politikas� Tasar�m Dok�man� (PasswordPolicy)

Revizyon: v2.0 (2025-11-25)

## 1. Ama� / Kapsam
Parola g�venli�i veritaban�ndaki JSON parametreleri ile y�netilir; de�i�iklik i�in deploy gerekmez.

## 2. Parametre Kay�tlar� (ApplicationId=1)
- Group="PasswordPolicy", Key="Options" (parola kurallar�)
- Group="PasswordPolicy", Key="Argon2" (hash parametreleri)
ParameterDataTypeId=15 (Json).

## 3. Options JSON Alanlar� (Kesin)
minLength=10
minUppercase=1
minLowercase=1
minDigits=1
minSpecial=1
disallowSequentialLettersCount=3 (>=3 aktif)
disallowSequentialDigitsCount=3 (>=3 aktif)
disallowRepeatedCharCount=3 (>=3 aktif)
passwordHistoryCount=3 (>0 aktif)
maxPasswordAgeDays=0 (>0 aktif)
enabledBlacklist=true
enabledPwnedCheck=true
pwnedPrefixCacheMinutes=30 (<=0 ?30)
throttleMillis=0 (>0 gecikme)

�rnek Options Value:
```json
{
  "minLength": 10,
  "minUppercase": 1,
  "minLowercase": 1,
  "minDigits": 1,
  "minSpecial": 1,
  "disallowSequentialLettersCount": 3,
  "disallowSequentialDigitsCount": 3,
  "disallowRepeatedCharCount": 3,
  "passwordHistoryCount": 3,
  "maxPasswordAgeDays": 0,
  "enabledBlacklist": true,
  "enabledPwnedCheck": true,
  "pwnedPrefixCacheMinutes": 30,
  "throttleMillis": 0
}
```

## 4. Argon2 JSON Alanlar�
memoryKB=32768
iterations=3
parallelism=2
saltLength=16
hashLength=32

�rnek Argon2 Value:
```json
{
  "memoryKB": 32768,
  "iterations": 3,
  "parallelism": 2,
  "saltLength": 16,
  "hashLength": 32
}
```

## 5. DTO S�n�flar�
```csharp
public sealed class PasswordPolicyOptions {
    public int MinLength { get; init; } = 10;
    public int MinUppercase { get; init; } = 1;
    public int MinLowercase { get; init; } = 1;
    public int MinDigits { get; init; } = 1;
    public int MinSpecial { get; init; } = 1;
    public int DisallowSequentialLettersCount { get; init; } = 3;
    public int DisallowSequentialDigitsCount { get; init; } = 3;
    public int DisallowRepeatedCharCount { get; init; } = 3;
    public int PasswordHistoryCount { get; init; } = 3;
    public int MaxPasswordAgeDays { get; init; } = 0;
    public bool EnabledBlacklist { get; init; } = true;
    public bool EnabledPwnedCheck { get; init; } = true;
    public int PwnedPrefixCacheMinutes { get; init; } = 30;
    public int ThrottleMillis { get; init; } = 0;
    public bool IsPasswordAgingEnabled => MaxPasswordAgeDays > 0;
    public bool IsHistoryEnabled => PasswordHistoryCount > 0;
    public bool IsSequentialLetterCheckEnabled => DisallowSequentialLettersCount >= 3;
    public bool IsSequentialDigitCheckEnabled => DisallowSequentialDigitsCount >= 3;
    public bool IsRepeatedCharCheckEnabled => DisallowRepeatedCharCount >= 3;
}
public sealed class Argon2Options {
    public int MemoryKB { get; init; } = 32768;
    public int Iterations { get; init; } = 3;
    public int Parallelism { get; init; } = 2;
    public int SaltLength { get; init; } = 16;
    public int HashLength { get; init; } = 32;
}
```

## 6. UserPasswordHistory
Alanlar: UserId (FK), PasswordHash (nvarchar(300)), HashAlgorithm (nvarchar(20)="Argon2id"). Salt format i�inde. passwordHistoryCount limiti a��l�rsa en eski kay�t silinir.

## 7. Aray�zler
```csharp
public interface IPasswordPolicyProvider {
    PasswordPolicyOptions Current { get; }
    Argon2Options Argon2 { get; }
    Task RefreshAsync(CancellationToken ct = default);
}
public interface IPasswordValidator {
    Task<PasswordValidationResult> ValidateAsync(int userId, string password, CancellationToken ct = default);
}
public sealed record PasswordValidationResult(bool Success, IReadOnlyList<string> Errors);
```

## 8. Do�rulama Hata Kodlar�
MIN_LENGTH, CATEGORY_UPPER, CATEGORY_LOWER, CATEGORY_DIGIT, CATEGORY_SPECIAL, SEQUENTIAL, REPEATED, BLACKLIST, PWNED, HISTORY, EXPIRED.

## 9. Do�rulama S�ras�
Uzunluk ? kategoriler ? ard���k ? tekrar ? blacklist ? pwned ? history ? ya� ? throttle.

## 10. Hashleme
Argon2id. Format: `$argon2id$v=19$m=<memoryKB>,t=<iterations>,p=<parallelism>$<salt>$<hash>` nvarchar(300).

## 11. Blacklist
enabledBlacklist=true ? HashSet kontrol; e�le�me BLACKLIST.

## 12. Pwned
enabledPwnedCheck=true ? SHA-1 ilk 5 hex prefix sorgulan�r; tam e�le�me PWNED. Prefix sonu�lar� pwnedPrefixCacheMinutes s�re cache.

## 13. Migration / Seed
Migration: UserPasswordHistory tablo + Options ve Argon2 parametreleri yoksa seed.

## 14. G�ncelleme
Parameter.Value de�i�ince politika an�nda ge�erli. 0 veya false kontrolleri kapat�r.

## 15. �zleme
Metrikler: password_validation_total, password_validation_error_total (errorCode), password_hash_duration_ms, hibp_queries_total.

## 16. Performans
Ba�lang�� Argon2: memoryKB=32768, iterations=3, parallelism=2. Yo�unlukta ge�ici iterations=2 uygulanabilir.

## 17. Kesin Kararlar
Hash: yaln�z Argon2id.
minSpecial=1 zorunlu.
Entropy kontrol� yok.
History / ya� >0 de�erlerde aktif.
Blacklist ve Pwned varsay�lan true.
Pepper ENV secret.
Throttle varsay�lan 0.

## 18. G�venlik
Parola d�z metin saklanmaz. TLS zorunlu. AttemptLimiter aktif. Parametreler Application baz�nda farkl�la�t�r�labilir.

Belge kesin kurallar� i�erir; �neri yoktur.


## 50. İşe Başlama Kural ve Adımları
Adım planı önerisi (onay sonrası her adımı ayrı ele alıp test + commit yaparız):
1.	Parametre/Policy Tanımı
•	Parameter veya yeni SecurityPolicy tablosunda parola politikası (MinLength, RequireUpper, RequireLower, RequireDigit, RequireSymbol, AllowedSymbols, MaxRepeatedChars, HistoryCount, LockoutThreshold, LockoutSeconds, HashAlgorithm, HashIterations, PepperEnabled) saklama modeli.
•	Versiyonlama + etkin/pasif alanı.
2.	Domain Model & DTO
•	Policy entity + read model (UI/Razor Page için).
•	Validation attribute / service: PasswordPolicyValidator.
3.	Hash & Saklama Altyapısı
•	Seçenek: Argon2id (tercih) + geri düşme PBKDF2.
•	Pepper yönetimi (IOptions + Azure KeyVault opsiyonunu sonraya bırakabiliriz).
•	Abstraction: IPasswordHasher (genişletilmiş) + IPasswordPolicyService.
4.	Kullanıcı Tablosu Genişletme
•	Parola hash kolonları: PasswordHash, PasswordSalt (Argon2 için ayrı salt saklanmayabilir; yine de extensible), PasswordCreatedAt.
•	Lockout alanları: FailedCount, LockoutUntil.
•	PasswordHistory tablosu (UserId, Hash, CreatedAt).
5.	Validasyon Akışı
•	Register/Reset/Change password flow’larında policy çek → PasswordPolicyValidator çalıştır → geçerse hashle → history kaydet → kullanıcı güncelle.
6.	Razor Pages Entegrasyonu
•	Sayfalar: Policy Görüntüle/Güncelle (admin), Parola Değiştir, Parola Sıfırla.
•	Client-side quick hints (min length, gereklilikler).
7.	Testler
•	Unit: Validator edge cases.
•	Unit: Hasher (aynı parola = farklı salt; doğrulama başarılı).
•	Unit: History (son N eski parolayı reddet).
•	Integration: Register + change + lockout senaryoları.
8.	Güvenlik Sertleştirme
•	Timing attack azaltma (sabit süreli karşılaştırma).
•	Lockout & exponential backoff.
•	Pepper yükleme hatalarında güvenli davranış.
9.	Konfigürasyon & Parameter Data
•	İlk seed için varsayılan politika eklenmesi.
•	Policy güncellenince kullanıcı işlemlerinde yeni kuralların geçmesi.
10.	Loglama & Audit
•	Başarısız denemelerde audit kaydı (ConnectionAudit benzeri ayrı tablo gerekirse PasswordAudit).
11.	Dokümantasyon
•	PasswordPolicy-Design-v2.0.md güncelle (gerçekleşen implementasyon bölümü).
İlk sprint önerisi (Adım 1–3): A. Policy storage yaklaşımını seç (Parameter mı yeni tablo mu).
B. Entity + migration oluştur.
C. Validator taslağı + temel kurallar.
D. Hashing servisi (Argon2id + fallback).
Onaylar mısın? Hangi noktayı değiştirmek istersin? Onay sonrası Adım 1’i uygulamaya başlayacağım.


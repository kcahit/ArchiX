# Parola Politikasý Tasarým Dokümaný (PasswordPolicy)

Revizyon: v2.0 (2025-11-25)

## 1. Amaç / Kapsam
Parola güvenliði veritabanýndaki JSON parametreleri ile yönetilir; deðiþiklik için deploy gerekmez.

## 2. Parametre Kayýtlarý (ApplicationId=1)
- Group="PasswordPolicy", Key="Options" (parola kurallarý)
- Group="PasswordPolicy", Key="Argon2" (hash parametreleri)
ParameterDataTypeId=15 (Json).

## 3. Options JSON Alanlarý (Kesin)
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

Örnek Options Value:
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

## 4. Argon2 JSON Alanlarý
memoryKB=32768
iterations=3
parallelism=2
saltLength=16
hashLength=32

Örnek Argon2 Value:
```json
{
  "memoryKB": 32768,
  "iterations": 3,
  "parallelism": 2,
  "saltLength": 16,
  "hashLength": 32
}
```

## 5. DTO Sýnýflarý
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
Alanlar: UserId (FK), PasswordHash (nvarchar(300)), HashAlgorithm (nvarchar(20)="Argon2id"). Salt format içinde. passwordHistoryCount limiti aþýlýrsa en eski kayýt silinir.

## 7. Arayüzler
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

## 8. Doðrulama Hata Kodlarý
MIN_LENGTH, CATEGORY_UPPER, CATEGORY_LOWER, CATEGORY_DIGIT, CATEGORY_SPECIAL, SEQUENTIAL, REPEATED, BLACKLIST, PWNED, HISTORY, EXPIRED.

## 9. Doðrulama Sýrasý
Uzunluk ? kategoriler ? ardýþýk ? tekrar ? blacklist ? pwned ? history ? yaþ ? throttle.

## 10. Hashleme
Argon2id. Format: `$argon2id$v=19$m=<memoryKB>,t=<iterations>,p=<parallelism>$<salt>$<hash>` nvarchar(300).

## 11. Blacklist
enabledBlacklist=true ? HashSet kontrol; eþleþme BLACKLIST.

## 12. Pwned
enabledPwnedCheck=true ? SHA-1 ilk 5 hex prefix sorgulanýr; tam eþleþme PWNED. Prefix sonuçlarý pwnedPrefixCacheMinutes süre cache.

## 13. Migration / Seed
Migration: UserPasswordHistory tablo + Options ve Argon2 parametreleri yoksa seed.

## 14. Güncelleme
Parameter.Value deðiþince politika anýnda geçerli. 0 veya false kontrolleri kapatýr.

## 15. Ýzleme
Metrikler: password_validation_total, password_validation_error_total (errorCode), password_hash_duration_ms, hibp_queries_total.

## 16. Performans
Baþlangýç Argon2: memoryKB=32768, iterations=3, parallelism=2. Yoðunlukta geçici iterations=2 uygulanabilir.

## 17. Kesin Kararlar
Hash: yalnýz Argon2id.
minSpecial=1 zorunlu.
Entropy kontrolü yok.
History / yaþ >0 deðerlerde aktif.
Blacklist ve Pwned varsayýlan true.
Pepper ENV secret.
Throttle varsayýlan 0.

## 18. Güvenlik
Parola düz metin saklanmaz. TLS zorunlu. AttemptLimiter aktif. Parametreler Application bazýnda farklýlaþtýrýlabilir.

Belge kesin kurallarý içerir; öneri yoktur.

# Parola Politikasý Tasarým Dokümaný (PasswordPolicy)

Revizyon: v2.5 (2025-12-06 09:30 TR)

---

## RL-04: Parola Yaþlandýrma (Password Aging) - v2.5

**Güncelleme Tarihi:** 2025-12-06 09:30 (Türkiye Saati)  
**Baþlangýç Tarihi:** 2025-12-06 06:05  
**Durum:** ?? IN PROGRESS

---

## Genel Bakýþ

Parolalarýn belirli bir süre sonra zorunlu olarak deðiþtirilmesini saðlayan mekanizma. `MaxPasswordAgeDays` parametresi (PasswordPolicy JSON) ve `User.PasswordChangedAtUtc` kolonu üzerinden yönetilir.

---

## 1. Entity Deðiþiklikleri

### 1.1 User Entity - Yeni Kolon

**Dosya:** `src/ArchiX.Library/Entities/User.cs`

```csharp
/// <summary>
/// Parolanýn son deðiþtirilme tarihi (UTC)
/// NULL = hiç deðiþtirilmemiþ (süresi dolmaz)
/// </summary>
[Column(TypeName = "datetimeoffset(4)")]
public DateTimeOffset? PasswordChangedAtUtc { get; set; }
```

### 1.2 PasswordPolicyOptions - Yeni Property

**Dosya:** `src/ArchiX.Library/Options/PasswordPolicyOptions.cs`

? **DONE:** `MaxPasswordAgeDays` property zaten eklendi

```csharp
/// <summary>
/// Parolanýn maksimum yaþý (gün cinsinden)
/// NULL = unlimited (süresi dolmaz)
/// Örnek: 90 = 90 gün sonra deðiþim zorunlu
/// </summary>
public int? MaxPasswordAgeDays { get; set; }
```

---

## 2. Database Migration

**Dosya:** `src/ArchiX.Library/Migrations/20251206_AddPasswordChangedAtUtcToUser.cs`

```csharp
migrationBuilder.AddColumn<DateTimeOffset?>(
    name: "PasswordChangedAtUtc",
    table: "Users",
    type: "datetimeoffset(4)",
    nullable: true,
    comment: "Parolanýn son deðiþtirilme tarihi");

migrationBuilder.CreateIndex(
    name: "IX_Users_PasswordChangedAtUtc",
    table: "Users",
    column: "PasswordChangedAtUtc");
```

---

## 3. IPasswordExpirationService Interface

**Dosya:** `src/ArchiX.Library/Abstractions/Security/IPasswordExpirationService.cs`

```csharp
/// <summary>
/// Parola yaþlandýrma kontrolü saðlayan servis
/// </summary>
public interface IPasswordExpirationService
{
    /// <summary>
    /// Parolanýn süresi dolup dolmadýðýný kontrol eder
    /// </summary>
    bool IsExpired(User user, PasswordPolicyOptions policy, DateTimeOffset? now = null);

    /// <summary>
    /// Parolanýn kalan gün sayýsýný hesaplar
    /// </summary>
    int? GetDaysUntilExpiration(User user, PasswordPolicyOptions policy, DateTimeOffset? now = null);

    /// <summary>
    /// Parolanýn süresi dolacaðý tarihi hesaplar
    /// </summary>
    DateTimeOffset? GetExpirationDate(User user, PasswordPolicyOptions policy);
}
```

---

## 4. PasswordExpirationService Implementasyonu

**Dosya:** `src/ArchiX.Library/Runtime/Security/PasswordExpirationService.cs`

```csharp
/// <summary>
/// Parola yaþlandýrma mantýðýný uygulayan servis
/// </summary>
public class PasswordExpirationService : IPasswordExpirationService
{
    public bool IsExpired(User user, PasswordPolicyOptions policy, DateTimeOffset? now = null)
    {
        // Policy null veya MaxPasswordAgeDays null ? unlimited
        if (policy?.MaxPasswordAgeDays == null)
            return false;

        // Negatif veya 0 ? geçersiz policy
        if (policy.MaxPasswordAgeDays <= 0)
            throw new InvalidOperationException("MaxPasswordAgeDays sýfýrdan büyük olmalýdýr");

        // PasswordChangedAtUtc null ? hiç deðiþtirilmemiþ, süresi dolmaz
        if (user?.PasswordChangedAtUtc == null)
            return false;

        var currentTime = now ?? DateTimeOffset.UtcNow;
        var expirationDate = user.PasswordChangedAtUtc.Value.AddDays(policy.MaxPasswordAgeDays.Value);

        return currentTime > expirationDate;
    }

    public int? GetDaysUntilExpiration(User user, PasswordPolicyOptions policy, DateTimeOffset? now = null)
    {
        if (policy?.MaxPasswordAgeDays == null)
            return null;

        if (user?.PasswordChangedAtUtc == null)
            return null;

        var currentTime = now ?? DateTimeOffset.UtcNow;
        var expirationDate = user.PasswordChangedAtUtc.Value.AddDays(policy.MaxPasswordAgeDays.Value);
        var daysRemaining = (expirationDate - currentTime).Days;

        return daysRemaining >= 0 ? daysRemaining : 0;
    }

    public DateTimeOffset? GetExpirationDate(User user, PasswordPolicyOptions policy)
    {
        if (policy?.MaxPasswordAgeDays == null || user?.PasswordChangedAtUtc == null)
            return null;

        return user.PasswordChangedAtUtc.Value.AddDays(policy.MaxPasswordAgeDays.Value);
    }
}
```

---

## 5. PasswordValidationService Güncellemesi

**Dosya:** `src/ArchiX.Library/Runtime/Security/PasswordValidationService.cs`

**Yeni Hata Kodu:** `EXPIRED`

**Doðrulama Akýþý (güncellenmiþ):**

```
1. Policy kurallarý (senkron)
   ? EMPTY, MIN_LENGTH, MAX_LENGTH, REQ_UPPER, REQ_LOWER, 
     REQ_DIGIT, REQ_SYMBOL, MIN_DISTINCT, REPEAT_SEQ, BLOCK_LIST

2. Parola Yaþlandýrma Kontrolü (senkron) ? YENÝ
   ? IsExpired(user, policy) ? EXPIRED error

3. Pwned Passwords Kontrolü (async)
   ? HIBP API ? PWNED error

4. Password History Kontrolü (async)
   ? Son N parola ? HISTORY error
```

---

## 6. Dependency Injection Kaydý

**Dosya:** `src/ArchiX.Library/Extensions/PasswordSecurityServiceCollectionExtensions.cs`

```csharp
public static IServiceCollection AddPasswordSecurity(this IServiceCollection services)
{
    services.AddSingleton<IPasswordPolicyProvider, PasswordPolicyProvider>();
    services.AddSingleton<IPasswordHasher, Argon2PasswordHasher>();
    services.AddSingleton<IPasswordPolicyAdminService, PasswordPolicyAdminService>();
    
    services.AddHttpClient<IPasswordPwnedChecker, PasswordPwnedChecker>();
    services.AddScoped<IPasswordHistoryService, PasswordHistoryService>();
    
    // RL-04: Password expiration service ? YENÝ
    services.AddScoped<IPasswordExpirationService, PasswordExpirationService>();
    
    services.AddScoped<PasswordValidationService>();
    
    return services;
}
```

---

## 7. Unit Test Tasarýmý

**Dosya:** `tests/ArchiX.Library.Tests/SecurityTests/PasswordExpirationServiceTests.cs`

**12 Adet Test Senaryosu:**

1. `IsExpired_ReturnsFalse_WhenMaxAgeDaysIsNull`
2. `IsExpired_ReturnsFalse_WhenPasswordChangedAtUtcIsNull`
3. `IsExpired_ReturnsFalse_WhenPasswordIsStillValid`
4. `IsExpired_ReturnsTrue_WhenPasswordIsExpired`
5. `IsExpired_ReturnsTrue_WhenPasswordJustExpired`
6. `IsExpired_ThrowsException_WhenMaxAgeDaysInvalid`
7. `GetDaysUntilExpiration_ReturnsNull_WhenMaxAgeDaysIsNull`
8. `GetDaysUntilExpiration_ReturnsCorrectValue`
9. `GetDaysUntilExpiration_ReturnsZero_WhenExpired`
10. `GetExpirationDate_ReturnsCorrectDate`
11. `GetExpirationDate_ReturnsNull_WhenPolicyNull`
12. `GetExpirationDate_ReturnsNull_WhenPasswordChangedAtUtcNull`

---

## 8. Kenar Durumlar ve Validasyonlar

| Durum | Davranýþ | Açýklama |
|-------|----------|----------|
| MaxPasswordAgeDays = null | Unlimited | Süresi dolmaz |
| MaxPasswordAgeDays = 0 | InvalidOperationException | Policy hatasý |
| MaxPasswordAgeDays < 0 | InvalidOperationException | Policy hatasý |
| PasswordChangedAtUtc = null | Not Expired (false) | Hiç deðiþtirilmemiþ = güvenli |
| PasswordChangedAtUtc + MaxDays < Now | Expired (true) | Doðru davranýþ |

---

## 9. Yapýlacaklar Özeti (Sýralý)

| # | Ýþ | Dosya | Durum |
|---|-----|-------|-------|
| 1 | User entity güncelle | User.cs | ? TODO |
| 2 | Migration oluþtur | 20251206_AddPasswordChangedAtUtcToUser.cs | ? TODO |
| 3 | Interface oluþtur | IPasswordExpirationService.cs | ? TODO |
| 4 | Servis uygula | PasswordExpirationService.cs | ? TODO |
| 5 | Validation entegrasyonu | PasswordValidationService.cs | ? TODO |
| 6 | DI kaydý | PasswordSecurityServiceCollectionExtensions.cs | ? TODO |
| 7 | Unit testler | PasswordExpirationServiceTests.cs | ? TODO |

---

## 10. Özet

? **Tamamlanan:**
- PasswordPolicyOptions'a MaxPasswordAgeDays eklendi
- TwoFactor default channel Email'e deðiþtirildi

? **Yapýlacak:** 7 iþ

**Tahmini Süre:** 1-2 gün

---

*Doküman Oluþturma Tarihi: 2025-12-06 09:30 (Türkiye Saati)*

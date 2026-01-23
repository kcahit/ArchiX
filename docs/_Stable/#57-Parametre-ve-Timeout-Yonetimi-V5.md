# #57 — Parametre ve Timeout Yönetimi (DB Driven)

(Revize 5: 22.01.2026 — İmplementasyon Hazır)

## 1) Amaç

1.1 Timeout ve benzeri parametrik kuralların **tamamının DB Parametre tablosu üzerinden** yönetilebilir hale getirilmesi.
1.2 "Hiçbir kural koda gömülü kalmayacak." prensibi: runtime davranışlar DB parametresinden okunacak.
1.3 Kapsam: UI session/idle timeout, Outbound HTTP (retry + timeout), Güvenlik (AttemptLimiter).
1.4 Güvenlik öncelik: DB erişimi yoksa uygulama **startup fail**.

---

## 2) Mevcut Sistem Referansı (#42)

2.1 DB'den JSON parametre okuma paterni var: `docs/#42-tabbed-tum-sayfalar-her-zaman-tab-page-altinda.md`.
2.2 Mevcut seed: `UI/TabbedOptions` (Id=3) tab davranışlarını yönetiyor (örn `tabbed.maxOpenTabs`).

---

## 3) Yeni Şema: master/detail (Parameters + ParameterApplications)

### 3.A Model
```
Parameters (definition/master)
├─ Id (PK)
├─ Group (string)
├─ Key (string)
├─ ParameterDataTypeId (FK)
├─ Description
├─ Template (JSON örnek)
└─ Unique: (Group, Key)

ParameterApplications (value/detail)
├─ Id (PK)
├─ ParameterId (FK)
├─ ApplicationId (FK)
├─ Value (string/JSON)
├─ RowVersion
└─ Unique: (ParameterId, ApplicationId)
```

### 3.B Fallback kuralı
- `ApplicationId = X` için değer yoksa `ApplicationId = 1` ("sistem") geçerlidir.

### 3.C Entity tanımları (.NET 9)

```csharp
// src/ArchiX.Library/Entities/Parameter.cs
public sealed class Parameter
{
    public int Id { get; set; }
    public required string Group { get; set; }
    public required string Key { get; set; }
    public int ParameterDataTypeId { get; set; }
    public string? Description { get; set; }
    public string? Template { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public ParameterDataType DataType { get; set; } = null!;
    public ICollection<ParameterApplication> Applications { get; set; } = [];
}

// src/ArchiX.Library/Entities/ParameterApplication.cs
public sealed class ParameterApplication
{
    public int Id { get; set; }
    public int ParameterId { get; set; }
    public int ApplicationId { get; set; }
    public required string Value { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public Parameter Parameter { get; set; } = null!;
    public Application Application { get; set; } = null!;
}
```

---

## 4) Parametre Envanteri (Kesin Değerler)

### 4.A UI Timeout (yeni parametre: `UI/TimeoutOptions`)
```json
{
  "sessionTimeoutSeconds": 645,
  "sessionWarningSeconds": 45,
  "tabRequestTimeoutMs": 30000
}
```

**NOT:** `maxOpenTabs` taşınmayacak; `UI/TabbedOptions` içinde kalacak.

### 4.B HTTP Policies (yeni parametre: `HTTP/HttpPoliciesOptions`)
```json
{
  "retryCount": 2,
  "baseDelayMs": 200,
  "timeoutSeconds": 30
}
```

**Validasyon range'leri:**
- `retryCount`: [0, 10]
- `baseDelayMs`: [10, 60000]
- `timeoutSeconds`: [1, 300]

### 4.C Security AttemptLimiter (yeni parametre: `Security/AttemptLimiterOptions`)
```json
{
  "window": 600,
  "maxAttempts": 5,
  "cooldownSeconds": 300
}
```

### 4.D System ParameterRefresh (yeni parametre: `System/ParameterRefresh`)
```json
{
  "uiCacheTtlSeconds": 300,
  "httpCacheTtlSeconds": 60,
  "securityCacheTtlSeconds": 30
}
```

---

## 5) Options Class Tanımları

```csharp
// src/ArchiX.Library.Web/Configuration/UiTimeoutOptions.cs
public sealed class UiTimeoutOptions
{
    public int SessionTimeoutSeconds { get; set; } = 645;
    public int SessionWarningSeconds { get; set; } = 45;
    public int TabRequestTimeoutMs { get; set; } = 30000;
}

// src/ArchiX.Library/Infrastructure/Http/HttpPoliciesOptions.cs
public sealed class HttpPoliciesOptions
{
    public int RetryCount { get; set; } = 2;
    public int BaseDelayMs { get; set; } = 200;
    public int TimeoutSeconds { get; set; } = 30;
}

// src/ArchiX.Library/Services/Security/AttemptLimiterOptions.cs
public sealed class AttemptLimiterOptions
{
    public int Window { get; set; } = 600; // seconds
    public int MaxAttempts { get; set; } = 5;
    public int CooldownSeconds { get; set; } = 300;
}

// src/ArchiX.Library/Infrastructure/Parameters/ParameterRefreshOptions.cs
public sealed class ParameterRefreshOptions
{
    public int UiCacheTtlSeconds { get; set; } = 300;
    public int HttpCacheTtlSeconds { get; set; } = 60;
    public int SecurityCacheTtlSeconds { get; set; } = 30;
}
```

---

## 6) Parametre Servisi (Interface + Fallback)

### 6.A Interface

```csharp
// src/ArchiX.Library/Services/Parameters/IParameterService.cs
public interface IParameterService
{
    Task<T?> GetParameterAsync<T>(string group, string key, int applicationId, CancellationToken ct = default)
        where T : class;
    
    Task<string?> GetParameterValueAsync(string group, string key, int applicationId, CancellationToken ct = default);
    
    Task SetParameterAsync(string group, string key, int applicationId, string value, CancellationToken ct = default);
    
    void InvalidateCache(string group, string key);
}
```

### 6.B Fallback mantığı (pseudo-code)

```csharp
public async Task<T?> GetParameterAsync<T>(string group, string key, int applicationId, CancellationToken ct)
{
    // 1. Cache'e bak
    var cacheKey = $"Param:{group}:{key}:{applicationId}";
    if (_cache.TryGetValue(cacheKey, out T? cached))
        return cached;

    // 2. DB: önce applicationId'ye özgü değer ara
    var param = await _db.Parameters
        .Include(p => p.Applications)
        .FirstOrDefaultAsync(p => p.Group == group && p.Key == key, ct);

    if (param == null)
        throw new ParameterNotFoundException(group, key);

    var appValue = param.Applications.FirstOrDefault(a => a.ApplicationId == applicationId);
    
    // 3. Fallback: applicationId yoksa ApplicationId=1'e bak
    appValue ??= param.Applications.FirstOrDefault(a => a.ApplicationId == 1);

    if (appValue == null)
        throw new ParameterValueNotFoundException(group, key, applicationId);

    // 4. Deserialize + cache
    var result = JsonSerializer.Deserialize<T>(appValue.Value);
    
    var ttl = GetTtlForGroup(group); // TTL grup bazlı
    _cache.Set(cacheKey, result, TimeSpan.FromSeconds(ttl));
    
    return result;
}
```

### 6.C Error Handling

**Startup kritik kontrol:**
```csharp
// Program.cs / Startup
var paramService = app.Services.GetRequiredService<IParameterService>();

try
{
    await paramService.GetParameterAsync<UiTimeoutOptions>("UI", "TimeoutOptions", 1);
    await paramService.GetParameterAsync<HttpPoliciesOptions>("HTTP", "HttpPoliciesOptions", 1);
    await paramService.GetParameterAsync<AttemptLimiterOptions>("Security", "AttemptLimiterOptions", 1);
}
catch (Exception ex)
{
    _logger.LogCritical(ex, "Kritik parametreler DB'den okunamadı. Uygulama başlatılamıyor.");
    throw;
}
```

**Runtime parse hatası:**
- Admin kayıt esnasında: JSON schema validation + try-parse → hatalıysa kaydetme.
- Runtime okuma esnasında: exception fırlatılır + log + default değere **dönülmez** (fail-safe yok).

---

## 7) Migration İşlemi

### 7.A Hazırlık (Assistant yapacak)
1. Yeni entity tanımları eklenecek (`Parameter` ve `ParameterApplication`)
2. Mevcut `Parameter` entity silinecek/yeniden yapılandırılacak
3. `AppDbContext` konfigürasyonu güncellenecek
4. `HasData` seed'leri eklenecek

### 7.B Migration oluşturma (Kullanıcı yapacak)

**Komutlar:**
```powershell
# Migration oluştur
Add-Migration ParameterSchemaRefactor

# Veritabanına uygula
Update-Database
```

**ÖNEMLİ:**
- Migration dosyası **EF Core tarafından otomatik** oluşturulacaktır
- Assistant migration dosyası yazmaz, sadece entity/DbContext hazırlar
- Data migration SQL'i gerekiyorsa migration dosyasına **manuel** eklenecektir

### 7.C Data Migration (gerekirse)

Eğer mevcut `Parameter` tablosunda veri varsa, migration sonrası manuel SQL gerekebilir:

```sql
-- Migration sonrası eski veriler yeni tablolara taşınacak
INSERT INTO Parameters (Group, [Key], ParameterDataTypeId, Description, Template, StatusId, CreatedAt)
SELECT DISTINCT [Group], [Key], ParameterDataTypeId, Description, Template, StatusId, CreatedAt
FROM [dbo].[Parameter_OLD];

INSERT INTO ParameterApplications (ParameterId, ApplicationId, Value, StatusId, CreatedAt)
SELECT p.Id, old.ApplicationId, old.Value, old.StatusId, old.CreatedAt
FROM [dbo].[Parameter_OLD] old
INNER JOIN Parameters p ON old.[Group] = p.[Group] AND old.[Key] = p.[Key];
```

---

## 8) Razor Injection (Layout)

```cshtml
@* src/ArchiX.WebHost/Pages/Shared/_Layout.cshtml *@
@inject IParameterService ParameterService

<!DOCTYPE html>
<html>
<head>
    <title>ArchiX</title>
    <script>
        window.ArchiX = window.ArchiX || {};
        
        // UI/TabbedOptions (mevcut)
        window.ArchiX.UiOptions = @Html.Raw(Json.Serialize(await ParameterService.GetParameterAsync<TabbedOptions>("UI", "TabbedOptions", 1)));
        
        // UI/TimeoutOptions (yeni)
        window.ArchiX.TimeoutOptions = @Html.Raw(Json.Serialize(await ParameterService.GetParameterAsync<UiTimeoutOptions>("UI", "TimeoutOptions", 1)));
    </script>
</head>
<body>
    @RenderBody()
</body>
</html>
```

**TabHost JS okuma noktası güncelleme:**
```javascript
// src/ArchiX.Library.Web/wwwroot/js/archix/tabbed/archix-tabhost.js
const config = {
    maxOpenTabs: window.ArchiX?.UiOptions?.tabbed?.maxOpenTabs || 15,
    sessionTimeoutSeconds: window.ArchiX?.TimeoutOptions?.sessionTimeoutSeconds || 600,
    sessionWarningSeconds: window.ArchiX?.TimeoutOptions?.sessionWarningSeconds || 30,
    tabRequestTimeoutMs: window.ArchiX?.TimeoutOptions?.tabRequestTimeoutMs || 30000
};
```

---

## 9) Seed (AppDbContext)

```csharp
// src/ArchiX.Library/Persistence/AppDbContext.OnModelCreating
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Yeni şema seed
    modelBuilder.Entity<Parameter>().HasData(
        new Parameter { Id = 4, Group = "UI", Key = "TimeoutOptions", ParameterDataTypeId = 2, CreatedAt = DateTime.UtcNow },
        new Parameter { Id = 5, Group = "HTTP", Key = "HttpPoliciesOptions", ParameterDataTypeId = 2, CreatedAt = DateTime.UtcNow },
        new Parameter { Id = 6, Group = "Security", Key = "AttemptLimiterOptions", ParameterDataTypeId = 2, CreatedAt = DateTime.UtcNow },
        new Parameter { Id = 7, Group = "System", Key = "ParameterRefresh", ParameterDataTypeId = 2, CreatedAt = DateTime.UtcNow }
    );

    modelBuilder.Entity<ParameterApplication>().HasData(
        new ParameterApplication { Id = 4, ParameterId = 4, ApplicationId = 1, Value = """{"sessionTimeoutSeconds":645,"sessionWarningSeconds":45,"tabRequestTimeoutMs":30000}""", CreatedAt = DateTime.UtcNow },
        new ParameterApplication { Id = 5, ParameterId = 5, ApplicationId = 1, Value = """{"retryCount":2,"baseDelayMs":200,"timeoutSeconds":30}""", CreatedAt = DateTime.UtcNow },
        new ParameterApplication { Id = 6, ParameterId = 6, ApplicationId = 1, Value = """{"window":600,"maxAttempts":5,"cooldownSeconds":300}""", CreatedAt = DateTime.UtcNow },
        new ParameterApplication { Id = 7, ParameterId = 7, ApplicationId = 1, Value = """{"uiCacheTtlSeconds":300,"httpCacheTtlSeconds":60,"securityCacheTtlSeconds":30}""", CreatedAt = DateTime.UtcNow }
    );
}
```

---

## 10) Test Planı

### 10.A Test Class'ları
- `ParameterServiceTests.cs` (fallback, cache, invalidation)
- `ParameterSchemaRefactorMigrationTests.cs` (seed korunuyor mu?)
- `UiTimeoutOptionsTests.cs` (startup validation)
- `HttpPoliciesOptionsTests.cs` (retry/timeout DB'den okunuyor)
- `AttemptLimiterOptionsTests.cs` (window/maxAttempts/cooldown DB'den okunuyor)
- `RazorParameterInjectionTests.cs` (Layout'ta config üretimi doğru)

### 10.B Örnek Test Case

```csharp
[Fact]
public async Task GetParameterAsync_ApplicationIdNotFound_ShouldFallbackToAppId1()
{
    // Arrange
    var service = new ParameterService(_db, _cache);
    
    // Act
    var result = await service.GetParameterAsync<UiTimeoutOptions>("UI", "TimeoutOptions", applicationId: 99);
    
    // Assert
    Assert.NotNull(result);
    Assert.Equal(645, result.SessionTimeoutSeconds); // ApplicationId=1 değeri
}

[Fact]
public async Task Startup_CriticalParameterMissing_ShouldThrow()
{
    // Arrange
    _db.Parameters.RemoveRange(_db.Parameters.Where(p => p.Group == "UI"));
    await _db.SaveChangesAsync();
    
    // Act & Assert
    await Assert.ThrowsAsync<ParameterNotFoundException>(async () =>
    {
        await _paramService.GetParameterAsync<UiTimeoutOptions>("UI", "TimeoutOptions", 1);
    });
}
```

---

## 11) Yapılacak İşler (Bağımlılıklarla Sıralı)

### 11.A Şema / Migration (1. öncelik) ==> tamamlandı 2026-01-22 19:30)
- [ ] **11.A.1** `Parameters` + `ParameterApplications` entity ekle
- [ ] **11.A.2** DbContext konfigürasyonu ekle
- [ ] **11.A.3 [11.A.1,11.A.2]** HasData seed'leri ekle
- [ ] **11.A.4 [11.A.1,11.A.2,11.A.3]** Mevcut seed'lerin korunduğunu doğrula (`UI/TabbedOptions`, `Security/PasswordPolicy`, `TwoFactor/Options`)
- [ ] **11.A.5 [11.A.1,11.A.2,11.A.3,11.A.4]** **Kullanıcı:** `Add-Migration ParameterSchemaRefactor` çalıştır
- [ ] **11.A.6 [11.A.5]** **Kullanıcı:** `Update-Database` çalıştır
- [ ] **11.A.7 [11.A.6]** Gerekiyorsa data migration SQL'i manuel ekle (migration sonrası kontrol)

### 11.B Options Class (2. öncelik - paralel yapılabilir) ==> tamamlandı 2026-01-23 09:40)
- [ ] **11.B.1** `UiTimeoutOptions` class oluştur
- [ ] **11.B.2** `HttpPoliciesOptions` class oluştur
- [ ] **11.B.3** `AttemptLimiterOptions` class oluştur
- [ ] **11.B.4** `ParameterRefreshOptions` class oluştur

### 11.C Servis / Cache (3. öncelik)
- [ ] **11.C.1 [11.A.6,11.B.1,11.B.2,11.B.3,11.B.4]** `IParameterService` interface tanımla
- [ ] **11.C.2 [11.C.1]** `ParameterService` yaz (fallback + cache)
- [ ] **11.C.3 [11.C.2]** DI'ye kaydet (`AddScoped<IParameterService, ParameterService>`)
- [ ] **11.C.4 [11.C.2]** `InvalidateCache` method uygula
- [ ] **11.C.5 [11.C.3]** Startup validation ekle (Program.cs)

### 11.D UI / JS (4. öncelik)
- [ ] **11.D.1 [11.C.3]** Razor Layout'a injection ekle (`IParameterService` kullanarak)
- [ ] **11.D.2 [11.D.1]** TabHost JS okuma noktalarını güncelle (`UiOptions.tabbed.maxOpenTabs` vs `TimeoutOptions.*`)
- [ ] **11.D.3 [11.D.2]** Geriye dönük uyumluluk test et

### 11.E Backend Entegrasyonlar (5. öncelik)
- [ ] **11.E.1 [11.C.3]** HTTP retry/timeout handler'lara DB okuma ekle
- [ ] **11.E.2 [11.C.3]** AttemptLimiter'a DB okuma ekle
- [ ] **11.E.3 [11.E.1,11.E.2]** Hard-coded default'ları kaldır

### 11.F Test (6. öncelik)
- [ ] **11.F.1 [11.A.6]** Migration test (seed'ler doğru mu?)
- [ ] **11.F.2 [11.A.6]** Seed test (yeni parametreler var mı?)
- [ ] **11.F.3 [11.C.2]** Fallback test (ApplicationId=1 fallback çalışıyor mu?)
- [ ] **11.F.4 [11.C.2]** Cache test (TTL doğru mu?)
- [ ] **11.F.5 [11.C.5]** Startup validation test (kritik parametre yoksa fail)
- [ ] **11.F.6 [11.D.1]** UI injection test (Layout'ta config doğru üretiliyor mu?)

---

**Bağımlılık Açıklaması:**
- **11.X.Y**: Bölüm numarası + iş sırası
- **[11.A.1]**: Bu iş, 11.A.1 tamamlanmadan yapılamaz
- **[11.A.1,11.B.2]**: Bu iş, 11.A.1 VE 11.B.2 tamamlanmadan yapılamaz
- Bağımlılık yoksa köşeli parantez yok (paralel yapılabilir)

---

## 99) Kapanış

Bu doküman **implementasyon hazır** haldedir. Entity, migration, servis, injection ve test örnekleri kodla birlikte verilmiştir.

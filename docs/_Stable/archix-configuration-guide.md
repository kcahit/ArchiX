# ArchiX Configuration Guide

## ArchiXOptions Alanları
- `ArchiXConnectionString`: AppDbContext bağlantısı.
- `ArchiXMigrationsAssembly`: (opsiyonel) migration assembly adı.
- `DefaultApplicationId`: host eşleşmezse fallback (genelde 1 veya 2).
- `HostApplicationMapping`: host → ApplicationId sözlüğü.
- `MenuCacheDuration`: menü cache süresi.
- `ParameterCacheDuration`: parametre cache süresi.

## appsettings Örnekleri
### Development
```json
{
  "ConnectionStrings": {
    "ArchiXDb": "Server=(localdb)\\MSSQLLocalDB;Database=ArchiX;Trusted_Connection=True;TrustServerCertificate=True",
    "ApplicationDb": "Server=(localdb)\\MSSQLLocalDB;Database=ArchiXWebHostDLL;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "ArchiX": {
    "DefaultApplicationId": 2,
    "MenuCacheDuration": "01:00:00",
    "ParameterCacheDuration": "00:30:00",
    "HostApplicationMapping": {
      "localhost:5000": 2,
      "127.0.0.1:5000": 2
    }
  }
}
```
### Production (placeholder)
```json
{
  "ConnectionStrings": {
    "ArchiXDb": "<use-keyvault-or-secret-manager>",
    "ApplicationDb": "<use-keyvault-or-secret-manager>"
  },
  "ArchiX": {
    "DefaultApplicationId": 2,
    "MenuCacheDuration": "04:00:00",
    "ParameterCacheDuration": "01:00:00",
    "HostApplicationMapping": {
      "prod.example.com": 2
    }
  }
}
```

## Static Files Cache Politikası
- Development: `Cache-Control: no-store, no-cache, must-revalidate`
- Production: `Cache-Control: public,max-age=31536000,immutable`
- RCL varlıkları: `/_content/ArchiX.Library.Web/**`, `asp-append-version="true"` kullan.

## Sağlık ve Pipeline
- HealthChecks: `/health` → `AddDbContextCheck<AppDbContext>`, `AddDbContextCheck<ApplicationDbContext>`.
- Middleware sırası: `UseStaticFiles` → `UseArchiX` → `UseApplicationContext` → `UseRouting` → `UseAuthentication` → `UseAuthorization` → `MapRazorPages`.

## Secrets
- Development: `dotnet user-secrets` ile bağla veya localdb.
- Production: Azure KeyVault / GH Secrets (CI/CD’de `ConnectionStrings:*`).

## Host → ApplicationId
- appsettings’teki mapping kullanılır; yoksa `DefaultApplicationId`.
- `UseArchiX` middleware HttpContext.Items["ApplicationId"] içine yazar; `UseApplicationContext` bunu DI context’e taşır.

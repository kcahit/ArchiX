# ArchiX.WebHostDLL - Quick Start

## 1) Paket Kaynağı
- Local feed: `./.nuget/local` (repo köküne eklenen `nuget.config` bunu içerir).
- Alternatif: GitHub Packages (ileride CI/CD ile; henüz tasarlanmadı).

## 2) Proje Kurulumu
1. `dotnet new webapp -n ArchiX.WebHostDLL` (örnek) veya mevcut projeyi kullan.
2. `csproj` içinde: `<PackageReference Include="ArchiX.Library" Version="1.0.0" />` ve `<PackageReference Include="ArchiX.Library.Web" Version="1.0.0" />`.
3. `Program.cs` iskeleti:
   - `builder.Services.AddArchiX(opts => ...)` (ArchiX Db conn, host mapping, cache süreleri).
   - `builder.Services.AddDbContext<ApplicationDbContext>(...)` (müşteri DB).
   - `builder.Services.AddArchiXMenu<ApplicationDbContext>();`
   - `builder.Services.AddAuthentication().AddCookie(); builder.Services.AddAuthorization();`
   - `UseArchiX(); UseApplicationContext(); UseAuthentication(); UseAuthorization();`
   - `UseStaticFiles` (dev: no-cache, prod: 1y immutable).
   - Dev’de `MigrateAsync` çağrıları (AppDbContext + ApplicationDbContext).

## 3) appsettings
- `appsettings.json` (örnek):
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
- `appsettings.Production.json`: secrets placeholder (`<use-keyvault-or-secret-manager>`), daha uzun cache süreleri, prod host mapping.

## 4) Migration Komutları
- ArchiX DB (çekirdek, AppDbContext): mevcut migration’lar Library içinde; yeni şema yoksa ek gerekmez.
- Müşteri DB (ApplicationDbContext):
  ```bash
  dotnet ef migrations add InitialCreate --project src/ArchiX.WebHostDLL --startup-project src/ArchiX.WebHostDLL --context ApplicationDbContext --output-dir Migrations
  dotnet ef database update --project src/ArchiX.WebHostDLL --startup-project src/ArchiX.WebHostDLL --context ApplicationDbContext
  ```

## 5) Çalıştırma
- `dotnet run --project src/ArchiX.WebHostDLL`
- Health endpoint: `/health` (iki DbContext check)
- Dashboard: `/Dashboard` (Razor Pages)
- Statik içerik: `/_content/ArchiX.Library.Web/**` (asp-append-version etkin)

## 6) Güvenlik
- Cookie auth varsayılan (`/Account/Login`, `/Account/Denied`).
- `UseArchiX` → host’tan ApplicationId belirler; `UseApplicationContext` → HttpContext’ten ApplicationId/User doldurur.
- ApplicationId=1 sistem kayıtları AppDbContext’te delete/disable korumalı.

## 7) Özet Akış (Dev)
1. Local feed’i kullan (nuget restore).
2. `dotnet build` → `dotnet test`.
3. Gerekirse `dotnet ef database update` (her iki DbContext için).
4. `dotnet run` ve `/health` kontrolü.

# #59 — WebHostDLL Projesinin NuGet Paketleri ile Çalışması (V8)

> **Durum:** Analiz/Tasarım Tamamlandı | **Kod:** İmplementasyona Hazır | **Açık Nokta:** 0  
> **Tarih:** 29.01.2026  
> **Format:** V8 - WebHost kaldırma çelişkisi giderildi, production hazırlık korundu

---

## 1) ANALİZ

### 1.1 Amaç ve Kapsam

**Hedef:**
- **ArchiX.WebHostDLL** adında yeni bir ASP.NET Core Razor Pages projesi oluşturmak (**aynı solution ve aynı repo içinde**)
- Bu projenin ArchiX.Library ve ArchiX.Library.Web'i **NuGet paketi (PackageReference)** olarak tüketmesini sağlamak
- Production ortamını simüle etmek (RCL, static web assets, DLL üzerinden çalışma)
- Çoklu veritabanı mimarisini (ArchiX DB + Müşteri DB) netleştirmek
- Entity'lerin hangi DB'de tutulacağını, migration/seed stratejisini belirlemek
- **Başarılı olduktan sonra ArchiX.WebHost projesini kaldırma** bu iş bu doküman kapsamında olmayacak. Daha sonra istenirse üzerinde çalışılıp karar verilecek. Şu an bu projeye hiçbir şekilde dokunulmayacak. Geçiş için inceleme amaçlı okunacak sadece bu kadar olacak.

**ÖNEMLİ NOT:**
- **ArchiX.WebHost projesine ASLA dokunulmayacak** - hiçbir değişiklik yapılmayacak
- **ArchiX.WebHost'u devreden çıkarmak için bu çalışma yapılıyor ama bu iş kapsamında ArchiX.WebHost asla dokunulmayacaktır**
- ArchiX.WebHostDLL gerçek müşteri projelerini simüle edecek (NuGet paketleri üzerinden)
- ArchiX solution içinde çalışma yapılacak (workspace avantajı)
- ArchiX.WebHostDLL projesi de ArchiX.WebHost peojesi ile aynı dizinde/hizada create edilecek (D:\_git\ArchiX\Dev\ArchiX\src)

**Prensipler:**
- ArchiX.Library ve ArchiX.Library.Web birer NuGet paketi olarak yayınlanacak
- ArchiX.WebHostDLL bu paketleri **PackageReference** ile referans alacak (gerçek production benzeri)
- Statik içerik (wwwroot) ve Razor Pages RCL içinde paketlenecek
- ArchiX.WebHostDLL'de CopyToHost mekanizması olmayacak
- Çoklu DbContext (ArchiX DB için **AppDbContext** + Müşteri DB için **ApplicationDbContext**) ile çalışılacak
- Menü/navigation dinamik (DB'den okunur, varsayılan boş)

**Kapsam:**
- NuGet paket yapısı ve yayınlama
- ArchiX.WebHostDLL projesi oluşturma ve yapılandırma
- Çoklu DB mimarisi ve tablo yerleşimi
- EF migrations/seed stratejisi (ArchiX.WebHostDLL'de)
- Program.cs entegrasyon API'si
- Dinamik menü/navigation sistemi
- ApplicationId ve parametre fallback mantığı
- Statik içerik ve Razor Pages paketleme
- **Production hazırlık: Configuration, Security, Health/Telemetry, CI/CD**

### 1.2 Mevcut Sistem Bağımlılıkları

**ArchiX Katman Yapısı (Development - Mevcut Durum):**
- `ArchiX.Library`: Core library (entities, DbContext, services, abstractions)
- `ArchiX.Library.Web`: Web katmanı (Razor Pages, ViewComponents, wwwroot, JS/CSS)
- `ArchiX.WebHost`: Test harness (**bu projeye dokunulmayacak**)
- 

**Hedef Yapı (Production Simülasyonu):**
- ArchiX.Library → NuGet paketi olarak yayınlanacak
- ArchiX.Library.Web → NuGet paketi olarak yayınlanacak (RCL + static web assets)
- **ArchiX.WebHostDLL** → **Yeni proje (aynı solution içinde)**, yukarıdaki NuGet paketlerini referans alacak

**Mevcut Bağımlılıklar:**
- TabHost JS: `src/ArchiX.Library.Web/wwwroot/js/archix/tabbed/archix-tabhost.js`
- Grid altyapısı: `GridTableViewModel`, `GridToolbarViewModel`, `archix.grid.component.js`
- BaseEntity + soft delete: `StatusId=6`, global filter, `IgnoreQueryFilters()`
- Entity framework: .NET 9, SQL Server

### 1.3 Mevcut Kısıtlar ve Gereksinimler

**1.3.1 Paketleme Kısıtları**
- ArchiX.WebHostDLL'de **PackageReference kullanılacak** (NuGet paketi tüketimi)
- Statik içerik paket içinde olmalı (external dosya yok)
- Razor Pages paket içinde olmalı (RCL pattern)
- **ArchiX.WebHost'a hiçbir değişiklik yapılmayacak**

**1.3.2 Veritabanı Kısıtları**
- Çoklu DB olacak (ArchiX DB + Müşteri DB)
- Cross-DB sorgu yapılamaz
- Her DbContext kendi migration'larını yönetir
- Seed verisi pakette değil, ArchiX.WebHostDLL projesinde olmalı

**1.3.3 Güvenlik/İş Mantığı Kısıtları**
- `ApplicationId=1` (System) silinemez ve update/disable edilemez
- Parametre fallback: ApplicationId yoksa System (1) değerleri kullanılır
- Soft delete zorunlu (fiziksel delete yok)
- Authentication/Authorization entegrasyonu gerekli

**1.3.4 Navigation Kısıtları**
- `window.location.href` kullanılamaz
- TabHost fetch pattern korunmalı
- Menü DB'den dinamik okunmalı

**1.3.5 Configuration Kısıtları**
- Host→ApplicationId mapping appsettings üzerinden yönetilebilir olmalı
- Cache süreleri konfigüre edilebilir olmalı
- Connection strings ortam bazlı yönetilmeli (Dev/Test/Prod)

### 1.4 Veri Modeli ve DB Yerleşimi

**ArchiX DB (Çekirdek):**
Tablolar:
- `Application`: Uygulama tanımları (Id=1: System, Id=2+: müşteri uygulamaları)
- `Parameter`: Sistem parametreleri (ApplicationId ile filtreleme)
- `User`, `Role`, `Permission`: Güvenlik tabloları
- `AuditLog`: Audit trail
- `CacheEntry`: Cache yönetimi (opsiyonel)

**Müşteri DB (Domain - örn. WebHostDLL):**
Tablolar:
- İş mantığı tabloları (test amaçlı örnek tablolar)
- `Menu`: Navigation tanımları
- Test tabloları

**Ortak Entity Yerleşimi:**
Örnek: `Menu` class'ı `ArchiX.Library` içinde tanımlı ama:
- Eğer müşteri DB'de tutulacaksa → `ApplicationDbContext`'e `DbSet<Menu>` eklenir
- Migration müşteri DB'ye yazılır
- Seed müşteri projesinde yapılır

**Application Kayıtları (ArchiX DB):**
```
Id=1  System          (ArchiX çekirdeği, silinemez)
Id=2  WebHostDLL Test (Test uygulama) ==> Test amaçlı
```

### 1.5 Paket Tüketim Modeli

**Hedef Yapı (Production Simülasyonu - Aynı Solution):**
```
NuGet Feed (Private / Local)
├── ArchiX.Library (1.0.0)
└── ArchiX.Library.Web (1.0.0)
    ├── Razor Pages (RCL)
    ├── ViewComponents
    ├── wwwroot/js
    ├── wwwroot/css
    └── wwwroot/images

ArchiX Solution
└── src/
    ├── ArchiX.Library (Paketlenecek)
    ├── ArchiX.Library.Web (Paketlenecek, RCL)
    ├── ArchiX.WebHost (Dokunulmayacak)
    └── ArchiX.WebHostDLL (Yeni proje)
        ├── Dependencies
        │   ├── ArchiX.Library (NuGet - PackageReference)
        │   └── ArchiX.Library.Web (NuGet - PackageReference)
        ├── Pages (müşteriye özel)
        ├── appsettings.json
        ├── appsettings.Development.json
        ├── appsettings.Production.json
        └── Program.cs
```

**Kritik Farklar:**
- `<ProjectReference>` yok → `<PackageReference>` (NuGet)
- CopyToHost yok → RCL + static web assets
- Seed/migration WebHostDLL projesinde
- Ortam bazlı konfigürasyon (Dev/Test/Prod)

---

## 2) TASARIM

### 2.1 NuGet Paket Yapısı

#### 2.1.1 ArchiX.Library Paketi

**İçerik:**
- Entities (Application, Parameter, Menu, BaseEntity vb.)
- DbContext (AppDbContext - ArchiX DB için)
- Repositories, Services
- Abstractions (IRepository, ICacheService vb.)

**Paket Özellikleri (csproj):**
```xml
<PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
    <Version>1.0.0</Version>
    <PackageId>ArchiX.Library</PackageId>
    <Authors>ArchiX Team</Authors>
</PropertyGroup>
```

**Build Komutu:**
- `dotnet pack src/ArchiX.Library/ArchiX.Library.csproj -c Release`

#### 2.1.2 ArchiX.Library.Web Paketi

**İçerik:**
- Razor Pages (RCL pattern)
- ViewComponents
- wwwroot (static web assets)
- TagHelpers

**Paket Özellikleri (csproj):**
```xml
<PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <AddRazorSupportForMvc>true</AddRazorSupportForMvc>
    <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
    <Version>1.0.0</Version>
    <PackageId>ArchiX.Library.Web</PackageId>
</PropertyGroup>

<ItemGroup>
    <EmbeddedResource Include="wwwroot\**\*" />
</ItemGroup>
```

**Static Web Assets:**
- `wwwroot/js/archix/**` → paket içinde, runtime'da `/_content/ArchiX.Library.Web/js/archix/**` olarak erişilir
- `wwwroot/css/**` → paket içinde, runtime'da `/_content/ArchiX.Library.Web/css/**` olarak erişilir
- `wwwroot/images/**` → paket içinde, runtime'da `/_content/ArchiX.Library.Web/images/**` olarak erişilir

**Build Komutu:**
- `dotnet pack src/ArchiX.Library.Web/ArchiX.Library.Web.csproj -c Release`

#### 2.1.3 NuGet Feed Stratejisi

**Karar:**
- İlk etapta **Local NuGet Feed** ile başlanacak (development test için)
- Production'a geçerken **GitHub Packages** kullanılacak
- Detaylar (authentication, CI/CD entegrasyonu vb.) daha sonra ele alınacak

**Local Feed Kullanımı (Development):**
- Klasör: `D:\LocalNuGetFeed\`
- Kolay test
- Hızlı iterasyon

**GitHub Packages (Production):**
- Repo: https://github.com/kcahit/ArchiX
- Authentication gerekecek
- Detaylar sonra ele alınacak

#### 2.1.4 NuGet Versiyonlama ve Bağımlılık Yönetimi

**Semantic Versioning (semver):**
- Format: `MAJOR.MINOR.PATCH` (örn. 1.0.0)
- MAJOR: Breaking changes (API değişiklikleri)
- MINOR: Yeni özellikler (backward compatible)
- PATCH: Bug fixes

**Prerelease Kullanımı:**
- Development: `1.0.0-alpha.1`, `1.0.0-beta.2`
- Release candidate: `1.0.0-rc.1`
- Stable: `1.0.0`

**Bağımlılık Sürüm Aralıkları:**
- EF Core: `[9.0.0, 10.0.0)` → 9.x serisinde kal
- ASP.NET Core: `[9.0.0, 10.0.0)` → 9.x serisinde kal
- Newtonsoft.Json: `[13.0.0, 14.0.0)` → 13.x serisinde kal

**Versiyon Arttırma Stratejisi:**
- Her commit sonrası: PATCH arttır (CI/CD otomatik)
- Yeni özellik: MINOR arttır (manuel karar)
- Breaking change: MAJOR arttır (manuel karar, changelog gerekli)

**NuGet Metadata:**
- PackageDescription: Kısa açıklama
- PackageProjectUrl: GitHub repo URL'si
- RepositoryUrl: GitHub repo URL'si
- PackageLicenseExpression: MIT (veya seçilen lisans)
- PackageTags: archix;erp;library;razor-pages

### 2.2 Çoklu DbContext Mimarisi

#### 2.2.1 AppDbContext (ArchiX DB)

**Konum:** `ArchiX.Library/Context/AppDbContext.cs`

**Amaç:**
- ArchiX DB bağlantısını yönetir
- Application, Parameter, User vb. çekirdek tabloları içerir
- Global soft delete filter uygular
- Tüm müşteriler bu context'i aynı şekilde kullanır

**Özellikler:**
- DbContextOptions<AppDbContext> ile konfigüre edilir
- OnModelCreating'de ApplyGlobalFilters() çağrılır
- Assembly'den entity konfigürasyonları uygulanır

**Signature:**
- `public class AppDbContext : DbContext`
- `public DbSet<Application> Applications { get; set; }`
- `public DbSet<Parameter> Parameters { get; set; }`
- `protected override void OnModelCreating(ModelBuilder modelBuilder)`

#### 2.2.2 ApplicationDbContext (Müşteri DB - WebHostDLL)

**Konum:** `ArchiX.WebHostDLL/Data/ApplicationDbContext.cs`

**ÖNEMLİ NOT:** Tüm müşteri projeleri `ApplicationDbContext` adını kullanır. Her müşteri kendi DB'sine bağlanır (connection string farklı).

**Amaç:**
- Müşteri DB bağlantısını yönetir
- Domain tabloları içerir (test amaçlı)
- Menu gibi ortak entity'leri içerebilir
- Seed verilerini yönetir (HasData)

**Özellikler:**
- DbContextOptions<ApplicationDbContext> ile konfigüre edilir
- Library'deki entity konfigürasyonları uygulanır (Menu için)
- Müşteriye özel konfigürasyonlar eklenir
- SeedData metodu ile initial data yüklenir

**Signature:**
- `public class ApplicationDbContext : DbContext`
- `public DbSet<Menu> Menus { get; set; }`
- `protected override void OnModelCreating(ModelBuilder modelBuilder)`
- `private void SeedData(ModelBuilder modelBuilder)` → Application(Id=2), Menu seed

**SeedData Örnek:**
- Application: Id=2, Code="WEBHOSTDLL_TEST", Name="WebHostDLL Test"
- Menu: Dashboard, Tanımlar (ApplicationId=2)

#### 2.2.3 Design-Time Factory

**Konum:** `ArchiX.WebHostDLL/Data/ApplicationDbContextFactory.cs`

**Amaç:**
- EF Core tools'un migration oluşturması için gerekli
- Design-time'da connection string sağlar
- MigrationsAssembly'yi müşteri projesine yönlendirir

**Özellikler:**
- `IDesignTimeDbContextFactory<ApplicationDbContext>` implement eder
- appsettings.json'dan connection string okur ("ApplicationDb")
- MigrationsAssembly "ArchiX.WebHostDLL" olarak ayarlanır

**Signature:**
- `public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>`
- `public ApplicationDbContext CreateDbContext(string[] args)`

#### 2.2.4 DbContext Lifecycle Yönetimi

**Scoped Lifetime (Varsayılan):**
- AddDbContext ile register edilir
- HTTP request başına bir instance
- Request sonunda dispose edilir

**BackgroundService/Async Operations:**
- Scoped service BackgroundService constructor'ında enjekte EDİLEMEZ
- ServiceScope oluştur, scope içinde DbContext al
- Scope dispose et (using pattern)

**Örnek Senaryo:**
- Background job: Parametreleri periyodik güncelleyen servis
- Çözüm: `IServiceScopeFactory` enjekte et, scope içinde AppDbContext kullan

**Anti-pattern:**
- Singleton service içinde DbContext tutma (memory leak + thread safety)
- DbContext'i manuel dispose etme (DI bunu halleder)

### 2.3 Program.cs Entegrasyon API'si

#### 2.3.1 AddArchiX Extension

**Konum:** `ArchiX.Library.Web/Extensions/ServiceCollectionExtensions.cs`

**Amaç:**
- Tek extension metodu ile tüm ArchiX servislerini register eder
- AppDbContext (ArchiX DB) konfigüre eder
- Repository, Cache, Parameter, Menu servislerini ekler
- ApplicationContext oluşturur

**İlişkiler:**
- ArchiXOptions ile konfigürasyon alır
- AppDbContext → ArchiXOptions.ArchiXConnectionString kullanır
- IRepository<T> → Repository<T> binding
- ICacheService → MemoryCacheService binding
- IMenuService → MenuService binding
- IParameterService → ParameterService binding
- IApplicationContext → HttpContext.Items["ApplicationId"] okur

**Signature:**
- `public static IServiceCollection AddArchiX(this IServiceCollection services, Action<ArchiXOptions> configure)`
- `public class ArchiXOptions` → ArchiXConnectionString, ArchiXMigrationsAssembly, DefaultApplicationId

**Servis Registrations:**
- DbContext<AppDbContext>
- IRepository<T>, Repository<T>
- ICacheService, MemoryCacheService
- IMenuService, MenuService
- IParameterService, ParameterService
- IApplicationContext, ApplicationContext

#### 2.3.2 UseArchiX Middleware

**Konum:** `ArchiX.Library.Web/Middleware/ArchiXMiddleware.cs`

**Amaç:**
- Request'e ApplicationId ekler
- Host bazlı routing yapar
- HttpContext.Items'a ApplicationId yazar

**İlişkiler:**
- Host value okur (context.Request.Host.Value)
- Switch expression ile ApplicationId belirler
- Fallback: ApplicationId = 1 (System)

**Signature:**
- `public static IApplicationBuilder UseArchiX(this IApplicationBuilder app)`

**Host Mapping Örnek:**
- "localhost:5000" → ApplicationId = 2 (WebHostDLL Test)
- "example.com" → ApplicationId = 1 (System, fallback)

#### 2.3.3 Müşteri Program.cs

**Konum:** `ArchiX.WebHostDLL/Program.cs`

**Amaç:**
- ArchiX Core servislerini register eder (AddArchiX)
- Müşteri DbContext'ini register eder (ApplicationDbContext)
- RCL desteğini ekler (AddApplicationPart)
- Middleware pipeline'ı kurar (UseArchiX)

**İlişkiler:**
- AddArchiX → ArchiXConnectionString="ArchiXDb", DefaultApplicationId=2
- AddDbContext<ApplicationDbContext> → ApplicationDb connection string
- AddApplicationPart → ArchiX.Library.Web.Marker assembly (RCL)
- UseArchiX → ApplicationId detection middleware

**Akış:**
- `builder.Services.AddArchiX(options => { ... })` → Core servisler
- `builder.Services.AddDbContext<ApplicationDbContext>(...)` → Müşteri DB
- `builder.Services.AddRazorPages().AddApplicationPart(...)` → RCL
- `app.UseArchiX()` → Middleware
- `app.UseStaticFiles()` → Static files (RCL + kendi)
- `app.MapRazorPages()` → Routing

#### 2.3.4 Konfigürasyon Yönetimi

**appsettings.json Yapısı:**

**Development (appsettings.Development.json):**
- ConnectionStrings: LocalDB veya test SQL Server
- Logging: Information seviye
- Cache süreleri: Kısa (test için)
- Host→ApplicationId mapping: localhost → ApplicationId=2

**Test (appsettings.Test.json):**
- ConnectionStrings: Test SQL Server
- Logging: Warning seviye
- Cache süreleri: Orta
- Host→ApplicationId mapping: test.example.com → ApplicationId=2

**Production (appsettings.Production.json):**
- ConnectionStrings: Secret Manager / Azure Key Vault
- Logging: Error seviye
- Cache süreleri: Uzun
- Host→ApplicationId mapping: prod.example.com → ApplicationId=2

**ArchiXOptions Genişletme:**
- `public Dictionary<string, int> HostApplicationMapping { get; set; }` → appsettings'den okunur
- `public TimeSpan MenuCacheDuration { get; set; }` → varsayılan 1 saat
- `public TimeSpan ParameterCacheDuration { get; set; }` → varsayılan 30 dakika

**Secret Manager Kullanımı (Development):**
- `dotnet user-secrets init`
- `dotnet user-secrets set "ConnectionStrings:ArchiXDb" "Server=..."`
- Production'da: Azure Key Vault

### 2.4 Dinamik Menü ve Navigation

#### 2.4.1 Menu Service

**Konum:** `ArchiX.Library/Services/MenuService.cs`

**Amaç:**
- ApplicationId'ye göre menü verilerini okur
- Cache kullanarak performans sağlar
- SortOrder'a göre sıralama yapar

**İlişkiler:**
- ApplicationDbContext (müşteri DB) enjekte edilir
- ICacheService enjekte edilir
- Menus tablosundan ApplicationId filtreleme yapar
- Cache key: `menu_{applicationId}`
- Cache süresi: ArchiXOptions.MenuCacheDuration (varsayılan 1 saat)

**Signature:**
- `public interface IMenuService`
- `Task<List<MenuItem>> GetMenuForApplicationAsync(int applicationId)`
- `public class MenuService : IMenuService`

**Cache Logic:**
- GetOrCreateAsync pattern
- Cache miss → DB query → OrderBy(SortOrder) → Select(MenuItem) → Cache

#### 2.4.2 Sidebar ViewComponent

**Konum:** `ArchiX.Library.Web/ViewComponents/SidebarViewComponent.cs`

**Amaç:**
- Sidebar menüsünü render eder
- IMenuService kullanarak menü verilerini alır
- IApplicationContext'ten ApplicationId okur

**İlişkiler:**
- IMenuService → GetMenuForApplicationAsync çağrısı
- IApplicationContext → ApplicationId property
- View → List<MenuItem> model

**Signature:**
- `public class SidebarViewComponent : ViewComponent`
- `public async Task<IViewComponentResult> InvokeAsync()`

#### 2.4.3 Varsayılan Boş Dashboard

**Konum:** `ArchiX.WebHostDLL/Pages/Dashboard.cshtml`

**Amaç:**
- Müşteri özelleştirebilir boş dashboard
- Tab-main div ile TabHost entegrasyonu
- Müşteri kendi widget'larını ekler

**Özellikler:**
- `#tab-main` ID (TabHost extract için)
- `.archix-work-area` class
- Bootstrap grid layout

**Markup:**
- `@page`
- `@model ArchiX.WebHostDLL.Pages.DashboardModel`
- `<div id="tab-main" class="archix-work-area">` → Container

#### 2.4.4 Cache Yönetimi ve Invalidation

**Cache Süreleri (Konfigüre Edilebilir):**
- Menu: ArchiXOptions.MenuCacheDuration (varsayılan 1 saat)
- Parameter: ArchiXOptions.ParameterCacheDuration (varsayılan 30 dakika)

**Cache Invalidation Stratejileri:**

**Manual Invalidation:**
- Menu değiştiğinde: `ICacheService.Remove($"menu_{applicationId}")`
- Parameter değiştiğinde: `ICacheService.Remove($"param_{applicationId}_{key}")`

**Sliding Expiration:**
- Her erişimde süre yenilenir
- Kullanılmayan cache otomatik temizlenir

**Absolute Expiration:**
- Belirli süre sonra mutlaka temizlenir
- Configuration'dan okunur

**Cache Invalidation API:**
- `public interface ICacheService`
- `void Remove(string key)`
- `void RemoveByPrefix(string prefix)` → Tüm menu/parameter cache'i temizle
- `Task InvalidateMenuCacheAsync(int applicationId)`
- `Task InvalidateParameterCacheAsync(int applicationId, string key = null)`

### 2.5 Parametre Yönetimi ve Fallback

#### 2.5.1 Parameter Service

**Konum:** `ArchiX.Library/Services/ParameterService.cs`

**Amaç:**
- ApplicationId'ye göre parametre okur
- System (ApplicationId=1) fallback sağlar
- Cache kullanarak performans sağlar

**İlişkiler:**
- AppDbContext (ArchiX DB) enjekte edilir
- IApplicationContext enjekte edilir
- ICacheService enjekte edilir
- Önce application-specific parameter aranır
- Bulunamazsa System (Id=1) parameter döner
- Cache key: `param_{applicationId}_{key}`
- Cache süresi: ArchiXOptions.ParameterCacheDuration (varsayılan 30 dakika)

**Generic Type Support:**
- GetValueAsync<T> → type conversion yapar
- Convert.ChangeType kullanır

**Signature:**
- `public interface IParameterService`
- `Task<string> GetValueAsync(string key, int? applicationId = null)`
- `Task<T> GetValueAsync<T>(string key, int? applicationId = null)`
- `public class ParameterService : IParameterService`

**Fallback Logic:**
1. Cache check: `param_{appId}_{key}`
2. DB query: Application-specific (ApplicationId=X)
3. DB query fallback: System (ApplicationId=1)
4. Not found → KeyNotFoundException

#### 2.5.2 Parametre Seed Örneği

**Konum:** `ArchiX.WebHostDLL/Data/ApplicationDbContext.cs` (OnModelCreating içinde)

**Amaç:**
- System (ApplicationId=1) parametreleri seed eder
- Müşteri (ApplicationId=2) override parametreleri seed eder

**Örnek Data:**
- System: SessionTimeout=30, MaxLoginAttempts=5
- WebHostDLL: SessionTimeout=60 (override)

**Metod:**
- `modelBuilder.Entity<Parameter>().HasData(...)` → System params (Id=1, 2)
- `modelBuilder.Entity<Parameter>().HasData(...)` → Override params (Id=100)

#### 2.5.3 Seed Stratejisi (Dev/Prod Guard)

**Development Seed:**
- Environment.IsDevelopment() kontrolü
- Test verileri (ApplicationId=2, örnek menüler/parametreler)
- Idempotent seed (aynı data tekrar eklenmez)

**Production Seed:**
- Sadece zorunlu veriler (ApplicationId=1: System)
- Müşteriye özel data migration scripti ile eklenir
- HasData kullanılmaz, manuel migration Up() metodu

**Idempotent Seed Pattern:**
- Entity.Id kontrolü: Eğer yoksa ekle
- Unique index kontrolü: Duplicate insert engelini
- Migration'da: `migrationBuilder.Sql("IF NOT EXISTS (...) INSERT ...")`

**Seed Guard Örneği:**
- Development: Tüm test data
- Production: Sadece System application (Id=1)
- `if (env.IsDevelopment()) { SeedTestData(); }`

### 2.6 Migration Stratejisi

#### 2.6.1 ArchiX DB Migrations

**Tam Komut:**
```bash
# Library projesinde
cd src/ArchiX.Library
dotnet ef migrations add InitialCreate --context AppDbContext --output-dir Migrations/ArchiX

# Eğer startup project farklıysa
dotnet ef migrations add InitialCreate --context AppDbContext --output-dir Migrations/ArchiX --startup-project ../ArchiX.Library
```

**Özellikler:**
- Library development sırasında yapılır
- Migration'lar Library projesinde (`Migrations/ArchiX/`)
- NuGet paketine dahil edilir

#### 2.6.2 Müşteri DB Migrations

**Tam Komut:**
```bash
# WebHostDLL projesinde
cd src/ArchiX.WebHostDLL
dotnet ef migrations add InitialCreate --context ApplicationDbContext

# Migration dosyası: Migrations/20260129_InitialCreate.cs
```

**Özellikler:**
- WebHostDLL projesinde yapılır
- Migrations klasörüne yazılır
- Menu tablosu (ortak entity) bu migration'da oluşur
- Test tabloları bu migration'da oluşur

**Migration İçeriği:**
- Menus table (Id, Title, Url, ApplicationId, SortOrder vb.)
- Test domain tables
- Seed data (Application Id=2, Menu kayıtları)

#### 2.6.3 Migration Uygulama (Runtime)

**Konum:** `ArchiX.WebHostDLL/Program.cs`

**Amaç:**
- Development environment'ta otomatik migration uygular
- AppDbContext.Database.MigrateAsync() → ArchiX DB
- ApplicationDbContext.Database.MigrateAsync() → Müşteri DB

**Özellikler:**
- `if (app.Environment.IsDevelopment())` guard
- ServiceScope kullanır
- Her iki DbContext için MigrateAsync çağrılır

**Akış:**
```
Development environment check
→ ServiceScope oluştur
→ AppDbContext al → MigrateAsync()
→ ApplicationDbContext al → MigrateAsync()
```

#### 2.6.4 Migration Komutları Referansı

**EF Core Migration Komutları:**

**Migration Oluşturma:**
- `dotnet ef migrations add <MigrationName> --context <ContextName>`
- `--output-dir <Directory>` → Klasör belirtir
- `--project <ProjectPath>` → Migration hangi projede oluşacak
- `--startup-project <ProjectPath>` → Startup project farklıysa

**Migration Uygulama:**
- `dotnet ef database update --context <ContextName>`
- `--project <ProjectPath>`
- `--startup-project <ProjectPath>`

**Migration Geri Alma:**
- `dotnet ef database update <PreviousMigrationName> --context <ContextName>`
- `dotnet ef migrations remove --context <ContextName>` → Son migration'ı sil

**Migration Script Oluşturma (SQL):**
- `dotnet ef migrations script --context <ContextName> --output migration.sql`
- Production deployment için

**Örnek Komutlar:**
```bash
# ArchiX DB migration (Library)
dotnet ef migrations add AddAuditLog --context AppDbContext --project src/ArchiX.Library

# Müşteri DB migration (WebHostDLL)
dotnet ef migrations add AddInvoiceTable --context ApplicationDbContext --project src/ArchiX.WebHostDLL

# Production SQL script
dotnet ef migrations script --context ApplicationDbContext --project src/ArchiX.WebHostDLL --output deploy.sql
```

### 2.7 Statik İçerik Paketleme

#### 2.7.1 RCL Static Web Assets

**Konum:** `ArchiX.Library.Web/wwwroot/`

**Yapı:**
```
wwwroot/
├── js/
│   ├── archix/
│   │   ├── tabbed/
│   │   │   └── archix-tabhost.js
│   │   └── grid/
│   │       └── archix.grid.component.js
│   └── site.js
├── css/
│   ├── modern/
│   │   └── main.css
│   └── tabhost.css
└── images/
    └── logo.svg
```

**Build Output (NuGet Paketi):**
- Paketlendiğinde: `lib/ArchiX.Library.Web/wwwroot/**`
- Runtime'da erişim: `/_content/ArchiX.Library.Web/js/archix/tabbed/archix-tabhost.js`

**Özellik:**
- RCL static web assets otomatik olarak host projesine yayınlanır
- `<EmbeddedResource Include="wwwroot\**\*" />` ile paketlenir

#### 2.7.2 Müşteri Projesi Static Files

**Konum:** `ArchiX.WebHostDLL/Program.cs`

**Amaç:**
- `app.UseStaticFiles()` ile hem RCL hem kendi dosyalarını servis eder
- RCL içindeki static files otomatik erişilebilir
- `/_content/ArchiX.Library.Web/**` yolu otomatik çalışır

**Akış:**
- UseStaticFiles() → wwwroot/ (kendi dosyalar)
- RCL middleware → /_content/ArchiX.Library.Web/** (RCL dosyalar)

#### 2.7.3 RCL Static Assets Versiyonlama (Cache-Bust)

**Cache-Bust Stratejisi:**
- Query string hash: `/js/archix/tabhost.js?v=abc123`
- File hash: Build sırasında otomatik oluşur
- RCL static files için: `asp-append-version="true"`

**Cache Headers (Production):**
- Static files için uzun süreli cache (1 yıl)
- `Cache-Control: public, max-age=31536000, immutable`
- Hash değişince yeni dosya yüklenir (cache-bust)

**UseStaticFiles Konfigürasyonu:**
```
StaticFileOptions:
  - OnPrepareResponse: Cache-Control header ekle
  - MaxAge: 1 yıl (production)
  - MaxAge: 0 (development, her seferinde yükle)
```

**Tag Helper Kullanımı:**
- `<script src="~/_content/ArchiX.Library.Web/js/archix/tabhost.js" asp-append-version="true"></script>`
- Build hash otomatik eklenir: `tabhost.js?v=abc123`

### 2.8 Application Context ve Claim Yönetimi

#### 2.8.1 Application Context Interface

**Konum:** `ArchiX.Library/Abstractions/IApplicationContext.cs`

**Amaç:**
- Current user ve application bilgilerini taşır
- Servisler arası veri paylaşımı sağlar

**Özellikler:**
- ApplicationId property
- UserId property
- UserName property

**Signature:**
- `public interface IApplicationContext`
- `int ApplicationId { get; }`
- `int UserId { get; }`
- `string UserName { get; }`
- `public class ApplicationContext : IApplicationContext`

#### 2.8.2 Middleware ile Context Doldurma

**Konum:** `ArchiX.Library.Web/Middleware/ApplicationContextMiddleware.cs`

**Amaç:**
- HttpContext'ten ApplicationId ve User bilgilerini okur
- IApplicationContext instance'ını doldurur

**İlişkiler:**
- HttpContext.Items["ApplicationId"] → UseArchiX middleware'den gelir
- HttpContext.User.Identity.IsAuthenticated → user authenticated ise
- HttpContext.User.FindFirst("UserId") → claim'den UserId okur
- ApplicationContext → IApplicationContext implementation

**Signature:**
- `public class ApplicationContextMiddleware`
- `public async Task InvokeAsync(HttpContext context, IApplicationContext appContext)`

**Akış:**
1. ApplicationId okur (Items'dan)
2. User authenticated mi? (Identity.IsAuthenticated)
3. UserId claim okur
4. ApplicationContext doldur

#### 2.8.3 Security ve Authentication

**Authentication/Authorization Stratejisi:**

**Cookie Authentication (Varsayılan):**
- AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
- Login sayfası: /Account/Login
- Cookie süresi: appsettings'den okunur
- SlidingExpiration: true

**JWT Authentication (Alternatif):**
- API endpoint'leri için
- Bearer token
- Token süresi/refresh token stratejisi

**ApplicationId Claim Mapping:**
- Login sonrası: ApplicationId claim ekle
- `new Claim("ApplicationId", applicationId.ToString())`
- Middleware: Claim'den okuyup HttpContext.Items'a yaz

**Authorization Policies:**
- `[Authorize(Policy = "RequireApplicationId")]` → ApplicationId zorunlu
- `[Authorize(Policy = "SystemOnly")]` → Sadece ApplicationId=1
- `[Authorize(Roles = "Admin")]` → Role-based

**Anti-Forgery Token:**
- Razor Pages: Otomatik eklenir
- AJAX: `@Html.AntiForgeryToken()` veya header

**CORS Policy (API için):**
- AllowedOrigins: appsettings'den okunur
- AllowedMethods: GET, POST, PUT, DELETE
- AllowCredentials: true (cookie için)

**ApplicationId=1 Koruması:**
- Soft delete engeli: `if (entity.Id == 1) throw new InvalidOperationException("System application cannot be deleted")`
- Update engeli: `if (entity.Id == 1 && entity.StatusId == 6) throw new InvalidOperationException("System application cannot be disabled")`
- Özel exception: `SystemApplicationProtectionException`

### 2.9 Health ve Telemetry

**Health Check Endpoint:**
- URL: `/health`
- ArchiX DB check: AppDbContext.Database.CanConnectAsync()
- Müşteri DB check: ApplicationDbContext.Database.CanConnectAsync()
- Response: Healthy/Unhealthy + detail

**Health Check Implementation:**
```
AddHealthChecks():
  - AddDbContextCheck<AppDbContext>("ArchiXDb")
  - AddDbContextCheck<ApplicationDbContext>("ApplicationDb")
  - AddCheck<CustomHealthCheck>("CustomCheck")
```

**EF Logging Seviyeleri:**
- Development: Information (tüm SQL query'ler)
- Test: Warning (sadece uyarılar)
- Production: Error (sadece hatalar)

**Logging Konfigürasyonu (appsettings.json):**
```
"Logging": {
  "LogLevel": {
    "Default": "Information",
    "Microsoft.EntityFrameworkCore": "Warning" // Production'da Error
  }
}
```

**Telemetry (Application Insights - Opsiyonel):**
- AddApplicationInsightsTelemetry()
- Custom events: Menu yükleme süresi, cache hit/miss
- Exception tracking

### 2.10 DbContext Lifecycle Yönetimi

**Scoped Lifetime (Varsayılan):**
- HTTP request başına bir DbContext instance
- Request sonunda otomatik dispose
- AddDbContext → Scoped olarak register

**Singleton Service ile DbContext Kullanımı (Anti-pattern):**
- ❌ YAPMA: `public MySingletonService(AppDbContext db)` → Memory leak
- ✅ YAP: `IServiceScopeFactory` enjekte et, scope içinde DbContext al

**BackgroundService Örneği:**
```
public class MyBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    
    public MyBackgroundService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        // DbContext kullan
    }
}
```

**Async Operations:**
- SaveChangesAsync() kullan (blocking yok)
- ToListAsync(), FirstOrDefaultAsync() kullan
- IQueryable chain'ini await et

**DbContext Best Practices:**
- ChangeTracker'ı temizle: `dbContext.ChangeTracker.Clear()`
- Tracking kapat (read-only): `AsNoTracking()`
- Connection pool: SQL Server otomatik halleder

---

## 3) UNIT TEST STRATEJİSİ

### 3.1 Test Kategorileri

#### 3.1.1 Paket Bütünlüğü Testleri
- NuGet paketi doğru build ediliyor mu?
- RCL static web assets pakete dahil mi?
- Dependencies doğru şekilde tanımlı mı?
- Semver kurallarına uyuluyor mu?

#### 3.1.2 DbContext Testleri
- `AppDbContext` doğru şekilde konfigüre ediliyor mu?
- Global filter çalışıyor mu?
- Soft delete filter bypass (`IgnoreQueryFilters`) çalışıyor mu?
- DbContext lifecycle (Scoped) doğru mu?

#### 3.1.3 Migration Testleri
- ApplicationDbContext için migration üretilebiliyor mu?
- Seed verisi doğru şekilde uygulanıyor mu?
- Ortak entity (Menu) müşteri DB'de doğru oluşuyor mu?
- Idempotent seed çalışıyor mu?

#### 3.1.4 Service Testleri
- `ParameterService` fallback doğru çalışıyor mu?
- `MenuService` cache doğru çalışıyor mu?
- ApplicationId filtreleme doğru yapılıyor mu?
- Cache invalidation çalışıyor mu?

#### 3.1.5 Integration Testleri
- `AddArchiX` extension doğru servisleri register ediyor mu?
- Middleware ApplicationId tespiti doğru çalışıyor mu?
- RCL static files runtime'da erişilebiliyor mu?
- Health check endpoint çalışıyor mu?

#### 3.1.6 Security Testleri
- ApplicationId=1 koruması çalışıyor mu?
- Authentication claim mapping doğru mu?
- Authorization policies çalışıyor mu?
- Anti-forgery token validation çalışıyor mu?

### 3.2 Test Case'leri

#### 3.2.1 ParameterService Fallback Testi

**Senaryo 1: Application-specific parameter yok, System döner**
- AppDbContext (InMemory) oluştur
- System parameter ekle (ApplicationId=1, Key="Timeout", Value="30")
- ParameterService oluştur (ApplicationId=2 ile)
- GetValueAsync("Timeout", 2) çağır
- Assert: "30" (System değeri) döner

**Senaryo 2: Application-specific parameter var, onu döner**
- AppDbContext (InMemory) oluştur
- System parameter ekle (ApplicationId=1, Value="30")
- Override parameter ekle (ApplicationId=2, Value="60")
- ParameterService oluştur (ApplicationId=2 ile)
- GetValueAsync("Timeout", 2) çağır
- Assert: "60" (Override değeri) döner

**Test Method Signature:**
- `[Fact] public async Task GetValue_ApplicationSpecificNotFound_ShouldFallbackToSystem()`
- `[Fact] public async Task GetValue_ApplicationSpecificFound_ShouldReturnSpecificValue()`

#### 3.2.2 Menu Service Testi

**Senaryo: Menü SortOrder'a göre sıralanır**
- ApplicationDbContext (InMemory) oluştur
- 3 Menu ekle (SortOrder: 1, 3, 2)
- MenuService oluştur
- GetMenuForApplicationAsync(2) çağır
- Assert: 3 item, sıralama: Dashboard(1), Definitions(2), Reports(3)

**Test Method Signature:**
- `[Fact] public async Task GetMenuForApplication_ShouldReturnOrderedMenu()`

#### 3.2.3 ApplicationId=1 Koruma Testi

**Senaryo: System application silinemez**
- AppDbContext (InMemory) oluştur
- Application ekle (Id=1, System)
- SoftDelete çağır
- Assert: InvalidOperationException fırlatır

**Senaryo 2: System application disable edilemez**
- Application ekle (Id=1, StatusId=1)
- StatusId=6 (disabled) set et
- SaveChanges çağır
- Assert: InvalidOperationException fırlatır

**Test Method Signature:**
- `[Fact] public async Task DeleteApplication_Id1_ShouldThrowException()`
- `[Fact] public async Task DisableApplication_Id1_ShouldThrowException()`

#### 3.2.4 Çoklu DbContext Testi

**Senaryo: İki DbContext bağımsız çalışır**
- AppDbContext (InMemory) oluştur → "ArchiXDb"
- ApplicationDbContext (InMemory) oluştur → "ApplicationDb"
- AppDbContext'e Application ekle
- ApplicationDbContext'e Menu ekle
- Assert: Her DbContext kendi verilerine erişir
- Assert: Cross-DB sorgu yok (AppDbContext'te Menu yok)

**Test Method Signature:**
- `[Fact] public async Task TwoDbContexts_ShouldWorkIndependently()`

#### 3.2.5 RCL Static Files Testi

**Senaryo: RCL static files erişilebilir**
- WebApplicationFactory oluştur
- HttpClient oluştur
- GET `/_content/ArchiX.Library.Web/js/archix/tabbed/archix-tabhost.js`
- Assert: HttpStatusCode.OK
- Assert: ContentType = "application/javascript"

**Senaryo 2: Cache-bust query string eklenir**
- asp-append-version="true" ile render et
- Assert: Query string içeriyor (?v=...)

**Test Method Signature:**
- `[Fact] public async Task StaticFiles_FromRCL_ShouldBeAccessible()`
- `[Fact] public async Task StaticFiles_WithCacheBust_ShouldHaveVersionQueryString()`

#### 3.2.6 Cache Invalidation Testi

**Senaryo: Menu cache invalidate edilir**
- MenuService → GetMenuForApplicationAsync(2) çağır (cache'e yaz)
- ICacheService.Remove($"menu_2") çağır
- MenuService → GetMenuForApplicationAsync(2) tekrar çağır
- Assert: DB'den tekrar okundu (cache miss)

**Test Method Signature:**
- `[Fact] public async Task CacheInvalidation_Menu_ShouldReloadFromDatabase()`

#### 3.2.7 Health Check Testi

**Senaryo: Health endpoint healthy döner**
- WebApplicationFactory oluştur (InMemory DB)
- GET `/health`
- Assert: HttpStatusCode.OK
- Assert: Response body "Healthy"

**Test Method Signature:**
- `[Fact] public async Task HealthCheck_WithHealthyDatabase_ShouldReturnHealthy()`

---

## 4) YAPILACAK İŞLER

### İş 4.1 — ArchiX.Library Paket Hazırlığı
- **Bağımlılık:** Tasarım 2.1.1, 2.1.4
- **Aksiyon:**
  - `GeneratePackageOnBuild` ekle
  - Version/Authors/Description meta verilerini ekle
  - Semver kurallarına göre versiyonla
  - Dependencies doğru tanımla (EF Core, ASP.NET Core sürüm aralıkları)
  - Build test: `dotnet pack -c Release`

### İş 4.2 — ArchiX.Library.Web RCL Dönüşümü
- **Bağımlılık:** Tasarım 2.1.2, 2.7.1, 2.7.3
- **Aksiyon:**
  - `AddRazorSupportForMvc` ekle
  - `wwwroot` içeriğini `EmbeddedResource` olarak işaretle
  - Static web assets build output kontrolü
  - Cache-bust için asp-append-version desteği ekle
  - Build test: `dotnet pack -c Release`

### İş 4.3 — AppDbContext ve Global Filter
- **Bağımlılık:** Tasarım 2.2.1
- **Aksiyon:**
  - Global filter extension method'u yaz
  - Soft delete filter'ı ekle
  - Configuration'ları assembly'den uygula
  - ApplicationId=1 koruma logic'i ekle (soft delete/disable engeli)

### İş 4.4 — AddArchiX Extension Metodu
- **Bağımlılık:** Tasarım 2.3.1, 2.3.4
- **Aksiyon:**
  - `ServiceCollectionExtensions` oluştur
  - `ArchiXOptions` class'ı yaz (HostApplicationMapping, cache süreleri ekle)
  - DbContext, Repository, Cache service registration'ları ekle
  - appsettings.json'dan konfigürasyon okuma

### İş 4.5 — UseArchiX Middleware
- **Bağımlılık:** Tasarım 2.3.2, 2.3.4
- **Aksiyon:**
  - ApplicationId detection middleware yaz
  - Host bazlı routing ekle (appsettings'den okunur)
  - HttpContext.Items'a ApplicationId yaz

### İş 4.6 — ParameterService ve Fallback
- **Bağımlılık:** Tasarım 2.5.1
- **Aksiyon:**
  - `IParameterService` interface'i tanımla
  - Fallback mantığını implement et
  - Cache entegrasyonu ekle (konfigüre edilebilir süre)

### İş 4.7 — MenuService ve Dinamik Navigation
- **Bağımlılık:** Tasarım 2.4.1, 2.4.4
- **Aksiyon:**
  - `IMenuService` interface'i tanımla
  - DB'den menü okuma
  - Cache entegrasyonu (konfigüre edilebilir süre)
  - Cache invalidation API ekle

### İş 4.8 — Sidebar ViewComponent
- **Bağımlılık:** İş 4.7, Tasarım 2.4.2
- **Aksiyon:**
  - ViewComponent oluştur
  - View template yaz
  - ApplicationContext entegrasyonu

### İş 4.9 — ArchiX.WebHostDLL Projesi Oluşturma
- **Bağımlılık:** İş 4.1-4.8
- **Aksiyon:**
  - Yeni ASP.NET Core Razor Pages projesi oluştur (aynı solution içinde)
  - NuGet paketlerini referans et: `<PackageReference Include="ArchiX.Library" Version="1.0.0" />`
  - NuGet paketlerini referans et: `<PackageReference Include="ArchiX.Library.Web" Version="1.0.0" />`
  - appsettings.json, appsettings.Development.json, appsettings.Production.json oluştur

### İş 4.10 — ApplicationDbContext ve Design-Time Factory
- **Bağımlılık:** İş 4.9, Tasarım 2.2.2, 2.2.3, 2.5.3
- **Aksiyon:**
  - `ApplicationDbContext` class'ı yaz
  - `IDesignTimeDbContextFactory` implement et
  - Seed metodu hazırla (Dev/Prod guard ekle, idempotent)

### İş 4.11 — WebHostDLL Program.cs Entegrasyonu
- **Bağımlılık:** İş 4.9, 4.10, Tasarım 2.3.3
- **Aksiyon:**
  - `AddArchiX` çağrısı ekle
  - İki DbContext register et
  - `AddApplicationPart` ile RCL'yi ekle
  - `UseArchiX` middleware'i ekle

### İş 4.12 — İlk Migration (ArchiX DB)
- **Bağımlılık:** İş 4.3, Tasarım 2.6.1, 2.6.4
- **Aksiyon:**
  - `dotnet ef migrations add InitialCreate --context AppDbContext --output-dir Migrations/ArchiX`
  - Migration kontrolü
  - Seed verisi kontrolü

### İş 4.13 — İlk Migration (Application DB)
- **Bağımlılık:** İş 4.10, Tasarım 2.6.2, 2.6.4
- **Aksiyon:**
  - `dotnet ef migrations add InitialCreate --context ApplicationDbContext`
  - Menu tablosu kontrolü
  - Seed verisi kontrolü (Dev/Prod guard)

### İş 4.14 — Migration Auto-Apply (Development)
- **Bağımlılık:** İş 4.12, 4.13, Tasarım 2.6.3
- **Aksiyon:**
  - Program.cs'de environment check
  - `MigrateAsync` çağrıları

### İş 4.15 — ApplicationContext Middleware
- **Bağımlılık:** Tasarım 2.8.1, 2.8.2, 2.8.3
- **Aksiyon:**
  - `IApplicationContext` interface'i yaz
  - Middleware implement et
  - DI registration
  - Claim mapping ekle (ApplicationId claim)

### İş 4.16 — Security Entegrasyonu
- **Bağımlılık:** Tasarım 2.8.3
- **Aksiyon:**
  - Cookie Authentication ekle
  - ApplicationId claim mapping
  - Authorization policies tanımla
  - Anti-forgery token konfigürasyonu
  - CORS policy (API için)

### İş 4.17 — Health Check ve Telemetry
- **Bağımlılık:** Tasarım 2.9
- **Aksiyon:**
  - Health check endpoint ekle (/health)
  - DbContext health check'leri
  - EF logging seviyelerini ayarla (appsettings)
  - Opsiyonel: Application Insights

### İş 4.18 — Connection String Yönetimi
- **Bağımlılık:** Tasarım 2.3.4
- **Aksiyon:**
  - appsettings.Development.json: LocalDB connection string
  - appsettings.Production.json: Placeholder (Secret Manager/Key Vault)
  - Secret Manager kurulumu (dotnet user-secrets)
  - Production not: Azure Key Vault entegrasyonu

### İş 4.19 — Static Assets Versiyonlama
- **Bağımlılık:** Tasarım 2.7.3
- **Aksiyon:**
  - asp-append-version="true" ekle
  - UseStaticFiles cache header konfigürasyonu
  - Production: 1 yıl cache, Development: 0 cache

### İş 4.20 — Unit Testler
- **Bağımlılık:** İş 4.1-4.18, Unit Test 3.2
- **Aksiyon:**
  - ParameterService fallback testleri
  - MenuService testleri
  - ApplicationId=1 koruma testi
  - Çoklu DbContext testi
  - RCL static files testi
  - Cache invalidation testi
  - Health check testi

### İş 4.20 — NuGet Paketi Yayınlama
- **Bağımlılık:** İş 4.1, 4.2
- **Aksiyon:**
  - Local NuGet feed kurulumu (D:\LocalNuGetFeed)
  - `dotnet pack` ile paket oluştur (Release config)
  - `dotnet nuget push` ile local feed'e yayınla
  - Version tag'i oluştur (git tag v1.0.0)
  - GitHub Packages: Daha sonra (CI/CD ile)

### İş 4.21 — CI/CD Pipeline (GitHub Packages)
- **Bağımlılık:** İş 4.21
- **Aksiyon:**
  - GitHub Actions workflow oluştur
  - Build → Test → Pack → Push (GitHub Packages)
  - Semver otomasyonu (commit mesajından versiyon arttır)
  - API key/credential yönetimi (GitHub Secrets)
  - Package cache temizliği

### İş 4.22 — Manuel Test (End-to-End)
- **Bağımlılık:** İş 4.1-4.21
- **Aksiyon:**
  - WebHostDLL projesi çalıştırma
  - Menü dinamik yükleme kontrolü
  - Parametre fallback kontrolü
  - Static files erişim kontrolü (RCL + cache-bust)
  - TabHost navigation kontrolü
  - Health check endpoint kontrolü
  - Authentication/Authorization kontrolü

### İş 4.23 — Dokümantasyon
- **Bağımlılık:** İş 4.1-4.22
- **Aksiyon:**
  - README.md (ArchiX.Library): API dokümantasyonu
  - README.md (ArchiX.Library.Web): RCL kullanımı
  - README.md (ArchiX.WebHostDLL): Örnek proje
  - Getting Started guide (yeni proje için)
  - NuGet paket kullanım guide
  - Migration guide (WebHost → WebHostDLL)
  - Security guide (Authentication/Authorization)
  - Configuration guide (appsettings, Secret Manager, Key Vault)

---

## 5) AÇIK NOKTALAR

> (boş — tüm kararlar alındı, implementasyona hazır)

---

## 6) REFERANSLAR

### DbContext Stratejisi

**ArchiX DB için:**
- `AppDbContext` (ArchiX.Library/Context/AppDbContext.cs)
- Sabit, tüm müşteriler aynı şekilde kullanır
- ArchiX DB'ye bağlanır

**Müşteri DB için:**
- `ApplicationDbContext` (Müşteri projesinde, örn. ArchiX.WebHostDLL/Data/ApplicationDbContext.cs)
- Tüm müşteri projeleri aynı ismi kullanır
- Her müşteri kendi DB'sine bağlanır (farklı connection string)

### Dosya Referansları
- ArchiX.Library: `src/ArchiX.Library/**` (NuGet paketi olarak yayınlanacak)
- ArchiX.Library.Web: `src/ArchiX.Library.Web/**` (NuGet paketi olarak yayınlanacak)
- **ArchiX.WebHost: Dokunulmayacak (inceleme amaçlı)**
- **ArchiX.WebHostDLL: Yeni oluşturulacak (test projesi, NuGet tüketimi)**
- TabHost JS: `src/ArchiX.Library.Web/wwwroot/js/archix/tabbed/archix-tabhost.js`
- Grid JS: `src/ArchiX.Library.Web/wwwroot/js/archix.grid.component.js`

### Entity Referansları
- Entities klasörü: `src/ArchiX.Library/Entities/**`

### Copilot Instructions
- Doküman format standardı: `.github/copilot-instructions.md`
- Katman seçimi kuralı: `.github/copilot-instructions.md`

### NuGet Paket Komutları
- Pack: `dotnet pack src/ArchiX.Library/ArchiX.Library.csproj -c Release`
- Push (Local): `dotnet nuget push bin/Release/ArchiX.Library.1.0.0.nupkg -s D:\LocalNuGetFeed`
- Push (GitHub): `dotnet nuget push bin/Release/ArchiX.Library.1.0.0.nupkg -s https://nuget.pkg.github.com/kcahit/index.json --api-key <TOKEN>`

---

**SON GÜNCELLEME:** 29.01.2026  
**SORUMLU:** GitHub Copilot (Workspace Agent) Claude Sonnet 4.5  
**FORMAT:** V8 - WebHost kaldırma çelişkisi giderildi, production hazırlık korundu

# ArchiX Mimari ve RCL (Razor Class Library) Rehberi

Revizyon: 2025-11-26

Amaç
- Çoklu uygulama (WebHost) tarafından tekrar kullanılabilen, sürdürülebilir bir web katmanı tasarımı için prensipler ve uygulama rehberi.
- Razor Pages tabanlı paylaşılabilir UI, DI (Dependency Injection) uzantıları, güvenlik, konfigürasyon ve paketleme stratejileri.

Özet Katmanlar
- Katman 1: ArchiX.Library (core abstractions + runtime servisler; UI yok)
- Katman 2: ArchiX.Library.Web (RCL; paylaşılan Razor Pages/TagHelper/DI/Security)
- Katman 3: ArchiX.WebHost (uygulama host; pipeline, routing, uygulamaya özgü sayfalar)

1) Katmanlı Mimari
- ArchiX.Library: Arayüzler (Abstractions), provider/servis implementasyonları, domain bağımsız altyapı.
- ArchiX.Library.Web (RCL): Paylaşılan Razor Pages, TagHelper, ViewComponent, ortak layout parça(ları), DI uzantıları, authorization/policy kayıtları.
- ArchiX.WebHost: Program.cs, middleware sırası, ortam/tenant ayarları, uygulamaya özgü sayfalar ve haritalamalar.

2) RCL Kullanım Senaryosu
- Tek noktadan güncellenebilir ortak yönetim sayfaları (ör. Parola Politikası, Parametre Yönetimi, Sağlık/Observability).
- UI tekrarını önler; host projeler sadece RCL’e referans verir.
- Host spesifik durumlar için override mekanizması (bkz. 12).

3) Paylaşılan Web Katmanı Sınırları
- Konmalı: Genel yönetim sayfaları, TagHelper’lar, filtreler, DI extension’lar, minimal layout parçaları, statik varlıklar (wwwroot).
- Konmamalı: Host’a/ortama/kuruma özel kararlar, sabit bağlantı dizgileri, yalnızca tek müşteri akışları.

4) Özelleştirilebilirlik (Extensibility)
- Policy/rol zorunlulukları attribute/DI üzerinden ayarlanabilir olmalı.
- RCL’deki sayfalar host projede aynı rota ile gölgelenebilir (override).
- Yapılandırılabilir bölümler: görünüm temaları, section alanları, Yetkilendirme policy adları (örn. “Admin”).

5) Sürümleme ve API Kararlılığı
- Semantik versiyonlama (MAJOR.MINOR.PATCH).
- Public API (namespace, method imzaları) stabil tutulur; breaking değişikliklerde [Obsolete] → kaldırma geçişi.
- Paket dağıtımı: internal NuGet feed veya GitHub Packages.

6) Konfigürasyon ve DI
- Konfigürasyonlar IOptions pattern veya parametre tablosu ile; host istediğinde ezebilmeli.
- DI uzantıları minimal ve tarafsız: builder.Services.AddArchiXWebDefaults(); gibi.
- Opsiyonel modüller (TwoFactor, JWT, AttemptLimiter) ayrı extension’larla açılıp kapatılabilir.

7) Statik Web Varlıkları (Static Web Assets)
- RCL’de wwwroot desteklenir; host pipeline’da app.UseStaticFiles() gerekir.
- RCL stil/js referansları için asp-append-version kullanımı önerilir.

8) Veri Erişimi ve Soyutlama
- PageModel doğrudan DbContext yerine servis arayüzlerini (ör. IPasswordPolicyAdminService) kullanır.
- Çoklu-uygulama/tenant: applicationId parametresi Page’den servise geçirilir.
- Performans: IMemoryCache veya uygun cache stratejisi; invalidation çağrıları (Invalidate).

9) Güvenlik ve İzolasyon
- Paylaşılan admin sayfaları için varsayılan yetkilendirme: [Authorize(Policy = "Admin")] (policy adı konfigüre edilebilir).
- Hassas işlemler için audit/metric hook noktaları; gizli değerler ENV/Secret Store’dan.

10) Paketleme ve Dağıtım
- CI: build + test + paket (RCL) → NuGet feed’e push.
- Host projeler csproj ProjectReference veya paket sürüm güncellemesi ile tüketir.
- RCL’de gömülü statik varlıklar otomatik servis edilir (UseStaticFiles gerekir).

11) Riskler ve Dikkat Edilecekler
- Aşırı host-özel mantık RCL’e taşınırsa esneklik düşer → özelleştirme noktaları açık olmalı.
- UI değişiklikleri tüm hostları etkiler → override fırsatı tasarlanmalı.
- Performans: Geniş statik varlık seti için önbellekleme/sıkıştırma/caching header ayarları.

12) Önerilen Klasör/Yapı
- ArchiX.Library
  - Abstractions/… (ör. Security)
  - Runtime/… (ör. Security)
  - Migrations/…
  - Docs (tasarım dokümanları, opsiyonel)
- ArchiX.Library.Web (RCL)
  - Pages/Admin/Security/…
  - TagHelpers/…
  - wwwroot/…
  - DI Extensions (ServiceCollectionExtensions)
- ArchiX.WebHost
  - Pages/… (host’a özgü)
  - Program.cs (pipeline + routing)
  - appsettings.*

13) Test Stratejisi
- RCL PageModel testleri: Servisleri mock ederek (örn. IPasswordPolicyAdminService).
- Host routing & authorization integration testleri (RCL sayfaları keşfediliyor mu, yetki uygulanıyor mu?).
- Geriye uyumluluk: Smoke test (yükle/güncelle akışları).

Uygulama Parçacıkları

RCL .csproj
<Project Sdk="Microsoft.NET.Sdk.Razor"> <PropertyGroup> <TargetFramework>net9.0</TargetFramework> <ImplicitUsings>enable</ImplicitUsings> <Nullable>enable</Nullable> </PropertyGroup> <ItemGroup> <ProjectReference Include="..\ArchiX.Library\ArchiX.Library.csproj" /> </ItemGroup> </Project>
WebHost Program.cs (RCL ve Razor Pages için gerekenler)

builder.Services.AddArchiXWebDefaults(); builder.Services.AddRazorPages();
var app = builder.Build(); app.UseStaticFiles(); app.MapRazorPages();

DI Örnekleri (Library tarafı uzantılar)

services.AddArchiXWebDefaults(); // caching + repository caching + password security (policy provider + hasher)
Sayfa Override Örneği
- RCL: Pages/Admin/Security/PasswordPolicy.cshtml(.cs)
- Host’ta aynı yolu oluşturursanız host dosyası önceliklidir; RCL sayfası gölgelenir (override).

Yetkilendirme Önerisi
- Paylaşılan sayfalara [Authorize(Policy = "Admin")] ekleyin veya MapPageRoute üzerinde konvansiyonla uygulayın.
- Policy adı host’ta kayıtlı olmalı (örn. services.AddAuthorization().AddPolicy("Admin", …)).

Konfigürasyon Noktaları
- Parametrik yapı (Parameters tablosu, Group/Key/ApplicationId = 1).
- Güncelleme: değer değiştir → provider.Invalidate(appId) → ilk çağrıda yeniden yüklenir.

Sürümleme ve Dağıtım
- Değişiklikler semantik versiyonlanır, CHANGELOG tutulur.
- Hostlar güncelleme notlarına göre paket sürümünü yükseltir.

Ek: Geliştirici Notları
- Razor Pages önceliklidir; Blazor/MVC yerine Pages hedeflenir.
- .NET 9 / C# 13 kullanımı; nullable/implicit usings açık.
- Kod stili .editorconfig’e uyar; interface’ler Abstractions altında, uygulamalar Runtime altında konumlandırılır.

# ArchiX Template Sistemi - Tasarım ve Uygulama Kararları (V3)

**Doküman Versiyonu:** 3.0  
**Tarih:** 16 Aralık 2025  
**Proje:** ArchiX.Library.Web - Çoklu Template Sistemi  
**Teknoloji:** .NET 9, Razor Pages, MSBuild Auto-Copy

---

## 1. Genel Bakış

ArchiX.Library.Web projesi için çoklu template sistemi geliştirilmiştir. Sistem, kullanıcıların farklı görsel tasarımlar (Classic, Modern, Minimal) arasında seçim yapabilmesini ve kişiselleştirme tercihlerini kaydedebilmesini sağlar.

### Temel Mimari Kararlar

1. **Tek Kaynak Prensibi:** Template'ler (HTML/CSS/Razor) **ArchiX.Library.Web** içinde geliştirilir
2. **Otomatik Dağıtım:** MSBuild ile **ArchiX.WebHost**'a her build'de kopyalanır
3. **Namespace İzolasyonu:** Kopyalanan dosyaların namespace'i otomatik değiştirilir (conflict önleme)
4. **Veritabanı:** Sadece kullanıcı tercihleri ve konfigürasyon saklanır (HTML/CSS asla DB'de değil)

---

## 2. Dosya Yapısı ve Kopyalama Mekanizması

### ArchiX.Library.Web (KAYNAK)

<!--
ArchiX.Library.Web/
├── Templates/Modern/Pages/
│   ├── Login.cshtml + Login.cshtml.cs
│   ├── Dashboard.cshtml + Dashboard.cshtml.cs
│   └── Shared/
│       ├── _Layout.cshtml
│       ├── _AuthLayout.cshtml
│       ├── _Navbar.cshtml
│       ├── _Sidebar.cshtml
│       └── _Footer.cshtml
└── wwwroot/css/
    ├── shared/
    │   ├── normalize.css
    │   └── bootstrap-utilities.css
    └── modern/
        ├── main.css
        └── 00-settings/ ... 05-pages/
-->

### ArchiX.WebHost (HEDEF - Otomatik Kopyalanır)

<!--
ArchiX.WebHost/
├── Pages/
│   ├── Login.cshtml + Login.cshtml.cs          [Auto-copied, namespace fixed]
│   ├── Dashboard.cshtml + Dashboard.cshtml.cs  [Auto-copied, namespace fixed]
│   └── Shared/                                 [Auto-copied from Library.Web]
└── wwwroot/css/                                [Auto-copied from Library.Web]
-->

---

## 3. MSBuild Otomatik Kopyalama

### ArchiX.WebHost.csproj Yapılandırması

<!--
<PropertyGroup>
  <CopyTemplates Condition="'$(CopyTemplates)' == ''">1</CopyTemplates>
</PropertyGroup>

<Target Name="CopyFromLibraryWeb" BeforeTargets="BeforeBuild" Condition="'$(CopyTemplates)' == '1'">
  - CSS dosyalarını kopyala
  - Shared layout'ları kopyala
  - Login.cshtml.cs namespace değiştir (ArchiX.WebHost.Pages)
  - Dashboard.cshtml.cs namespace değiştir (ArchiX.WebHost.Pages)
</Target>
-->

### Kopyalama Davranışı

- **Her Build'de:** Değişiklikler otomatik kopyalanır
- **SkipUnchangedFiles:** Sadece değişen dosyalar kopyalanır (hız optimizasyonu)
- **Namespace Replacement:** `ArchiX.Library.Web.Templates.Modern.Pages` → `ArchiX.WebHost.Pages`
- **Devre Dışı Bırakma:** `dotnet build -p:CopyTemplates=0`

---

## 4. Template Tipleri

### 4.1. Modern Template (İlk Faz - Tamamlandı)

- **Tasarım:** Gradient, glassmorphism, smooth animasyonlar
- **Renk:** Mor-mavi gradient (#667eea → #764ba2)
- **UI:** Bootstrap 5.3 + özel komponentler
- **İkon:** Bootstrap Icons
- **Hedef:** SaaS, yönetici panelleri

**Tamamlanan Sayfalar:**
- ✅ Login (route: `/Login`)
- ✅ Dashboard (route: `/Dashboard`)

### 4.2. Classic Template (Gelecek Faz)

- **Tasarım:** Geleneksel, stabilite odaklı
- **Renk:** Mavi-gri (#007bff, #6c757d)
- **Animasyon:** Minimal (0.2s)
- **Hedef:** Kurumsal kullanıcılar, ERP

### 4.3. Minimal Template (İleri Faz)

- **Tasarım:** Sade, içerik odaklı
- **Renk:** Tek ana renk + gri tonları
- **Hedef:** Mobil öncelikli, düşük bant genişliği

---

## 5. CSS Mimari Kararları

### 5.1. Katmanlı Yapı (Her Template İçinde)

<!--
modern/
├── main.css                    [Tüm katmanları import eder]
├── 00-settings/variables.css   [Renkler, fontlar, spacing]
├── 01-tools/mixins.css         [Utility sınıfları]
├── 02-base/                    [reset, typography, global]
├── 03-components/              [buttons, forms, cards, tables]
├── 04-layouts/                 [header, sidebar, auth-layout]
└── 05-pages/                   [login, dashboard, grid, form]
-->

### 5.2. Shared Katmanı

<!--
shared/
├── normalize.css               [CSS reset - tüm template'ler için]
└── bootstrap-utilities.css     [Ortak utility sınıfları]
-->

### 5.3. Layout CSS Bağlantısı

<!--
<!-- Modern Template -->
<link rel="stylesheet" href="~/css/modern/main.css" />

<!-- Classic Template (gelecek) -->
<link rel="stylesheet" href="~/css/classic/main.css" />
-->

---

## 6. Kullanıcı Tercihleri (UserPreference - Gelecek Faz)

### 6.1. Veritabanı Şeması (Tasarım)

<!--
UserPreferences Table:
- UserId (PK)
- ApplicationId (varsayılan: 1)
- TemplateName (Classic/Modern/Minimal)
- ColorTheme (Light/Dark/Custom)
- CompactMode (bool)
- SidebarPosition (Left/Right)
- CustomColors (JSON)
- DashboardConfig (JSON)
- GridConfig (JSON)
-->

### 6.2. Tercih Çözümleme Sırası

1. **Query String:** `?template=Modern` (test/debug için)
2. **Kullanıcı Tercihi:** UserPreferences tablosu
3. **Uygulama Varsayılanı:** Application parametresi
4. **Sistem Varsayılanı:** Modern

### 6.3. Renk Teması Stratejisi

- **CSS Custom Properties** kullanılır
- **Light/Dark Mode:** `data-theme="dark"` attribute'u ile
- **Custom Colors:** UserPreference.CustomColors JSON'undan `<style>` bloğu oluşturulur

<!--
<style>
  :root {
    --primary-color: #667eea;    /* DB'den gelir */
    --secondary-color: #764ba2;  /* DB'den gelir */
  }
</style>
-->

---

## 7. Geliştirme Workflow'u

### 7.1. Yeni Sayfa Ekleme

1. **ArchiX.Library.Web** içinde geliştir:
   - `Templates/Modern/Pages/NewPage.cshtml`
   - `Templates/Modern/Pages/NewPage.cshtml.cs`
   - `namespace ArchiX.Library.Web.Templates.Modern.Pages`

2. **csproj'e** kopyalama adımı ekle:
   <!--
   <Copy SourceFiles="..\ArchiX.Library.Web\Templates\Modern\Pages\NewPage.cshtml" ... />
   -->

3. **Build yap** → Otomatik WebHost'a kopyalanır (namespace düzeltilir)

### 7.2. CSS Değişiklikleri

1. **ArchiX.Library.Web/wwwroot/css/modern/** altında düzenle
2. **Build yap** → Otomatik WebHost'a kopyalanır
3. **F5** ile test et (Runtime Compilation sayesinde değişiklik anında görünür)

### 7.3. Kriz Testi (Dosya Silme)

**Senaryo:** Developer yanlışlıkla WebHost/Pages/Login.cshtml siler

**Sonuç:**
1. **Build** yapılır
2. MSBuild otomatik Library.Web'den kopyalar
3. Dosya **geri gelir** (sanki hiç silinmemiş gibi)

---

## 8. .gitignore Yapılandırması

<!--
# Auto-copied from Library.Web (don't commit)
src/ArchiX.WebHost/Pages/Login.cshtml
src/ArchiX.WebHost/Pages/Login.cshtml.cs
src/ArchiX.WebHost/Pages/Dashboard.cshtml
src/ArchiX.WebHost/Pages/Dashboard.cshtml.cs
src/ArchiX.WebHost/Pages/Shared/
src/ArchiX.WebHost/wwwroot/css/modern/
src/ArchiX.WebHost/wwwroot/css/shared/
-->

**Neden?**
- Kopyalanan dosyalar Git'e eklenmez (tek kaynak: Library.Web)
- Clone sonrası ilk build otomatik oluşturur
- Team çalışmasında conflict önlenir

---

## 9. Tamamlanan Adımlar (16 Aralık 2025)

### ✅ ADIM 1: CSS Kopyalama (12:30)
- `Frontend-Denemeler/css/` → `Library.Web/wwwroot/css/modern/`
- Shared klasörü oluşturuldu
- Katmanlı yapı (00-05) kopyalandı

### ✅ ADIM 2: Layout Altyapısı (12:41)
- `_Layout.cshtml`, `_AuthLayout.cshtml`, `_Navbar.cshtml`, `_Sidebar.cshtml`, `_Footer.cshtml`
- `_ViewImports.cshtml`, `_ViewStart.cshtml`

### ✅ ADIM 3: Login Sayfası (13:10)
- `Login.cshtml` + `Login.cshtml.cs` (PageModel)
- `Dashboard.cshtml` + `Dashboard.cshtml.cs` (Placeholder)
- Route: `/Login`, `/Dashboard`

### ✅ ADIM 4: MSBuild Auto-Copy (14:52)
- WebHost.csproj yapılandırması
- Namespace otomatik düzeltme
- Clean + Rebuild işlemleri

### ✅ ADIM 5: Test ve Doğrulama (15:00+)
- F5 ile başarılı çalıştırma
- Login sayfası görüntüleme
- Kriz testi (dosya silme/geri gelme)

---

## 10. Sonraki Adımlar

### Adım 4: Dashboard Detaylandırma
- `Ana-Ekran-Calismasi.html` → `Dashboard.cshtml`
- Chart.js entegrasyonu
- İstatistik kartları (Toplam Kullanıcı, Aktif Projeler, Yeni Mesajlar)
- Responsive tasarım iyileştirmeleri

### Adım 5: Grid Sayfası
- `grid-filtre-baslikta.html` → `Grid.cshtml`
- DataTables.js entegrasyonu
- Gelişmiş filtreler, slicer paneli
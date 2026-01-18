# 000 - Layout & Scroll Architecture (v1)

> Amaç: Layout hiyerarşisini C# `object` mantığı gibi **tek bir kökten** üretmek ve UI davranışlarını (özellikle scroll/taşma) **tek kontrat** altında toplamak.
>
> Bu doküman, uygulanacak değişiklikleri **numaralı ve izlenebilir** şekilde listeler. Her madde hem “ne” hem “neden” hem de “nerede” (dosya yoluyla) eşleşir.
>
> Kapsam: Razor Pages (`src/ArchiX.Library.Web`) ve Modern tema.

---

## 0) Problemin Net Tanımı (Gözlem)

### 0.1 Scroll anomalisi
- Browser (body) scroll ediyor, footer “aşağıda” ortaya çıkıyor.
- Aynı anda uygulama içinde “tab/workspace” gibi ekranlar sabit kalıyor veya beklenmeyen şekilde davranıyor.
- Büyük ekranlarda altta “ince çizgi / boşluk / taşma” hissi oluşuyor.

### 0.2 Kök neden sınıfı
Bu davranışlar genellikle şunların kombinasyonundan çıkıyor:
- Body düzeyinde `overflow` açılması (sayfanın kendisi scroll oluyor)
- Footer için `margin-bottom` gibi “hack” boşlukları
- Bazı alanların `position: fixed` olması (tab host gibi)
- Page içeriğinin genişlemesi (`min-width`, `overflow: visible` gibi)

---

## 1) Tasarım İlkeleri (Kontrat)

### 1.1 Tek Scroll Sahibi
**Kontrat:** Uygulamada scroll’un sahibi *tek bir yer* olacak.
- `HEADER` ve `FOOTER` layout’ın parçasıdır (saklanmaz).
- Scroll **sadece** `MAIN` içinde yapılır.
- `BODY` scroll *yapmaz* (overflow kapalı).

> Neden: Desktop `DataGridView` mantığı. İçerik kendi içinde akar; çerçeve (header/footer) sabit bir yaşam alanı sağlar.

### 1.2 Tek Kök Layout (Modern Base)
**Kontrat:** Modern tema için tek bir “kök layout” olmalı.
- Her sayfa/alt-layout bu base’ten türemeli.
- Kural: “Klasik/Fantastik” diye ayrı tasarım yok; olursa onu tasarlayan kişi ayrıca base tanımlamalı.

### 1.3 CSS/JS Hiyerarşisi
- Modern temaya ait stil/behavior: `wwwroot/css/modern/...` ve `wwwroot/js/archix/...` altında olmalı.
- Layout shell davranışı (scroll kontratı) da modern tema parçasıdır.

---

## 2) Hedef Mimari (Dosya & Sorumluluk Haritası)

### 2.1 Modern Base Layout
- Dosya (hedef isim): `src/ArchiX.Library.Web/Pages/Shared/_ModernBaseLayout.cshtml`
- Sorumluluk:
  - `<head>` ortakları (anti-forgery token meta dahil)
  - `archix-shell` body sınıfı
  - `Header / Footer` section zorunluluğu
  - `main` tek scroll alanı (CSS ile)

### 2.2 Türev Layout’lar
- Örnek: `src/ArchiX.Library.Web/Pages/Shared/_AdminLayout.cshtml`
- Sorumluluk:
  - *Sadece* section doldurur: `Styles`, `Header`, `Footer`, `Scripts`
  - `RenderBody()` base’in `main` alanı içinde akar

### 2.3 Shell CSS
- Dosya (hedef): `src/ArchiX.Library.Web/wwwroot/css/modern/04-layouts/archix-shell.css`
- Sorumluluk:
  - `html, body` yükseklik
  - `body` flex-col
  - `main` overflow: auto
  - `body` overflow: hidden

---

## 3) Uygulama Planı (Numaralı İş Paketi)

> Bu bölüm “yapılacaklar listesi”dir. Her madde uygulanınca işaretlenebilir.

### (A) Dosya İsimlendirme ve Yerleşim
A1. `_BaseLayout.cshtml` dosyasını `_ModernBaseLayout.cshtml` olarak yeniden adlandır.
- Neden: Base’in sadece Modern tema için geçerli olduğunu açık etmek.
- Etki alanı: `src/ArchiX.Library.Web/Pages/Shared/`

A2. Shell CSS’ini modern altına taşı.
- Şu an: `src/ArchiX.Library.Web/wwwroot/css/archix.shell.css`
- Hedef: `src/ArchiX.Library.Web/wwwroot/css/modern/04-layouts/archix-shell.css`
- Neden: “modern tema davranışı” modern altında yaşamalı.

A3. `_ModernBaseLayout.cshtml` içinde shell css referansını güncelle.
- Eski: `~/css/archix.shell.css`
- Yeni: `~/css/modern/04-layouts/archix-shell.css`

### (B) Admin Layout’un Modern Base’ten türemesi
B1. `src/ArchiX.Library.Web/Pages/Shared/_AdminLayout.cshtml` içinde
- `Layout = "_BaseLayout"` → `Layout = "_ModernBaseLayout"`
- Neden: Tek kökten türeme.

B2. Admin layout’un `Styles` bölümünde modern `main.css` kullanımı netleştir.
- Karar: Admin ekranları modern tema parçası sayıldığı için `~/css/modern/main.css` kalır.
- İleride ayrı tema doğarsa yeni base/tema çıkar.

### (C) Scroll Kontratı Uyum Kontrolleri (Taşma/Çizgi)
C1. Browser (body) scroll’unu kapat.
- Shell CSS zaten `body { overflow: hidden; }` yapar.

C2. Footer için global `margin-bottom` hack’lerini kaldır.
- Özellikle WebHost `site.css` gibi yerlerde bulunan `body { margin-bottom: 60px; }` benzeri yaklaşımlar modern base ile çakışır.
- Not: Bu repo’da Admin artık modern `main.css` üzerinden gittiği için `site.css` bağımlılığı zaten azaltılmalı.

C3. Yatay taşma üreten stilleri tespit et.
- Örnek risk: `min-width: 1400px` (grid), `overflow: visible`.
- Kural: İçerik genişlerse **main içindeki** uygun kapsayıcı `overflow-x: auto` yönetir; body değil.

### (D) Tabbed/Workspace Uyum Kontrolü
D1. Tab host davranışı “main scroll kontratı” ile uyumlu olmalı.
- Hedef: tab içerikleri, main içinde scroll eden bölgede yaşamalı.
- `position: fixed` gibi kurallar varsa, shell kontratı ile çakışmayacak şekilde ele alınmalı.

---

## 4) Kırılma Kontrol Listesi (Manual)

### 4.1 Ekran bazlı kontrol
- [ ] Admin/Security: Blacklist
- [ ] Admin/Security: PasswordPolicy (JSON diff)
- [ ] Modern: Dashboard
- [ ] Modern: Dataset/Grid
- [ ] Modern: Tabbed navigation açıkken, en geniş içerik sayfaları

### 4.2 Davranış checklist
- [ ] Footer her zaman görünür (layout parçası)
- [ ] Scroll sadece içerikte (main) çalışır
- [ ] Browser scroll ile footer’a “inme” yok
- [ ] Altta ince yatay çizgi/boşluk yok (ya da sadece içerik overflow-x gerekiyorsa main içinde görünür)

---

## 5) Unit/Integration Test Stratejisi (Opsiyonel ama önerilen)

> UI scroll davranışı doğrudan unit test ile ölçülmez; ama “layout hiyerarşisi” ve “doğru layout seçimi” test edilebilir.

Öneriler:
- Razor Pages endpoint testleri: Sayfanın layout kullanımı (response HTML içinde `archix-shell` body class var mı?)
- Smoke test: `_AdminLayout` render oldu mu, required sections mevcut mu?

---

## 6) Notlar (accordion.css konusu)

- `accordion.css` modern tema içinde bir component dosyası.
- Scroll/taşma gibi “sayfa kabuğu” problemleri bu dosyaya yıkılmamalı.
- Bu dokümandaki kontrat uygulanınca, component css’leri **kendi scope’unda** kalır; global taşmalar için önce layout/shell aranır.

---

## 6.1 Scope Dışına Taşma (Component/Theme kaçakları) - Hızlı Denetim Listesi

> Kullanıcı talebi: `accordion.css` ile sınırlı değil; benzer şekilde **konusu dışına taşan** diğer dosyalar da tespit edilmeli.
> Amaç: “tek yerden yönetim” prensibini korumak.

### 6.1.1 Kaçak sınıfları (ne arıyoruz?)

**L-1: Layout/Scroll kontratını bozanlar**
- `body`/`html` üzerinde `overflow-*`, `height`, `min-height` oynayan kurallar
- Footer boşluğu için `margin-bottom` hack’leri
- Global `position: fixed` üst seviye barlar (tab host benzeri) ve z-index patlamaları

**L-2: Genişlik taşması üretenler**
- `min-width` büyük değerler (`1400px` gibi)
- `width: 100vw` / `calc(100vw ...)` gibi viewport genişliğine zorlayan kurallar
- container’larda `overflow: visible` ile birleşen tablolar

**L-3: Aşırı `!important` kullananlar**
- Component dosyasında global seçici ile bootstrap/tema override etmek
- Sonuç: “küçük bir değişiklik her yere bulaşıyor” etkisi

### 6.1.2 Mevcut codebase’te şimdiden görülen riskli noktalar

> Not: Aşağıdaki maddeler *kesin hata* demek değildir; “konusu dışına taşma” ihtimali yüksek olan adaylardır.

**(R1) `modern/02-base/global.css`**
- `body { overflow-x: auto !important; overflow-y: auto !important; }`
- Bu kural, shell kontratıyla (body scroll yok) doğrudan çakışma potansiyeli taşır.

**(R2) `modern/05-pages/grid.css`**
- Büyük `min-width` ve `overflow` kombinasyonları yatay taşmayı tetikleyebilir.
- Kural: Yatay scroll lazımsa, `main` içindeki ilgili kapsayıcıda (`table` wrapper) yönetilmeli.

**(R3) `wwwroot/css/tabhost.css` + `modern/03-components/tabhost.css`**
- `position: fixed` / `top` / `z-index` kombinasyonları layout kontratını delme riski taşır.
- Kural: Tab host “main içinde” yaşar; scroll’u `main` veya tab pane üstlenir.

**(R4) WebHost/Tekil `site.css` benzeri legacy dosyalar**
- `body { margin-bottom: 60px; }` gibi footer hack’leri.
- Modern shell varken bu yaklaşım kaldırılmalı ya da kapsamı daraltılmalı.

**(R5) Navigasyonun `_blank` ile kabuktan kaçması**
- Örn: `Pages/Shared/_SecurityNav.cshtml` içinde Admin/Security linklerinin `target="_blank"` ile yeni sekmede açılması.
- Etki: Kullanıcı “tek layout / tek scroll” beklerken, yeni tab farklı kabuk/senaryo ile açılır; TabHost intercept de devre dışı kalır.
- Kural: Uygulama içi linkler varsayılan olarak `_blank` kullanmamalı; özel durum gerekiyorsa açıkça gerekçelendirilip sınırlanmalı.

### 6.1.3 Uygulanacak standart (yeniden tasarım kuralı)

1) **Önce sınıflandır**: Bu kural layout mu, component mi, page-specific mi?
2) **Layout ise**: sadece `archix-shell.css` / base layout üstünden yönet.
3) **Component ise**: mutlaka bir scope sınıfı ile sınırla (örn. `.archix-accordion-primary ...`).
4) **Page-specific ise**: ilgili `05-pages/*.css` altında kalsın; global seçici kullanma.

### 6.1.4 Takip işi (yeniden başlatma noktası)

- Bu dokümandaki (R1-R4) adaylar üzerinden tek tek geçilecek.
- Hedef: “tek yerden yönetim” ilkesine uymayan kuralları ya taşıma ya da scope’lama.

---

## 7) Uygulama Durumu

- [x] Modern base layout eklendi: `Pages/Shared/_ModernBaseLayout.cshtml`.
- [x] `_AdminLayout` modern base’e türetilerek section modeline geçirildi.
- [x] Shell CSS modern altına taşındı: `wwwroot/css/modern/04-layouts/archix-shell.css`.
- [x] Modern template layout (`Templates/Modern/Pages/Shared/_Layout.cshtml`) modern base’e bağlandı (section model).
- [ ] (C2) Body/legacy scroll hack’leri (örn. `global.css` body overflow ve WebHost `site.css` margin-bottom) shell kontratıyla uyumlu hale getirilecek.
- [ ] TabHost (fixed/top/z-index) ile shell kontratı uyumu netleştirilecek; gerekirse TabHost CSS sadeleştirilecek.
- [ ] Tüm ekranlar kontrol listesine göre doğrulanacak.

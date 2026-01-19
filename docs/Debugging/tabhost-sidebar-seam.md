# TabHost / Sidebar “Seam” Sorunu — Deneme Günlüğü

Bu doküman, TabHost’un sol kenarının sidebar ile tam hizalanmaması ("seam" / 1–2px boşluk veya çizgi) problemi için yapılan denemeleri ve sonuçlarını, ileride aynı noktaya tekrar düşmemek için kronolojik olarak özetler.

> Not: Bu kayıt bir “çözüm dokümanı” değil; hangi değişikliklerin **işe yaramadığını / beklenen etkiyi vermediğini** ve hangi adımların sorunu farklı yöne çektiğini loglar.

## Problem Tanımı
- TabHost (sekme şeridi + pane border’ı) sidebar’ın sağ kenarı ile tam birleşmiyor.
- Kullanıcı gözünde 1–2px “boşluk/çizgi” görünüyor.
- Scroll ve footer erişimi problemleri de bir aşamada bu süreçle birlikte ortaya çıktı.

## Çalışma Ortamı / Mimari
- `CopyToHost` target’ı `src/ArchiX.Library.Web/wwwroot/**` → `src/ArchiX.WebHost/wwwroot/**` kopyalar.
- UI modern template: `_Layout.cshtml` + `_ModernBaseLayout.cshtml` shell.
- TabHost JS: `wwwroot/js/archix/tabbed/archix-tabhost.js`

---

## Denemeler (Başarısız / Beklenen Etki Yok)

### 9) Tab strip/panes left alignment overrides (margin/padding)
**Zaman:** 2026-01-19

**Dosyalar / Değişiklik:**
- `src/ArchiX.Library.Web/wwwroot/css/modern/03-components/tabs.css`
  - `#archix-tabhost-tabs.nav-tabs`: `margin-left:0 !important`, `padding-left:0 !important`
- `src/ArchiX.Library.Web/wwwroot/css/tabhost.css`
  - `#archix-tabhost-tabs`: `margin-left/padding-left:0 !important`
  - `#archix-tabhost-panes`: `padding-left/margin-left:0 !important`

**Beklenen:** Tab şeridinin sol kenarı sidebar çizgisine “tam sıfır” yanaşsın.

**Gözlenen:** Kullanıcı: “olmadı” (seam/sol hizasızlık devam).

### 10) Outer TabHost pane border removal (deterministic seam elimination)
**Zaman:** 2026-01-19

**Dosya / Değişiklik:**
- `src/ArchiX.Library.Web/Templates/Modern/Pages/Shared/_Layout.cshtml`
  - `#archix-tabhost-panes` class: `tab-content border border-top-0` → `tab-content border-top-0`

**Beklenen:** Dış pane container sol/sağ border çizgileri kalkacağı için sidebar ile “yanaşmıyor” hissi veren seam tamamen kaybolsun.

**Gözlenen:** Kullanıcı: “olmadı”

### 11) Remove all outer borders from TabHost panes container
**Zaman:** 2026-01-19

**Dosya / Değişiklik:**
- `src/ArchiX.Library.Web/Templates/Modern/Pages/Shared/_Layout.cshtml`
  - `#archix-tabhost-panes` class: `tab-content border-top-0` → `tab-content`

**Beklenen:** Pane container hiç border çizmediği için (top dahil) seam kalmasın; border’lar içerideki card/grid container’lardan gelsin.

**Gözlenen:** (bekleniyor)

### 1) Body scroll yönetimi ile seam/scroll ilişkisi
**Dosyalar:**
- `src/ArchiX.Library.Web/wwwroot/css/modern/02-base/global.css`
- `src/ArchiX.Library.Web/wwwroot/css/modern/04-layouts/archix-shell.css`

**Deneme:**
- `body` için `overflow:hidden` / `overflow:initial` / `overflow:auto` varyasyonları
- Scroll’un yalnızca `.archix-shell-main` üzerinde olması hedefi

**Sonuç:**
- Bazı kombinasyonlarda browser scroll tamamen kayboldu; footer’a inilemedi.
- Asıl sorun seam’i çözmedi; sadece scroll davranışı üzerinde yan etki üretti.

---

### 2) TabHost’u `position: fixed` pinleme (geri alındı)
**Dosya:**
- `src/ArchiX.Library.Web/wwwroot/css/tabhost.css`

**Deneme:**
- `#archix-tabhost { position: fixed; top:60px; left: calc(260px+8px) }`

**Sonuç:**
- Shell main scroll kontratı bozuldu; uzun sayfalarda footer’a inme ve içerik kesilmesi görüldü.
- Sonrasında `position: static` ile normal akışa alındı.

---

### 3) “Shim” yaklaşımı ile negatif margin denemesi (geri alındı)
**Dosya:**
- `src/ArchiX.Library.Web/wwwroot/css/tabhost.css`

**Deneme:**
- `.main-content > #archix-tabhost { margin-left: -2px !important; }`

**Sonuç:**
- Görsel seam’i deterministik çözmedi.
- “Çöp/deneme” olarak geri alındı.

---

### 4) `.main-content` margin-left piksel oynatma (250/258/261 vs.)
**Dosya:**
- `src/ArchiX.Library.Web/wwwroot/css/modern/04-layouts/sidebar.css`

**Deneme:**
- `margin-left` değerlerini 250–261 aralığında oynatma

**Sonuç:**
- Seam bazen değişiyor gibi görünse de kalıcı çözüm üretmedi.
- Bazı değerlerde overlap/taşma riski ve görsel tutarsızlık.

---

### 5) Bootstrap gutter ve container padding sıfırlama
**Dosya:**
- `src/ArchiX.Library.Web/Templates/Modern/Pages/Shared/_Layout.cshtml`

**Deneme:**
- `container-fluid` → `px-0`
- `row` → `g-0`

**Sonuç:**
- Genel spacing üzerinde etkili; fakat seam beklenen şekilde tamamen kaybolmadı.

---

### 6) TabHost panes ve tab strip için border-left/margin/padding sıfırlama
**Dosyalar:**
- `src/ArchiX.Library.Web/wwwroot/css/tabhost.css`
- `src/ArchiX.Library.Web/wwwroot/css/modern/03-components/tabs.css`

**Deneme:**
- `#archix-tabhost-tabs` ve `#archix-tabhost-panes` için:
  - `border-left: 0 !important`
  - `margin-left: 0 !important`
  - `padding-left: 0 !important`

**Sonuç:**
- Bazı çizgi kaynaklarını azalttı, fakat kullanıcı gözünde “yanaşmıyor” problemi devam etti.

---

### 7) Sidebar `border-right` kaldırma
**Dosya:**
- `src/ArchiX.Library.Web/wwwroot/css/modern/04-layouts/sidebar.css`

**Deneme:**
- `#sidebar.sidebar { border-right: 0; }`

**Sonuç:**
- UI daha sade göründü; fakat kullanıcı “tab sola yanaşmıyor” problemine çözüm olmadığını belirtti.

---

### 8) Sidebar genişliklerini CSS variable ile tek kaynaktan yönetme
**Dosyalar:**
- `src/ArchiX.Library.Web/wwwroot/css/modern/00-settings/variables.css`
- `src/ArchiX.Library.Web/wwwroot/css/modern/04-layouts/sidebar.css`

**Deneme:**
- `--archix-sidebar-width` / `--archix-sidebar-collapsed-width` ekleme
- `#sidebar.sidebar width` ve `.main-content margin-left` aynı variable’a bağlandı

**Sonuç:**
- Mimari olarak “tek kaynak” iyi; ancak seam problemine kullanıcı tarafında net çözüm sağlamadı.

---

## Kısa Notlar — CopyToHost Şüphesi
- Kullanıcı kopyalama problemi şüphesi belirtti.
- Hash doğrulamaları yapıldı:
  - `src/ArchiX.Library.Web/wwwroot/css/modern/main.css` == `src/ArchiX.WebHost/wwwroot/css/modern/main.css`
  - aynı şekilde `sidebar.css` hash’leri eşleşti.
- Yani kopyalama mekanizması en azından `modern/**` için çalışıyor görünüyordu.

---

## Son Durum (Bu Dokümanın Yazıldığı Anda)
- TabHost scroll ve footer kesilmesi problemi belirli bir aşamada düzeldi.
- “TabHost’un sol hizası / çizgi yanaşmıyor” problemi kullanıcı gözünde devam etti.

---

## Açık Kalan Sorular (teknik)
- Sorun gerçek bir layout “gap” mi, yoksa iki ayrı border’ın farklı elemanlarda çizilmesiyle oluşan optik bir seam mi?
- Sidebar sabit (`position: fixed`) + content `margin-left` yaklaşımında subpixel rounding var mı?
- TabHost’ın sol kenarındaki seam’i üreten gerçek DOM elemanı hangisi? (pane border mı, content wrapper mı, grid container mı?)

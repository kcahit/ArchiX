# CSS Specificity & Architecture Guide

## 1) TabHost CSS Cascade Order

### Cascade Hierarchy (düşükten yükseğe):

1. **Bootstrap Defaults** (node_modules/bootstrap/)
   - Specificity: `(0,0,1)` - class selector
   - Örnek: `.container { max-width: 1140px; margin-left: auto; margin-right: auto; }`

2. **Template General Styles** (wwwroot/css/modern/main.css)
   - Specificity: `(0,0,2)` - class + element
   - Örnek: `.archix-shell-main { padding: 1rem; }`

3. **TabHost Specific** (wwwroot/css/tabhost.css)
   - Specificity: `(1,0,3)` - ID + classes
   - Örnek: `#archix-tabhost-panes .archix-tab-content .container { margin: 0 !important; }`
   - **EN YÜKSEK** → Bu kurallar kazanır

---

## 2) Specificity Hesaplama

### Formül: `(ID, Class, Element)`

**Örnek 1:**
```css
.container { margin: auto; }
```
- ID: 0
- Class: 1
- Element: 0
- **Toplam: (0,0,1)**

**Örnek 2:**
```css
#archix-tabhost-panes .archix-tab-content { padding: 0.75rem; }
```
- ID: 1 (#archix-tabhost-panes)
- Class: 1 (.archix-tab-content)
- Element: 0
- **Toplam: (1,1,0)**

**Örnek 3:**
```css
#archix-tabhost-panes .archix-tab-content .container { margin: 0 !important; }
```
- ID: 1
- Class: 2 (.archix-tab-content, .container)
- Element: 0
- **Toplam: (1,2,0)**
- **!important** → her zaman kazanır (specificity bypass)

---

## 3) TabHost Hizalama Sorunu (Case Study)

### Problem:
Definitions/Application tab içeriği ortalanıyor (sol-üst yerine).

### Kök Neden:
```html
<div class="archix-tab-content">
  <div class="container py-4">  <!-- ← SUÇLU -->
    <h1>Definitions / Application</h1>
  </div>
</div>
```

Bootstrap `.container` default kuralı:
```css
.container {
  max-width: 1140px;
  margin-left: auto;   /* ← ortalama */
  margin-right: auto;  /* ← ortalama */
}
```

### Çözüm:
`tabhost.css` içinde override:
```css
#archix-tabhost-panes .archix-tab-content .container {
  max-width: none !important;
  margin-left: 0 !important;
  margin-right: 0 !important;
  width: 100%;
}
```

**Specificity:**
- Bootstrap `.container` → `(0,0,1)`
- TabHost override → `(1,2,0)` + `!important`
- **TabHost kazanır ✅**

---

## 4) !important Kullanım Kuralları

### Sadece şu durumlarda kullan:

1. **Bootstrap Override** (tabhost.css içinde)
   - Bootstrap'in `margin: auto` gibi kurallarını ezmek için
   - Örnek: `.container { margin-left: 0 !important; }`

2. **Utility Classes** (modern/main.css)
   - `.text-center`, `.d-none` gibi utility'ler
   - Örnek: `.d-none { display: none !important; }`

### Kullanma:

- Component-level styling (archix-tabhost.css, tabs.css)
- Page-specific styles (Dashboard.css)
- Inline styles (JavaScript enjekte ettiği kurallar)

---

## 5) BEM Naming Convention (Gelecek İçin)

### Şu anki durum:
```css
#archix-tabhost-panes .archix-tab-content .container { ... }
```

### BEM ile:
```css
.archix-tab__content { ... }
.archix-tab__container--full-width { ... }
```

**Avantajlar:**
- Daha düşük specificity (0,1,0)
- ID selector'a gerek yok
- Daha okunaklı
- !important ihtiyacı azalır

**Geçiş planı:**
- Faze 2'de (optional)
- Mevcut kurallar çalışmaya devam eder (backward compatible)

---

## 6) CSS Değişiklik Test Checklist

### Yeni CSS kuralı eklerken:

1. **Computed styles kontrol:**
   - F12 → Elements → element seç → Computed tab
   - Hangi kural kazanıyor? (en üstte görünür)
   - Override edilen kurallar çizgili görünür

2. **Karşılaştırma testi:**
   - Dashboard tab (referans - çalışan)
   - Problem tab (test edilen)
   - Computed styles yan yana yaz (margin-left/max-width)

3. **Specificity doğrulama:**
   - Yeni kural mevcut kuraldan daha yüksek mi?
   - !important gerekli mi? (sadece Bootstrap override için)

4. **Regresyon testi:**
   - En az 3 farklı tab aç (Dashboard, Definitions, Dataset Tools)
   - Hepsinde hizalama tutarlı mı?

5. **Console log:**
   - `ArchiX.cssDebugMode()` → border'ları görselleştir
   - Specificity chain console'da görüntüle

---

## 7) Debugging Workflow (CSS)

### Adım 1: Problem tespiti
- F12 → Elements → `.archix-tab-content` seç
- Computed → `margin-left: auto` mı `0` mu?

### Adım 2: Kural kaynağı
- Computed'da kural adına tık → hangi dosya? (bootstrap.css mı tabhost.css mi?)
- Satır numarası not et

### Adım 3: Specificity hesapla
- Problem kural: `(0,0,1)`
- Override kural: `(1,2,0)` → yeterli mi?

### Adım 4: Test
- Yeni kural ekle → F12 refresh → Computed kontrol
- Kural uygulandı mı? (çizgili olmayan = aktif)

### Adım 5: Doküman
- `docs/Debugging/` altına not düş:
  - Hangi selector eklendi
  - Beklenen: margin-left: 0
  - Gözlenen: (F12 screenshot)

---

## 8) Hızlı Referans

### En sık kullanılan specificity değerleri:

| Selector | Specificity | Örnek |
|----------|-------------|-------|
| `element` | (0,0,1) | `div`, `p`, `span` |
| `.class` | (0,1,0) | `.container`, `.archix-tab-content` |
| `#id` | (1,0,0) | `#archix-tabhost-panes` |
| `.class .class` | (0,2,0) | `.tab-content .container` |
| `#id .class` | (1,1,0) | `#archix-tabhost-panes .archix-tab-content` |
| `#id .class .class` | (1,2,0) | `#archix-tabhost-panes .archix-tab-content .container` |
| `!important` | ∞ | Her zaman kazanır |

### Console debug komutları:

```javascript
// Specificity chain göster
ArchiX.cssDebugMode();

// Border'ları kaldır
ArchiX.cssDebugModeOff();

// Element computed styles
getComputedStyle(document.querySelector('.archix-tab-content'));
```

---

## 9) Dosya Yapısı

```
src/ArchiX.Library.Web/wwwroot/css/
├── modern/
│   ├── main.css              (template genel)
│   └── 03-components/
│       ├── archix-tabhost.css  (component styling)
│       └── tabs.css            (tab görünümü)
├── tabhost.css               (layout + override - EN YÜKSEK)
└── ...

node_modules/bootstrap/dist/css/
└── bootstrap.css             (framework defaults - EN DÜŞÜK)
```

**Load sırası (HTML):**
1. Bootstrap (CDN)
2. modern/main.css
3. tabhost.css

**Cascade:** Son yüklenen dosya (tabhost.css) + yüksek specificity → kazanır

---

## 10) İlgili Dokümanlar

- **Troubleshooting:** `docs/Debugging/frontend-troubleshooting.md` (F12 workflow)
- **Extract Chain:** `docs/Debugging/frontend-troubleshooting.md` (section 4)
- **Copilot Guide:** `.github/copilot-instructions.md` (Frontend Debugging Template)

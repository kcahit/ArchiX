# Frontend Troubleshooting Guide

## 1) F12 DevTools Checklist

### Elements Tab
1. Tab içeriğini seç: `.archix-tab-content`
2. Parent container kontrol: `#archix-tabhost-panes`
3. İç wrapper'lar: `.container`, `.row`, `.col-*`
4. Sağ tık → Copy → Copy outerHTML (karşılaştırma için)

### Computed Tab
1. `.archix-tab-content` seç → Computed styles:
   - `width`, `max-width`
   - `margin-left`, `margin-right`
   - `padding`
   - `display`, `overflow`
2. Child `.container` seç (varsa) → Computed:
   - `max-width` (Bootstrap default: 1140px)
   - `margin-left`, `margin-right` (auto mu?)
3. Karşılaştırma: Dashboard tab vs problem tab (değerleri yan yana yaz)

### Network Tab
1. Tab aç → Network'te request bul
2. Headers tab → Request Headers:
   - `X-ArchiX-Tab: 1` var mı?
   - `X-Requested-With: XMLHttpRequest` var mı?
3. Preview/Response tab:
   - Response HTML boyutu (full layout: ~85 KB, partial: ~12 KB)
   - `#tab-main` var mı? `.archix-work-area` var mı?

### Console Tab
1. `window.ArchiX.Debug = true` yaz (Enter)
2. Tab aç/kapat → console log'lara bak:
   - Extract chain hangi selector'dan seçti?
   - HTML length ne kadar?
3. Helper komutlar:
   - `ArchiX.diagnoseTab('tab-id')` → tab HTML + computed styles
   - `ArchiX.dumpExtractChain('/Dashboard')` → extract sırası test
   - `ArchiX.cssDebugMode()` → border ekle (görsel debug)

---

## 2) TabHost Hizalama Sorunları

### Semptom: Tab içeriği ortalanıyor (sol-üst yerine)

**Kontrol Listesi:**
1. F12 → Elements → `.archix-tab-content` içinde `.container` var mı?
2. `.container` Computed styles → `margin-left: auto` mı?
3. Dashboard tab ile karşılaştır (Dashboard'da `.container` yok mu?)

**Çözüm:**
- `tabhost.css` içinde `.container` override var mı kontrol et:
```css
#archix-tabhost-panes .archix-tab-content .container {
    max-width: none !important;
    margin-left: 0 !important;
    margin-right: 0 !important;
}
```
- Yoksa ekle, build, test et

---

## 3) CSS Specificity Hierarchy

**TabHost CSS Cascade Order (düşükten yükseğe):**
1. Bootstrap defaults (`node_modules/bootstrap/...`)
   - `.container { max-width: 1140px; margin: auto; }`
2. `modern/main.css` (template genel stilleri)
   - `.archix-shell-main { ... }`
3. `tabhost.css` (TabHost-specific overrides)
   - `#archix-tabhost-panes .archix-tab-content { ... }`
   - **En yüksek specificity** → kazanır

**!important Kullanımı:**
- Sadece `tabhost.css` içinde Bootstrap override için
- Örnek: `.container { margin-left: 0 !important; }`

**Specificity Hesaplama:**
- `#archix-tabhost-panes .archix-tab-content .container` → (1,0,3)
- `.container` → (0,0,1)
- `#archix-tabhost-panes` kazanır

---

## 4) Extract Chain Troubleshooting

**Sıralama (#tab-main → .archix-work-area → main):**

1. `#tab-main` bulunursa → innerHTML al ✅
2. Bulunamazsa `.archix-work-area` bul → innerHTML al
3. Bulunamazsa `main.archix-shell-main` bul → innerHTML al + duplicate temizle:
   - `#archix-tabhost` sil
   - `#sidebar` sil
   - `nav.navbar` sil
   - `footer` sil

**Sorun: Extract yanlış içeriği alıyor**

**Kontrol:**
1. Network → Response Preview → `#tab-main` var mı?
2. `X-ArchiX-Tab: 1` header gönderilmiş mi?
   - Evet → Backend minimal layout döndürmeli (`_Layout.cshtml` conditional)
   - Hayır → Full layout dönüyor, extract fallback'e düşüyor
3. Console → `ArchiX.dumpExtractChain('/problematic-url')` → hangi selector seçildi?

**Çözüm:**
- `_Layout.cshtml` içinde `isTabRequest` kontrolü çalışıyor mu?
- Tab fetch fonksiyonu `X-ArchiX-Tab: 1` header ekliyor mu?

---

## 5) Response Size Analizi

**Beklenen:**
- Full layout: ~85 KB (navbar + sidebar + tabhost + footer)
- Partial layout (tab fetch): ~12 KB (sadece `#tab-main` içeriği)

**Kontrol:**
1. Network tab → request seç
2. Headers → Response Headers → `Content-Length` kontrol et
3. 80+ KB ise → full layout dönüyor (hatalı)
4. 10-20 KB ise → partial dönüyor (doğru)

**Sorun: Full layout dönüyor**
- `X-ArchiX-Tab: 1` header eksik → `archix-tabhost.js` → `loadContent()` kontrol et
- Backend `isTabRequest` kontrolü çalışmıyor → `_Layout.cshtml` debug et

---

## 6) Duplicate Toast Sorunu

**Semptom:** Aynı mesaj 10+ kez görünüyor

**Kontrol:**
1. F12 → Elements → `#toastContainer` altında aynı `data-tab-id` ile birden fazla toast var mı?
2. `archix-tabhost.js` → `showAutoClosePrompt()` fonksiyonu duplicate kontrol yapıyor mu?

**Çözüm:**
```javascript
// showAutoClosePrompt() içinde
const existing = c.querySelector(`.toast[data-tab-id="${CSS.escape(tabId)}"]`);
if (existing) return; // duplicate varsa yeni toast açma
```

---

## 7) Nested Tab Sorunları

**Semptom:** Nested tab açılmıyor ya da içerik yanlış

**Kontrol:**
1. `enableNestedTabs` config değeri `true` mu? (DB parametre)
2. Sidebar hiyerarşisi ile tab hiyerarşisi aynı mı?
3. `openNestedTab()` ile `openTab()` extract mantığı aynı mı?

**Debug:**
1. Console → `ArchiX.Debug = true`
2. Nested tab aç → console log'lara bak
3. Extract chain hangi selector'dan seçti?

---

## 8) Ortak Debugging Workflow

**3 Deneme Kuralı:**
1. Deneme 1: Kod değişikliği → test → "olmadı" ise devam
2. Deneme 2: Farklı kod değişikliği → test → "olmadı" ise devam
3. Deneme 3: **RUNTIME TEŞHİS ZORUNLU** (F12 checklist + screenshot/innerHTML/computed)

**Runtime Teşhis Adımları:**
1. F12 → Elements → problematic element → Copy outerHTML
2. Computed styles → margin/width/max-width değerleri
3. Network → request headers + response size
4. Console → `ArchiX.diagnoseTab()` komutları
5. Bulguları debug log'a yaz (`docs/Debugging/`)

---

## 9) Debug Log Format

```markdown
## YYYY-MM-DD HH:MM (TR local)
- Change: {file} -> {selector/function} için {değişiklik}.
- Expected: {beklenen sonuç}.
- Observed: {gözlenen sonuç - "olmadı" ise F12 bulguları}.
```

**Yasaklar:**
- ❌ `Observed: (bekleniyor)` → belirsiz
- ✅ `Observed: olmadı. F12 Computed: margin-left: auto, max-width: 1140px` → kesin

---

## 10) Hızlı Komutlar

**Console Helper'lar (Development only):**
```javascript
// Debug mode aç
ArchiX.Debug = true;

// Tab diagnose
ArchiX.diagnoseTab('tab-id'); // tab HTML + computed styles

// Extract chain test
ArchiX.dumpExtractChain('/Dashboard'); // hangi selector kazanıyor?

// CSS debug (görsel)
ArchiX.cssDebugMode(); // border ekle, console'a specificity yaz
```

**F12 Kısayollar:**
- `Ctrl+Shift+I` → DevTools aç
- `Ctrl+Shift+C` → Element picker
- `F5` → Refresh
- `Ctrl+F5` → Hard refresh (cache bypass)

# Debug Log: Application Record Submit - Yanlış Tab Reload (#32-submit)

## Problem Tanımı
- **Beklenen:** Form submit → Aynı tab reload (Definitions/Application)
- **Gözlenen:** Form submit → Farklı tab reload (Dashboard?)
- **Durum:** "Yeni Kayıt" accordion'da doğru açılıyor ✅, ancak "Kaydet" → yanlış tab

---

## Denemeler

### 2026-01-23 20:35 - İlk Deneme (AJAX submit + reloadCurrentTab)
- Change:
  - Record.cshtml: AJAX submit + `window.ArchiX.TabHost.reloadCurrentTab()`
  - EntityRecordPageBase: AJAX request → `OkResult()`
  - archix-tabhost.js: `reloadCurrentTab()` async yap
- Expected: Form submit → Aynı tab reload
- Observed: ❌ Yanlış tab reload (kullanıcı: "kaydet yine hatalı taba atıyor")
- F12 bulguları: (BEKLENİYOR)

### 2026-01-23 20:40 - Debug Logging Eklendi
- Change:
  - Record.cshtml: Form submit handler'a console.log ekle
  - archix-tabhost.js: `reloadCurrentTab()` fonksiyonuna detaylı log ekle
- Expected: Console'da detaylı akış görülecek
- Observed: ✅ **KÖK NEDEN BULUNDU!**
- Console log'ları:
  ```
  [TabHost] Aktif tab detail: {id: 't_okcxNkqzmkusqsdd', url: '/Dashboard', ...}
  [TabHost] Target URL: /Dashboard
  ```
  **SORUN:** Aktif tab Dashboard olarak tespit ediliyor! (Definitions/Application değil)

### 2026-01-23 20:45 - KÖK NEDEN FİX: reloadTabById Eklendi
- Change:
  1. Record.cshtml: `reloadCurrentTab()` yerine `reloadTabById()` kullan
     - Accordion'un parent pane'ini bul (`.tab-pane[data-tab-id]`)
     - Parent tab ID'sine göre reload et
     - Accordion'u kapat
  2. archix-tabhost.js: `reloadTabById(tabId)` fonksiyonu ekle
     - `state.activeId` yerine verilen `tabId` kullan
     - Belirli bir tab'ı reload et
- Expected: Form submit → Accordion'un bulunduğu tab reload (Definitions/Application)
- Observed: ❌ **Hala Dashboard'a atıyor! "[Record Form]" log'ları yok!**
- F12 bulguları:
  ```
  [TabHost] reloadCurrentTab BAŞLADI (ESKİ KOD!)
  [TabHost] Aktif tab detail: {url: '/Dashboard', ...}
  ```
  **SORUN:** Script section çalışmıyor veya browser cache eski JS'i kullanıyor

### 2026-01-23 20:50 - Script Init Debug Eklendi
- Change:
  - Record.cshtml: Script başına init log ekle
  - Form submit handler'a detaylı accordion check log ekle
  - Form bulundu mu, accordion görünür mü kontrol et
- Expected: Console'da görülecek:
  ```
  [Record Form INIT] Script yüklendi - timestamp: (zaman)
  [Record Form INIT] Form bulundu: true
  [Record Form] Submit event tetiklendi
  [Record Form] Accordion element: (element)
  [Record Form] Accordion içinde - AJAX submit başlıyor
  ```
- Observed: ❌ **"[Record Form INIT]" log'u YOK!**
- F12 bulguları: Sadece eski `[TabHost] reloadCurrentTab` log'ları var
- **KÖK NEDEN BULUNDU:** `@section Scripts` accordion içine extract edilmiyor!

### 2026-01-23 21:00 - KRİTİK FİX: Script Inline'a Taşındı
- Change:
  - Record.cshtml: `@section Scripts` kaldırıldı
  - Tüm JS kodu inline olarak `#tab-main` içine taşındı
  - `showRecordInAccordion()` fonksiyonu `#tab-main` içeriğini extract ediyor
  - Şimdi script'ler accordion içinde çalışacak
- Expected:
  - Console'da `[Record Form INIT]` log'u görülecek
  - Form submit → `[Record Form]` log'ları görülecek
  - `reloadTabById()` doğru tab'ı reload edecek
- Observed: ✅ Script çalışıyor ANCAK yeni sorun!
- F12 bulguları:
  ```
  [Record Form INIT] Script yüklendi ✅
  [Record Form] Parent tab ID: g_Definitions ✅ (GROUP TAB!)
  [TabHost] Target URL: null ❌ (Group tab'ın URL'i yok!)
  ```
  **YENİ SORUN:** Parent tab bir **group tab**, URL'i `null` → 404 hatası

### 2026-01-23 21:05 - Group Tab Child Tab Fix
- Change:
  - Record.cshtml: Parent tab group tab ise (`g_` ile başlıyorsa)
  - Group içindeki **aktif child tab**'ı bul (`.tab-pane.active[data-tab-id]`)
  - O child tab'ı reload et
- Expected:
  - Console'da "Group tab tespit edildi" log'u
  - "Aktif child tab ID: (gerçek tab ID)" log'u
  - `reloadTabById(child-tab-id)` ile doğru tab reload
- Observed: (TEST EDİLECEK)
- **TEST ADIMLARI:**
  1. **HARD REFRESH:** `Ctrl+Shift+R` (inline JS değişti)
  2. Definitions → Application tab (group içinde)
  3. Grid'de "Değiştir" → Accordion
  4. Form submit et
  5. **Console'da bak:**
     - "Group tab tespit edildi" görüyor musun?
     - "Aktif child tab ID: ..." ne diyor?
     - 404 hatası var mı?

---

## Kök Neden Analizi

**Kök Neden:** `state.activeId` Dashboard tab'ını gösteriyor, Definitions/Application değil

**Kanıt (F12 Console):**
```
[TabHost] Aktif tab ID: t_okcxNkqzmkusqsdd
[TabHost] Aktif tab detail: {url: '/Dashboard', ...}
```

**Neden Oldu:**
- Kullanıcı Dashboard tab'ındayken Definitions/Application'ı açtı
- Ancak TabHost `state.activeId` güncellemedi
- Veya Definitions/Application tab'ı arka planda açıldı ama aktif edilmedi

**Çözüm:**
- `reloadCurrentTab()` (activeId'ye güvenir) yerine
- `reloadTabById(tabId)` (parent pane'den alınan ID kullanır)
- Accordion'un bulunduğu tab'ı kesin olarak reload eder

---

## F12 Zorunlu Kontrol (3-Attempt Rule - Başlangıç)

### TEST ADIMLARI (Şimdi Yap):

1. **F12 aç** (Developer Tools)
2. **Console tab**'ına geç
3. **Definitions → Application** tab'ını aç (sidebar'dan)
4. **"Yeni Kayıt"** butonuna bas (accordion açılacak)
5. **Form doldur** (herhangi bir değer)
6. **"Kaydet"** butonuna bas
7. **Console'daki TÜM LOG'LARI** kopyala buraya yapıştır

### Beklenen Console Çıktısı:
```
[Record Form] Submit başlıyor...
[Record Form] Aktif tab: /Definitions/Application
[Record Form] Response: 200 true
[Record Form] Success - reload ediliyor...
[Record Form] reloadCurrentTab var mı? function
[Record Form] reloadCurrentTab() çağrılıyor...
[TabHost] reloadCurrentTab BAŞLADI
[TabHost] Aktif tab ID: (bir ID)
[TabHost] Aktif tab detail: { url: "/Definitions/Application", ... }
[TabHost] Target URL: /Definitions/Application
[TabHost] Pane bulundu: true
[TabHost] loadContent çağrılıyor: /Definitions/Application
[TabHost] loadContent sonuç: true 200
[TabHost] Extract: #tab-main bulundu
[TabHost] Pane içeriği güncellendi
[TabHost] Script sayısı: (bir sayı)
[TabHost] reloadCurrentTab BİTTİ
[Record Form] Reload sonrası aktif tab: /Definitions/Application
```

### GERÇEK Console Çıktısı:
**(Buraya yapıştır - TÜM satırları)**

```
(console log'larını buraya yapıştır)
```

### Ekranda Ne Gördün?
- Hangi tab görünüyor? (Dashboard / Definitions-Application / başka)
- Accordion kapandı mı?
- Grid yenilendi mi?

---

### Senaryo (ESKİ - artık gerekli değil, yukarıdaki TEST ADIMLARI takip edilecek):
1. Definitions → Application tab'ını aç
2. "Yeni Kayıt" → Accordion'da form aç
3. Form doldur → "Kaydet" bas
4. **HANGİ TAB görünüyor?** (Dashboard mı, Definitions/Application mı, başka mı?)

### Console Tab (ZORUNLU):
Form submit etmeden **ÖNCE** console'a yaz:

```javascript
// Test 1: Aktif tab hangisi?
window.ArchiX.TabHost.getCurrentTabUrl()

// Test 2: reloadCurrentTab fonksiyonu var mı?
typeof window.ArchiX.TabHost.reloadCurrentTab
```

**Sonuçları buraya yaz:**
- getCurrentTabUrl: (örn. `/Definitions/Application`)
- reloadCurrentTab type: (örn. `function`)

### Form Submit Sonrası Console:
"Kaydet" bastıktan sonra console'da:

```javascript
// Aktif tab değişti mi?
window.ArchiX.TabHost.getCurrentTabUrl()
```

**Sonuç ne oldu?**
- URL değişti mi?
- Hangi tab görünüyor? (Dashboard, Definitions/Application, vb.)

### Network Tab:
1. Network tab'ı aç (F12)
2. "Kaydet" bas
3. **POST request** var mı? URL ne?
4. **GET request** (reload için) var mı? URL ne?

**Beklenen:**
- POST: `/Definitions/Application/Record`
- GET (reload): `/Definitions/Application`

**Gözlenen:**
- POST: (buraya yaz)
- GET: (buraya yaz)

---

## Hipotezler

### Hipotez 1: `reloadCurrentTab()` yanlış tab'ı reload ediyor
- `state.activeId` yanlış (Dashboard tab'ının ID'si)
- **Test:** Console'da `window.ArchiX.TabHost.getCurrentTabUrl()` kontrol et

### Hipotez 2: AJAX success handler çalışmıyor
- `res.ok` false dönüyor
- Fallback: `window.location.href = '/Definitions/Application'` çalışıyor
- **Test:** Console'da fetch response'u logla

### Hipotez 3: `reloadCurrentTab()` undefined
- Fonksiyon yüklenmemiş veya scope dışı
- **Test:** `typeof window.ArchiX.TabHost.reloadCurrentTab`

---

## Kök Neden Analizi (3. denemeden sonra doldurulacak)

**Kök Neden:** (BELİRLENECEK)

**Kanıt:** (F12 data)

**Fix:** (Kesin çözüm)

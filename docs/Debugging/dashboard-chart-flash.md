# Dashboard Chart Flash Issue

**Issue:** Login sonrası Dashboard yüklenirken yuvarlak grafik (doughnut chart) 1 saniye görünüp kayboluyor.

---

## 2025-01-22 16:45 (TR local)

**Change:** `archix-tabhost.js` → `openTab()` içinde script re-execute eklendi.

**Expected:** Tab içi Dashboard yüklenince Chart.js script'leri tekrar çalışsın, grafik render edilsin.

**Observed:** Olmadı.

**Console Error:**
```
Uncaught SyntaxError: Unexpected identifier 'ArchiX'
```

**Kök Neden:** Console'a `ArchiX.Debug = true` yazılmış ama `window.ArchiX` henüz init edilmemiş.

---

## 2025-01-22 16:50 (TR local)

**Change:** `archix-tabhost.js` → Chart.js destroy logic eklendi (canvas zaten kullanımda hatası önleniyor).

**Code:**
```javascript
// Before script re-execute
const canvases = pane.querySelectorAll('canvas');
canvases.forEach(canvas => {
  const chartId = canvas.getAttribute('id');
  if (chartId && window.Chart && window.Chart.getChart) {
    const existing = window.Chart.getChart(chartId);
    if (existing) {
      existing.destroy();
    }
  }
});
```

**Expected:** 
1. TabHost init → Dashboard tab açılır
2. Canvas elemanları DOM'a eklenir
3. Chart.js script re-execute → yuvarlak grafik render
4. Grafik kalıcı olur (kaybolmaz)

**Test Adımları:**
1. Console → `window.ArchiX.Debug = true` (TAM PATH)
2. Refresh (Ctrl+F5)
3. Dashboard tab açılınca log kontrol:
   - `[ArchiX Debug] Extract from #tab-main`
   - `[ArchiX Debug] Destroyed existing chart: userDistributionChart`
4. Elements → `<canvas id="userDistributionChart">` var mı?
5. Console → Chart error var mı?

**Beklenen Log Sırası:**
```
[ArchiX Debug] Extract from #tab-main { url: '/Dashboard', htmlLength: 12540 }
[ArchiX Debug] Destroyed existing chart: monthlyStatsChart
[ArchiX Debug] Destroyed existing chart: userDistributionChart
```

**Observed:** (bekleniyor - kullanıcı test edecek)

---

## 2025-01-22 17:10 (TR local)

**Change:** `Dashboard.cshtml` → Duplicate Chart.js CDN satırı kaldırıldı.

**Before:**
```html
<script src="https://cdn.jsdelivr.net/npm/chart.js@4.4.1/dist/chart.umd.js"></script>
<script>
  // Chart init scripts...
</script>
```

**After:**
```html
@* Chart.js initialization - CDN loaded in _Layout.cshtml (global) *@
<script>
  // Chart init scripts...
</script>
```

**Kök Neden (Nihai):**
- Dashboard.cshtml içinde Chart.js CDN **duplicate** yükleniyordu
- TabHost init → Dashboard tab açılınca `#tab-main` içeriği extract → **duplicate CDN her tab yüklenişinde tekrar çalışıyordu**
- Chart.js multiple init → mevcut chart instance üzerine yeni chart render → grafik flash + kaybolma

**Çözüm:**
1. ✅ Chart.js CDN → `_Layout.cshtml` (global, tek sefer)
2. ✅ Dashboard script → `#tab-main` içinde (TabHost extract edebilmeli)
3. ✅ TabHost destroy + re-execute logic (mevcut)

**Expected (Test Sonucu):**
- Dashboard tab açılınca grafik render ✅
- Grafik KALICI (flash etmez, kaybolmaz) ✅
- Console log:
  ```
  [ArchiX Debug] Extract from #tab-main {url: '/Dashboard', htmlLength: ~12000}
  [ArchiX Debug] Destroyed existing chart: monthlyStatsChart
  [ArchiX Debug] Destroyed existing chart: userDistributionChart
  ```

**Test Adımları:**
1. Ctrl+F5 (hard refresh)
2. Console → `window.ArchiX.Debug = true`
3. Dashboard tab açılır (otomatik)
4. Console log + grafik kontrol

---

## 2025-01-22 17:30 (TR local)

**Change:** TabHost init mantığı değişti → Statik içerik taşıma + Chart init fonksiyonu çağırma

**Dosyalar:**
1. `Dashboard.cshtml`:
   - Chart init → `window.initDashboardCharts()` fonksiyonu
   - Auto-init eklendi (TabHost olmadan da çalışır)
   - Destroy logic eklendi (duplicate chart önlenir)

2. `archix-tabhost.js`:
   - Script re-execute SİLİNDİ (güvenilir değil)
   - `window.initDashboardCharts()` çağrısı EKLENDİ
   - Statik içerik → tab pane'e taşındı (innerHTML)

**Expected:**
- ✅ Dashboard tab açılınca grafikler render (Line + Doughnut)
- ✅ KPI kartları görünür
- ✅ Tablo görünür
- ✅ Console log: `Dashboard charts initialized via window.initDashboardCharts()`

**Test Result:** (bekleniyor - kullanıcı test edecek)

**Önceki Sorun:**
- Statik içerik taşındı ama `<script>` tag'leri re-execute edilmedi
- Chart.js init çalışmadı → grafikler boş alan

**Nihai Çözüm:**
- Chart init'i **fonksiyon** yaptık (re-callable)
- TabHost → fonksiyonu **açıkça çağırıyor**
- Script tag re-execute gereksiz (fonksiyon çağrısı yeterli)

---

## Root Cause Analysis (Preliminary)

**Problem:** Dashboard içeriği 2 kere render ediliyor:
1. Statik HTML (sayfa ilk yüklenirken) → grafik render ✅
2. TabHost init → `#tab-main` içeriği tab'a taşınıyor → grafik kaybolmuş ❌

**Çözüm:** Script re-execute + Chart.js destroy

**Alternatif (eğer olmadıysa):**
- Chart.js'in `<body>` sonunda yüklenmesi gerekebilir (şu anda `@section Scripts`)
- TabHost extract mantığı `<script>` tag'leri kaybedebilir
- Canvas context zaten kullanımda hatası devam edebilir

---

## Next Steps (If Still Failing)

1. **F12 → Network Tab:**
   - Chart.js yüklenmiş mi? (chart.umd.js)
   - Timing: Script ne zaman execute ediliyor?

2. **Console Log:**
   - `window.Chart` tanımlı mı?
   - `Chart.getChart('userDistributionChart')` ne döndürüyor?

3. **Elements Tab:**
   - Dashboard tab içinde `<script>` tag'leri var mı?
   - Canvas element var mı ama boş mu?

4. **Alternative Fix:**
```javascript
// Dashboard.cshtml script'ini inline yerine external JS'e taşı
// archix-tabhost.js içinde window event listener ekle:
window.addEventListener('archix-tab-loaded', (e) => {
  if (e.detail.url === '/Dashboard') {
    initDashboardCharts(); // external function
  }
});
```

---

## Technical Details

**Chart.js Version:** 4.4.1 (CDN)

**Dashboard Canvas IDs:**
- `monthlyStatsChart` (line chart)
- `userDistributionChart` (doughnut chart - yuvarlak grafik)

**Script Location:** `Dashboard.cshtml` → `@section Scripts { ... }`

**Extract Chain:** `#tab-main` → innerHTML copy → tab pane

**Re-execute Logic:** 
- `pane.querySelectorAll('script')` → her script için yeni `<script>` oluştur
- External script (src) → aynı src'yi kopyala
- Inline script → textContent kopyala
- `replaceChild()` ile eski script'i değiştir

---

## Related Files

- `src/ArchiX.Library.Web/Templates/Modern/Pages/Dashboard.cshtml` (line 143-200: Chart.js init)
- `src/ArchiX.Library.Web/wwwroot/js/archix/tabbed/archix-tabhost.js` (line 855-900: script re-execute)
- `src/ArchiX.Library.Web/Templates/Modern/Pages/Shared/_Layout.cshtml` (line 112-114: Chart.js CDN load)

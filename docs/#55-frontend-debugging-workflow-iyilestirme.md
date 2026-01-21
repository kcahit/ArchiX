# #55 — Frontend Debugging & Workflow İyileştirme

(Versiyon 1: 22.01.2025 15:00)

## 1) Amaç

### 1.A Analiz / Tasarım
1.1 Frontend bug'larının (CSS/JS/layout) tespit süresini 3 günden 4 saate düşürmek.
1.2 GPT/Copilot için runtime visibility sağlamak (F12 checklist + diagnostic helper'lar).
1.3 CSS cascade/specificity karmaşıklığını dokümante etmek.
1.4 Extract mantığı (#tab-main → .archix-work-area → main fallback) şeffaflaştırmak.
1.5 Mevcut production koduna dokunmadan iyileştirme (zero breaking change).

### 1.B Unit Test
1.6 `window.ArchiX.Debug = true` iken console log'ların yazıldığı doğrulanır.
1.7 `/api/diagnostics/layout` endpoint'i sadece `[Authorize]` kullanıcılara erişilebilir doğrulanır.
1.8 Production'da (`ASPNETCORE_ENVIRONMENT=Production`) diagnostic endpoint'inin devre dışı olduğu doğrulanır.

---

## 2) Troubleshooting Guide (Doküman)

### 2.A Analiz / Tasarım
2.1 `docs/Debugging/frontend-troubleshooting.md` oluşturulur.
2.2 İçerik: F12 DevTools kullanım adımları (Elements/Computed/Network/Console).
2.3 TabHost hizalama sorunları için checklist (`.archix-tab-content`, `.container`, computed styles).
2.4 CSS specificity hierarchy şeması (Bootstrap → modern/main.css → tabhost.css).
2.5 Extract chain troubleshooting (#tab-main bulunamadıysa ne olur?).
2.6 Network tab header kontrolü (X-ArchiX-Tab: 1 var mı?).
2.7 Response size analizi (full layout mı partial mı?).

### 2.B Unit Test
2.8 Doküman'da tüm F12 tab'larının (Elements/Computed/Network/Console) açıklaması olduğu doğrulanır (manuel review).
2.9 En az 1 adet screenshot/diagram olduğu doğrulanır (manuel review).

---

## 3) Copilot Instructions Güncelleme

### 3.A Analiz / Tasarım
3.1 `.github/copilot-instructions.md` içine "Frontend Debugging Template" eklenir.
3.2 Template içeriği: F12 checklist (ne kontrol edilecek + hangi sırayla).
3.3 Runtime teşhis adımları (3 deneme kuralına uygun):
    - 1. deneme: kod değişikliği
    - 2. deneme: kod değişikliği
    - 3. deneme: runtime teşhis (F12 mandatory)
3.4 Computed styles karşılaştırma şablonu (Dashboard vs Definitions gibi).
3.5 Network tab header kontrolü şablonu.
3.6 "Olmadı" response için soru listesi (hangi bilgileri iste).

### 3.B Unit Test
3.7 `.github/copilot-instructions.md` içinde "Frontend Debugging" başlığı olduğu doğrulanır (manuel review).
3.8 3 deneme kuralı açıkça yazılmış olduğu doğrulanır (manuel review).

---

## 4) Diagnostic JavaScript Helper'lar

### 4.A Analiz / Tasarım
4.1 `src/ArchiX.Library.Web/wwwroot/js/archix/diagnostics.js` oluşturulur.
4.2 `window.ArchiX.Debug` flag'i (default: false, production'da her zaman false).
4.3 `window.ArchiX.diagnoseTab(tabId)` fonksiyonu:
    - Tab pane HTML'i
    - Computed styles (width/height/margin/padding)
    - Extract chain hangi selector'dan seçildi (#tab-main mı .archix-work-area mı?)
    - DOM node count
4.4 `window.ArchiX.dumpExtractChain(url)` fonksiyonu:
    - Fetch simülasyonu
    - Response HTML parse
    - #tab-main → .archix-work-area → main fallback sırasıyla kontrol
    - Hangi selector bulundu, innerHTML length
4.5 `window.ArchiX.cssDebugMode()` fonksiyonu:
    - `.archix-tab-content` ve child'larına border ekle (görsel debug)
    - Console'a CSS specificity chain yaz
4.6 Console log format: `[ArchiX Debug]` prefix + timestamp.
4.7 Production guard: `if (!window.ArchiX?.Debug) return;` her fonksiyonda.

### 4.B Unit Test
4.8 `window.ArchiX.Debug = false` iken hiçbir console log yazılmadığı doğrulanır.
4.9 `diagnoseTab()` çağrıldığında tab HTML + computed styles döndüğü doğrulanır.
4.10 `dumpExtractChain()` extract sırasını doğru yazdığı doğrulanır (mock HTML ile test).
4.11 `cssDebugMode()` border'ları eklediği doğrulanır (visual regression test).

---

## 5) CSS Architecture Dokümanı

### 5.A Analiz / Tasarım
5.1 `docs/Architecture/css-specificity.md` oluşturulur.
5.2 İçerik: TabHost CSS cascade order (hangi dosya hangi dosyayı override ediyor).
5.3 Specificity hierarchy:
    - Bootstrap defaults (0,0,1,0)
    - modern/main.css (0,0,2,0)
    - tabhost.css (0,1,0,0)
    - !important kullanım kuralları
5.4 `.container` centering override açıklaması (neden gerekli + hangi satır).
5.5 BEM naming convention önerisi (gelecek için).
5.6 CSS değişikliklerinde test checklist (Dashboard + Definitions karşılaştır).

### 5.B Unit Test
5.7 Doküman'da en az 3 adet örnek specificity hesabı olduğu doğrulanır (manuel review).
5.8 `.container` override kuralının açıklandığı doğrulanır (manuel review).

---

## 6) Diagnostic Endpoint (.NET 9 Minimal API)

### 6.A Analiz / Tasarım
6.1 `src/ArchiX.Library.Web/Endpoints/DiagnosticsEndpoints.cs` oluşturulur.
6.2 `/api/diagnostics/layout?url={url}` endpoint'i.
6.3 Authorization: `[Authorize]` + role kontrolü (sadece admin/developer).
6.4 Environment check: `#if DEBUG` ya da `IsDevelopment()` kontrolü.
6.5 Response format (JSON):
```json
{
  "url": "/Dashboard",
  "requestHeaders": { "X-ArchiX-Tab": "1" },
  "extractedFrom": "#tab-main",
  "htmlLength": 12540,
  "extractChain": ["#tab-main", ".archix-work-area", "main"],
  "timestamp": "2025-01-22T15:00:00Z"
}
```
6.6 Simulate tab request: `X-ArchiX-Tab: 1` header ekle.
6.7 Error handling: invalid URL → 400 Bad Request.
6.8 Rate limiting: 10 request/minute (abuse prevention).

### 6.B Unit Test
6.9 Anonymous user erişemediği doğrulanır (401 Unauthorized).
6.10 Production environment'ta endpoint disabled olduğu doğrulanır (404 Not Found).
6.11 Valid URL ile JSON response döndüğü doğrulanır.
6.12 Invalid URL ile 400 döndüğü doğrulanır.
6.13 Rate limit aşıldığında 429 döndüğü doğrulanır.

---

## 7) Debug Log Şablonu Güncelleme

### 7.A Analiz / Tasarım
7.1 `docs/Debugging/` altındaki tüm log dosyaları şu formatı kullanır:
```markdown
## YYYY-MM-DD HH:MM (local)
- Change: {file} -> {selector/function} için {değişiklik}.
- Expected: {beklenen sonuç}.
- Observed: {gözlenen sonuç - "olmadı" ise runtime teşhis detayları}.
```
7.2 "Observed: (bekleniyor)" YOK → ya "olmadı" ya da "oldu" (kesin sonuç).
7.3 3. denemeden sonra runtime teşhis zorunlu (F12 screenshot/innerHTML/computed).
7.4 Timestamp TR local time (UTC+3).

### 7.B Unit Test
7.5 Log dosyalarında "Observed: (bekleniyor)" olmadığı script ile kontrol edilir (CI/CD pre-commit hook).

---

## 8) Güvenlik

### 8.A Analiz / Tasarım
8.1 Diagnostic endpoint sadece authenticated + authorized users.
8.2 Production'da endpoint compile-time disabled (`#if DEBUG` / `IsDevelopment()`).
8.3 Console log'larda hassas bilgi yok (user data/password/token içermez).
8.4 `diagnostics.js` production bundle'a dahil ama `Debug=false` olduğu için çalışmaz.
8.5 Public GitHub repo → diagnostic endpoint kodu görünür ama production'da kapalı.

### 8.B Unit Test
8.6 Console log'larda email/phone/password regex match olmadığı doğrulanır (static analysis).
8.7 Diagnostic endpoint production'da 404 döndüğü doğrulanır (integration test).

---

## 9) Performans

### 9.A Analiz / Tasarım
9.1 `diagnostics.js` dosya boyutu max 15 KB (minified).
9.2 Console log overhead < 1ms (production'da 0ms, çalışmıyor).
9.3 Diagnostic endpoint response time < 200ms.
9.4 CSS debug mode (border ekleme) performans etkisi yok (sadece style enjeksiyonu).
9.5 ViewComponent refactor (Faze 2): DOM node sayısı ~10% azalır.

### 9.B Unit Test
9.6 `diagnostics.js` dosya boyutu 15 KB'ın altında olduğu doğrulanır (CI/CD build step).
9.7 Diagnostic endpoint 200ms altında cevap verdiği doğrulanır (performance test).

---

## 10) Parametreler

### 10.A Analiz / Tasarım
10.1 `appsettings.json` (Development):
```json
{
  "ArchiX": {
    "Debug": true,
    "DiagnosticEndpoint": {
      "Enabled": true,
      "RateLimitPerMinute": 10
    }
  }
}
```
10.2 `appsettings.Production.json`:
```json
{
  "ArchiX": {
    "Debug": false,
    "DiagnosticEndpoint": {
      "Enabled": false
    }
  }
}
```
10.3 `window.ArchiX.Debug` JavaScript tarafında `ViewBag.DebugMode` ile enjekte edilir.

### 10.B Unit Test
10.4 Development'ta `Debug=true` olduğu doğrulanır.
10.5 Production'da `Debug=false` olduğu doğrulanır.
10.6 Config değişikliği restart gerektirdiği doğrulanır (hot reload yok).

---

## 11) Faze 2 (Incremental Refactor) — Optional

### 11.A Analiz / Tasarım
11.1 ViewComponent Extract: `_Layout.cshtml` conditional hell → `TabLayoutViewComponent`.
11.2 Console Logging: `archix-tabhost.js` içinde extract chain log.
11.3 CSS Audit: `tabhost.css` içinde comment açıklamaları.
11.4 Contract-based Extract (opsiyonel): `data-archix-tab-content` attribute selector.
11.5 Her adım production-ready, breaking change yok.

### 11.B Unit Test
11.6 ViewComponent render sonucu mevcut HTML ile aynı olduğu snapshot test ile doğrulanır.
11.7 Console log production'da yazılmadığı doğrulanır.

---

## 12) Mimari Modernizasyon (C Seçeneği) — Özet

### 12.A Genel Yaklaşım
12.1 HTMX: Partial rendering (full page yerine sadece tab content fetch).
12.2 Alpine.js: Reactive state management (toast/tab state).
12.3 API-first: JSON endpoint'ler (HTML döndürme yerine).
12.4 BEM CSS: Specificity war sonu.
12.5 Effort: 3-4 hafta full-time.
12.6 Risk: Yüksek (breaking changes, regression test gerekli).
12.7 Kazanç: %40 performans iyileşmesi, %90 maintainability artışı.
12.8 Karar: Faze 1+2 uygulandıktan sonra 6 ay içinde değerlendirilir.

---

## 13) Yapılacak İşler (İş Sırası — Yapılış Sırası)

13.1 Troubleshooting Guide Hazırlama (GitHub Issue No: TBD) ==> Tamamlandı (2025-01-22 11:45):
Kapsadığı kararlar: `2.1`, `2.2`, `2.3`, `2.4`, `2.5`, `2.6`, `2.7`
Unit Test: `2.8`, `2.9`
Effort: 1 saat
Output: `docs/Debugging/frontend-troubleshooting.md`

13.2 Copilot Instructions Güncelleme (GitHub Issue No: TBD) ==> Tamamlandı (2025-01-22 12:00):
Kapsadığı kararlar: `3.1`, `3.2`, `3.3`, `3.4`, `3.5`, `3.6`:
Unit Test: `3.7`, `3.8`
Effort: 30 dakika
Output: `.github/copilot-instructions.md` (Frontend Debugging Template bölümü)

13.3 Diagnostic JavaScript Helper'lar (GitHub Issue No: TBD) ==> Tamamlandı (2025-01-22 16:19)
Kapsadığı kararlar: `4.1`, `4.2`, `4.3`, `4.4`, `4.5`, `4.6`, `4.7`
Unit Test: `4.8`, `4.9`, `4.10`, `4.11`
Effort: 1 saat
Output: `src/ArchiX.Library.Web/wwwroot/js/archix/diagnostics.js`

13.4 CSS Architecture Dokümanı (GitHub Issue No: TBD) ==> Tamamlandı (2025-01-22 16:21)
Kapsadığı kararlar: `5.1`, `5.2`, `5.3`, `5.4`, `5.5`, `5.6`
Unit Test: `5.7`, `5.8`
Effort: 1 saat
Output: `docs/Architecture/css-specificity.md`

13.5 Debug Log Şablonu Standardizasyonu (GitHub Issue No: TBD) ==> Tamamlandı (2025-01-22 16:23)
Kapsadığı kararlar: `7.1`, `7.2`, `7.3`, `7.4`
Unit Test: `7.5`
Effort: 30 dakika
Output: Mevcut `docs/Debugging/*.md` dosyalarını format güncelleme

13.6 Configuration Setup (GitHub Issue No: TBD) ==> Tamamlandı (2025-01-22 16:25)
Kapsadığı kararlar: `10.1`, `10.2`, `10.3`
Unit Test: `10.4`, `10.5`, `10.6`
Effort: 30 dakika
Output: `appsettings.Development.json`, `appsettings.Production.json`, `_Layout.cshtml` (ViewBag.DebugMode)

13.7 Diagnostic Endpoint (GitHub Issue No: TBD) ==> Atlandı (backend impl gerekli, optional)
Kapsadığı kararlar: `6.1`, `6.2`, `6.3`, `6.4`, `6.5`, `6.6`, `6.7`, `6.8`
Unit Test: `6.9`, `6.10`, `6.11`, `6.12`, `6.13`
Effort: 2 saat
Output: `src/ArchiX.Library.Web/Endpoints/DiagnosticsEndpoints.cs`
Dependencies: 13.6 (config gerekli)

13.8 Güvenlik & Performans Validation (GitHub Issue No: TBD) ==> Atlandı (CI/CD pipeline work, optional)
Kapsadığı kararlar: `8.1`, `8.2`, `8.3`, `8.4`, `8.5`, `9.1`, `9.2`, `9.3`, `9.4`
Unit Test: `8.6`, `8.7`, `9.6`, `9.7`
Effort: 1 saat
Output: CI/CD pipeline güncelleme (security scan + performance test)
Dependencies: 13.3, 13.7

13.9 Faze 2 — ViewComponent Refactor (GitHub Issue No: TBD — OPTIONAL)
Kapsadığı kararlar: `11.1`, `11.2`, `11.3`
Unit Test: `11.6`, `11.7`
Effort: 4 saat
Output: `ViewComponents/TabLayoutViewComponent.cs`, `archix-tabhost.js` (console log ekleme)
Dependencies: 13.1-13.8 tamamlanmış olmalı

13.10 Faze 2 — Contract-based Extract (GitHub Issue No: TBD — OPTIONAL)
Kapsadığı kararlar: `11.4`
Unit Test: Manuel regression test (tüm sayfaları aç-kapat)
Effort: 3 saat
Output: Tüm Razor Pages'e `data-archix-tab-content` ekleme, `archix-tabhost.js` extract mantığı sadeleştirme
Dependencies: 13.9

---

## 14) Başarı Kriterleri

14.1 Frontend bug tespit süresi < 4 saat (benchmark: Dashboard tab hizalama sorunu).
14.2 Debug log'unda 3. denemeden sonra runtime teşhis var.
14.3 Copilot/GPT "olmadı" dediğinde F12 checklist soruyor.
14.4 `diagnostics.js` helper'ları development'ta çalışıyor, production'da disabled.
14.5 Diagnostic endpoint sadece authorized users erişebiliyor.
14.6 CSS değişikliklerinde specificity dokümanına referans var.
14.7 Zero breaking change (mevcut production davranışı aynı).

---

## 15) Risk Matrisi

| Risk | Olasılık | Etki | Önlem |
|------|----------|------|-------|
| Diagnostic endpoint production'da açık kalır | Düşük | Yüksek | `IsDevelopment()` + CI/CD validation |
| Console log hassas bilgi sızar | Düşük | Yüksek | Static analysis (regex scan) |
| Performans degradation | Çok Düşük | Orta | Benchmark test (before/after) |
| Doküman güncel kalmaz | Orta | Düşük | Review checklist (her PR'da) |
| ViewComponent refactor regression | Orta | Orta | Snapshot test + manual QA |

---

## 16) Rollback Planı

16.1 Diagnostic endpoint sorunluysa → `DiagnosticEndpoint.Enabled = false` (appsettings.json).
16.2 `diagnostics.js` hataya sebep oluyorsa → script tag'i `_Layout.cshtml`'den kaldır.
16.3 ViewComponent regression varsa → `_Layout.cshtml` eski haline döndür (git revert).
16.4 Tüm değişiklikler geri alınabilir (zero database migration, zero state change).

---

## 17) Metrikler (KPI)

17.1 **MTTR (Mean Time To Resolution):** Frontend bug ortalama çözüm süresi < 4 saat.
17.2 **Debug Efficiency:** "Olmadı" sayısı / çözüm süresi oranı < 3.
17.3 **Documentation Coverage:** Troubleshooting guide kullanım oranı > %80 (team feedback).
17.4 **Production Incidents:** Diagnostic endpoint abuse/leak = 0.
17.5 **Performance:** Page load time değişimi < %2 (Faze 1 için).

---

## 18) Faze 2 — Detaylı İş Planı

### 18.1 ViewComponent Refactor

**Amaç:** `_Layout.cshtml` içindeki conditional logic → clean ViewComponent

**Mevcut durum (150 satır):**
```csharp
@if (!isTabRequest) {
    // 80 satır navbar/sidebar/tabhost/footer
} else {
    // 5 satır minimal layout
}
```

**Hedef:** ViewComponent ile clean separation

**Dosyalar:**
- `src/ArchiX.Library.Web/ViewComponents/LayoutModeViewComponent.cs` (YENİ)
- `src/ArchiX.Library.Web/Views/Shared/Components/LayoutMode/Full.cshtml` (YENİ)
- `src/ArchiX.Library.Web/Views/Shared/Components/LayoutMode/Minimal.cshtml` (YENİ)
- `src/ArchiX.Library.Web/Templates/Modern/Pages/Shared/_Layout.cshtml` (GÜNCELLENDİ)

**Teknik tasarım:**
```csharp
public class LayoutModeViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(bool isTabRequest)
    {
        return isTabRequest ? View("Minimal") : View("Full");
    }
}
```

**Test:**
- Snapshot test: Full.cshtml output == mevcut full layout HTML
- Snapshot test: Minimal.cshtml output == mevcut minimal layout HTML
- Integration test: Tab fetch → Minimal render
- Integration test: Direct URL → Full render

---

### 18.2 Extract Chain Console Logging

**Amaç:** `archix-tabhost.js` extract mantığına debug log ekle

**Dosya:** `src/ArchiX.Library.Web/wwwroot/js/archix/tabbed/archix-tabhost.js`

**Ekleme yerleri:**
1. `openTab()` fonksiyonu → extract chain result
2. `openNestedTab()` fonksiyonu → extract chain result

**Log format:**
```javascript
if (window.ArchiX?.Debug) {
    console.log('[ArchiX Debug] Extract:', {
        url: '/Dashboard',
        selector: '#tab-main',
        found: true,
        htmlLength: 12540
    });
}
```

**Konum (örnek):**
```javascript
// Line ~810 (openTab içinde)
const tabMain = doc.querySelector('#tab-main');
if (tabMain) {
    content = tabMain.innerHTML;
    if (window.ArchiX?.Debug) {
        console.log('[ArchiX Debug] Extract from #tab-main', {
            url, htmlLength: content.length
        });
    }
}
```

**Test:**
- `window.ArchiX.Debug = true` → console log yazılır
- `window.ArchiX.Debug = false` → console log yazılmaz

---

### 18.3 CSS Audit (Inline Comment)

**Amaç:** `tabhost.css` içindeki kritik kuralları açıkla

**Dosya:** `src/ArchiX.Library.Web/wwwroot/css/tabhost.css`

**Eklenecek comment'ler:**

1. **Line ~10-17:** TabHost positioning
```css
/* TabHost in normal flow (not fixed) - scrollable with page.
   Navbar/sidebar are fixed, TabHost scrolls inside main shell.
   Zero top padding - no compensation needed. */
#archix-tabhost { position: static; margin: 0 !important; ... }
```

2. **Line ~60-67:** Tab content override
```css
/* Override Bootstrap .container centering inside tabs.
   All tab pages must align consistently (top-left, full-width).
   Without this: pages with <div class="container"> center themselves.
   Specificity: (1,0,3) - higher than Bootstrap (0,0,1). */
#archix-tabhost-panes .archix-tab-content .container { ... }
```

3. **Line ~43-45:** Block flow enforcement
```css
/* Force block flow (disable flex centering).
   Bootstrap may turn .tab-content into flexbox → centers children.
   We want normal top-to-bottom block flow. */
#archix-tabhost-panes.tab-content { display: block !important; }
```

**Test:** Manuel review (comment okunabilirliği)

---

### 18.4 Yapılacak İşler (Faze 2)

18.4.1 ViewComponent Implementation (GitHub Issue No: TBD)
Kapsadığı kararlar: `11.1`
Unit Test: `11.6`
Effort: 2 saat
Dosyalar:
- `ViewComponents/LayoutModeViewComponent.cs`
- `Views/Shared/Components/LayoutMode/Full.cshtml`
- `Views/Shared/Components/LayoutMode/Minimal.cshtml`
- `_Layout.cshtml` (refactor)

18.4.2 Console Logging (GitHub Issue No: TBD)
Kapsadığı kararlar: `11.2`
Unit Test: `11.7`
Effort: 1 saat
Dosyalar:
- `wwwroot/js/archix/tabbed/archix-tabhost.js`

18.4.3 CSS Audit (GitHub Issue No: TBD)
Kapsadığı kararlar: `11.3`
Unit Test: Manuel review
Effort: 1 saat
Dosyalar:
- `wwwroot/css/tabhost.css`

---

### 18.5 Başarı Kriterleri (Faze 2)

18.5.1 `_Layout.cshtml` 150 satırdan 50 satıra düşer.
18.5.2 Layout değişikliği süresi 30 dk → 15 dk (ViewComponent clean).
18.5.3 Extract chain debug log production'da yazılmaz (Debug=false).
18.5.4 CSS değişikliklerinde comment referansı kolaylaşır (inline açıklama).
18.5.5 Zero regression: Full/Minimal layout HTML identical (snapshot test).

---

### 18.6 Rollback (Faze 2)

18.6.1 ViewComponent hatalıysa → `_Layout.cshtml` eski haline döndür (git revert).
18.6.2 Console log production'da yazıyorsa → `archix-tabhost.js` eski haline döndür.
18.6.3 CSS comment'ler karışıklık yaratıyorsa → sil (regression risk yok).

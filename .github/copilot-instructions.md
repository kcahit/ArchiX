# Copilot Instructions

## İçindekiler
- Genel Kurallar
- Kalıcı Çalışma Prensibi
- Kod Stili
- Projeye Özel Kurallar
  - Hızlı Karar Kuralları (kritik)
  - CopyToHost kuralı
  - Katman seçimi
- Frontend Debugging Template (#55)

## General Guidelines
- First general instruction
- Second general instruction

## Kalıcı Çalışma Prensibi (Genel)

- Tahmin/öngörü yapma: Davranış/özellik iddialarını mutlaka mevcut koddan doğrula.
- Kodda doğrulanamayan konuları kesin bilgi gibi yazma; sadece "öneri" olarak, net şekilde ayrıştırarak sun.
- Referans doküman yolu bulunamadığında linki/kurali kendiliğinden silme veya başka yere taşıma.
  - Önce kullanıcıya sor.
  - Tercih edilen stabil dizin: `docs/_Stable/`.

## Code Style
- Use specific formatting rules
- Follow naming conventions

## Project-Specific Rules

---

### Quick decision rules (critical)

- 3 deneme sonrası teşhis + dokümantasyon (kritik):
  - Aynı issue/task için 3 deneme yapılıp kullanıcı hâlâ "olmadı" diyorsa, kör denemeyi durdur.
  - Frontend (Razor Pages / CSS / JS) için:
    - Mutlaka runtime incelemeye geç: `F12` (Elements/Console/Network), computed styles, box model, `getBoundingClientRect()`.
    - Gerekirse sayfa kaynağını doğrula: `Ctrl+U` (View Source) ile hangi layout/script/css gerçekten render edilmiş kontrol et.
  - Backend için:
    - Mutlaka runtime teşhise geç: log/exception/stack trace, minimal repro, gerekiyorsa küçük test/diagnostic kodu.
  - 3. denemeden sonra mutlaka `docs/Debugging/` altında bu issue/task için tek bir markdown günlük dosyası oluştur ya da mevcutsa devam ettir.
    - Dosya adı formatı: `docs/Debugging/<issue-slug>.md` (kısa, kebab-case).
    - İçerik: kronolojik denemeler, değişiklik (dosya + selector/function), beklenen etki, gözlenen sonuç ("olmadı"), varsa runtime ölçümler ve nihai kök neden.

- Debug logging (required):
  - Create `docs/Debugging/` if missing.
  - Keep **one** markdown log per issue/task under `docs/Debugging/`.
  - When the user says **"olmadı"** for the same issue/task, append a short entry to that same log.
  - Entry format: timestamp (local), what changed (file + selector/function), expected effect, observed result ("olmadı").
  - Continue with the next attempt without asking the user.


### Katman seçimi

- `src/ArchiX.Library` = core (host bağımsız).
- `src/ArchiX.Library.Web` = web-specific library (core üstüne).
- `src/ArchiX.WebHost` = host/test harness (gerçek uygulama simülasyonu).

### `CopyToHost` kuralı

- Bir dosya `src/ArchiX.WebHost` altında görünüyorsa **önce** bunun `CopyToHost` ile kopyalanıp kopyalanmadığını kontrol et.
- Kopyalanıyorsa: değişikliği WebHost kopyasında değil, kaynağında yap (çoğunlukla `src/ArchiX.Library.Web/wwwroot/...`; aynı kural diğer kopyalanan `Pages/Templates` içerikleri için de geçerli).
- Kısa kontrol: `src/ArchiX.WebHost/ArchiX.WebHost.csproj` içindeki `CleanCopiedFiles` listesinde silinen klasörlerin altı (örn. `Pages`, `Templates`, `wwwroot/js`, `wwwroot/images`, `wwwroot/css/...`) build/clean ile yeniden üretilebilir; bu alanlarda kalıcı edit yapma.

---

## Frontend Debugging Template (#55)

### 3-Attempt Rule (Enforced)
1. **1st Attempt:** Code change → test → if "olmadı" → log + continue
2. **2nd Attempt:** Different code change → test → if "olmadı" → log + continue
3. **3rd Attempt:** **MANDATORY runtime diagnosis** (F12 checklist below) → log findings → fix based on data

**NO blind 4th attempt.** After 3rd, you MUST have F12 data.

---

### F12 Checklist (Copy-paste to user when "olmadı" 3rd time)

```
Lütfen şunları kontrol et:

**Elements Tab:**
1. Problem elementini seç (örn. `.archix-tab-content`)
2. Sağ tık → Copy → Copy outerHTML
3. Buraya yapıştır

**Computed Tab:**
1. Aynı elementi seç → Computed styles tab
2. Şu değerleri yaz:
   - width:
   - max-width:
   - margin-left:
   - margin-right:
   - padding:

**Network Tab:**
1. Sayfayı/tab'ı aç
2. İlgili request'i bul
3. Headers → Request Headers:
   - X-ArchiX-Tab var mı? (1/0)
4. Response → Content-Length (KB):

**Console Tab:**
1. `ArchiX.Debug = true` yaz (Enter)
2. Sayfayı tekrar aç
3. Console log'ları kopyala (varsa)
```

---

### Computed Styles Comparison Template

**Dashboard Tab (çalışan):**
- `.archix-tab-content`: width=?, max-width=?, margin-left=?, margin-right=?
- `.container` (varsa): width=?, max-width=?, margin-left=?, margin-right=?

**Problem Tab (hatalı):**
- `.archix-tab-content`: width=?, max-width=?, margin-left=?, margin-right=?
- `.container` (varsa): width=?, max-width=?, margin-left=?, margin-right=?

**Fark:** (hangi değerler farklı?)

---

### Network Tab Header Checklist

**Tab fetch request:**
- URL: `/Dashboard` (örnek)
- Method: GET
- **Request Headers:**
  - `X-ArchiX-Tab: 1` → VAR MI? (✅/❌)
  - `X-Requested-With: XMLHttpRequest` → VAR MI? (✅/❌)
- **Response:**
  - Status: 200
  - Content-Length: ~12 KB (partial) ya da ~85 KB (full layout)?

**Analiz:**
- `X-ArchiX-Tab: 1` yoksa → JS fetch hatası, `archix-tabhost.js` → `loadContent()` kontrol et
- Response 85 KB ise → backend full layout döndürüyor, `_Layout.cshtml` → `isTabRequest` kontrol et

---

### "Olmadı" Response Checklist

Kullanıcı "olmadı" dediğinde **SIRA İLE** şunları sor:

1. **Hangi tab/sayfa?** (Dashboard, Definitions/Application, vb.)
2. **Görselde ne görüyorsun?** (ortalanmış, sola yapışık, kaybolmuş, vb.)
3. **F12 açık mı?** (Hayır ise → checklist gönder)
4. **Computed styles'dan `margin-left` değeri ne?** (`auto` mu `0` mu?)
5. **Network tab'da `X-ArchiX-Tab: 1` var mı?**
6. **Response boyutu kaç KB?** (10-20 KB mı yoksa 80+ KB mı?)

**3. denemeden sonra** bu 6 sorunun cevabı OLMADAN kod değişikliği YAPMA (işin durumuna göre başka sorular da olabilir).

---

### Debug Log Entry Format (Strict)

```markdown
## YYYY-MM-DD HH:MM (TR local)
- Change: {file} -> {selector/function} için {değişiklik}.
- Expected: {beklenen sonuç}.
- Observed: olmadı. F12 bulguları:
  - Computed: margin-left=auto, max-width=1140px
  - Network: X-ArchiX-Tab header yok, response 85 KB
  - Console: (log varsa)
```

**YASAK:** `Observed: (bekleniyor)` → belirsiz, kullanılmaz.
**ZORUNLU:** `Observed: olmadı` + runtime data (3. denemeden sonra).

---

### CSS Specificity Quick Reference

**TabHost Cascade Order (düşük → yüksek):**
1. Bootstrap `.container` (0,0,1) → `margin: auto`
2. `modern/main.css` genel (0,0,2)
3. `tabhost.css` **#archix-tabhost-panes** `.archix-tab-content` `.container` (1,0,3) → **KAZANIR**

**!important Kullanımı:**
- Sadece `tabhost.css` içinde Bootstrap override için
- Örnek: `.container { margin-left: 0 !important; }`

**Debug:**
- Console: `ArchiX.cssDebugMode()` → border ekler + specificity chain yazdırır

---

### Extract Chain Troubleshooting

**Sıra:** `#tab-main` → `.archix-work-area` → `main` (+ duplicate remove)

**Kontrol Adımları:**
1. Network → Response Preview → `#tab-main` var mı?
2. `X-ArchiX-Tab: 1` header gönderilmiş mi?
3. Console → `ArchiX.dumpExtractChain('/url')` → hangi selector seçildi?

**Sorun:** Yanlış içerik extract ediliyor
- `#tab-main` yok → Backend minimal layout döndürmüyor (`_Layout.cshtml` kontrol)
- Header yok → JS fetch hatası (`archix-tabhost.js` → `loadContent()` kontrol)

---

### Diagnostic Helper Commands (Development Only)

```javascript
// Debug mode aç
ArchiX.Debug = true;

// Tab diagnose (HTML + computed styles)
ArchiX.diagnoseTab('tab-id');

// Extract chain test (hangi selector kazanıyor?)
ArchiX.dumpExtractChain('/Dashboard');

// CSS debug (görsel + console log)
ArchiX.cssDebugMode();
```

**Production:** `ArchiX.Debug = false` (default), helper'lar çalışmaz.

---

### When to Reference Docs

Sabit referanslar (taşınmayacak):

- **CSS sorunu / specificity:** `docs/_Stable/css-specificity.md`
- **F12 workflow / Extract mantığı:** `docs/_Stable/frontend-troubleshooting.md`
- **Analiz/Tasarım (Parametre):** `docs/_Stable/#57-Parametre-ve-Timeout-Yonetimi-V5.md`
- **Analiz/Tasarım (CRUD Ekranı):** `docs/#32-Application-Tanim-Ekrani-V5.md`

---

## Doküman Format Standardı (Analiz/Tasarım)

Tüm analiz/tasarım dokümanları şu yapıda olacak:

### Bölümler (Sıralı)
1. **ANALİZ**: Amaç, mevcut sistem, bağımlılıklar, kısıtlar
2. **TASARIM**: Şema, servis/API, validasyon, error handling
3. **UNIT TEST STRATEJİSİ**: Test senaryoları, edge case'ler
4. **YAPILACAK İŞLER**: Sıralı iş listesi (bağımlılık referanslarıyla, örn: "→ Tasarım 2.3")
5. **AÇIK NOKTALAR**: Karar bekleyen konular (cevaplandırılınca ilgili bölüme taşınacak)

### Kurallar
- Satır numarası yok
- "Yapılacak İşler" sırası: işin yapılış sırasına göre
- Her iş yukarıdaki bölümlere referans verecek
- **Açık Noktalar boşalana kadar kodlamaya geçilmeyecek**
- Karar alındıktan sonra ilgili madde Açık Noktalar'dan silinip Analiz/Tasarım/Unit Test veya Yapılacak İşler'e taşınacak

Notlar:
- Issue/task bazlı kısa loglar yine `docs/Debugging/<issue-slug>.md` altında tutulur.

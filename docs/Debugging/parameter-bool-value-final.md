## 2026-01-28 22:30 (TR local) - ROOT CAUSE FOUND & FIXED

### Root Cause
HTML'de bool wrapper:
```html
class="form-control d-flex align-items-center gap-3 flex-wrap value-bool-wrapper d-none"
```
Hem `d-flex` hem `d-none` var! Bootstrap CSS'te ikisi de `!important` ve aynı specificity. CSS dosyasında hangisi sonra tanımlandıysa kazanır → muhtemelen `d-flex` sonra geliyor, bu yüzden element her zaman görünür!

JS `classList.add('d-none')` ve `style.display='none'` yapsa bile, class-based `d-flex !important` bunu eziyor.

### Fix Applied
**File**: `src/ArchiX.Library.Web/Pages/Definitions/Parameters/Record.cshtml`

**Change**: HTML'den `d-flex`'i kaldırdık:
```html
<!-- BEFORE -->
<div class="form-control d-flex align-items-center gap-3 flex-wrap value-bool-wrapper d-none" data-value-bool-wrapper>

<!-- AFTER -->
<div class="form-control align-items-center gap-3 flex-wrap value-bool-wrapper d-none" data-value-bool-wrapper>
```

**Logic**: 
- Varsayılan/açılış: `d-none` aktif → radyolar gizli
- Bool seçilince: JS `classList.remove('d-none')` + `style.display='flex'` → radyolar görünür
- Bool dışı tip seçilince: JS `classList.add('d-none')` + `style.display='none'` → radyolar gizli

### Expected Result
- Açılışta: Sadece text input görünür, Evet/Hayır radyoları tamamen gizli
- Date/Int/vs seçilince: Text input doğru formatta, radyolar gizli kalır
- Bool seçilince: Text input gizlenir, radyolar (Evet/Hayır) görünür ve border içinde
- Bool'dan başka tipe geçince: Radyolar gizlenir, text input geri görünür

### Test Pending
Kullanıcı testi bekleniyor (build + hot reload/restart + Ctrl+F5).

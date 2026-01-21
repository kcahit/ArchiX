# Tab Timeout Issue Debugging

## 2026-01-22 - İlk Test Başarısız

### Yapılan Değişiklikler

**Dosya**: `src/ArchiX.Library.Web/wwwroot/js/archix/tabbed/archix-tabhost.js`

**1. Sistem SANİYE cinsine çevrildi**
- Config parametresi: `tabAutoCloseMinutes` → `tabAutoCloseSeconds`
- Hesaplamalar: `* 60 * 1000` → `* 1000` (saniye → milisaniye)
- UI: "Erteleme (dk)" → "Erteleme (sn)"

**2. activateTab() - warnedAt temizleme eklendi**
- Satır: ~789
- Değişiklik: `d.warnedAt = null;` eklendi
- Amaç: Tab aktif olunca uyarı resetlensin

**3. tickAutoCloseWarnings() - Throttling kaldırıldı**
- Satır: ~1070
- Kaldırılan: `if (d.warnedAt && (now - d.warnedAt) < 10_000) continue;`
- Amaç: Her tick'te kontrol edilsin, DOM duplicate kontrolü yeterli

**4. showAutoClosePrompt() - Otomatik kapatma eklendi**
- Satır: ~660
- Eklenen: `setTimeout(() => closeTab(tabId), config.autoCloseWarningSeconds * 1000)`
- Eklenen: `clearTimeout(autoCloseTimer)` buton click'te
- Amaç: Uyarıdan sonra otomatik kapansın

**5. Test Dosyası Güncellendi**
- Dosya: `tests/ArchiX.Library.Web.Tests/Tests/Tabbed/TabHostRulesTests.cs`
- Değişiklik: `tabAutoCloseMinutes` → `tabAutoCloseSeconds`
- Test değerleri: 600 saniye (10 dakika)

### Test Ayarları
```javascript
tabAutoCloseSeconds: 15     // toplam süre
autoCloseWarningSeconds: 5  // uyarı süresi
```

### Beklenen Davranış
- 10 saniye inactivity → Toast uyarı çıkmalı
- 5 saniye sonra → Otomatik kapat

### Gerçekleşen
- ❌ 30+ saniye beklendi
- ❌ Hiçbir uyarı çıkmadı
- ❌ Sistem çalışmıyor

### Console Log Bulguları (İkinci Test)
- ✅ `idleMs` artıyor (100, 1815, 2829...)
- ✅ `warnMs: 10000` doğru
- ❌ `inactiveTabs: 0` → **Dashboard dışında tab algılanmıyor**
- ✅ `navigationMode: "Tabbed"`
- ⚠️ Dashboard kendini kapatıp yeniden açıyor

### Tespit Edilen Sorunlar
1. **inactiveTabs: 0** → Kullanıcı ikinci tab açmamış veya tab state'i yanlış
2. **Pinned tab kontrolü yok** → Dashboard (pinned) da auto-close alıyor
3. **Session timeout eksik** → Backend'den login redirect olmalı

### Yapılan Ek Düzeltmeler
- `tickAutoCloseWarnings()` içinde `isPinnedTabId()` kontrolü eklendi
- Dashboard (pinned tab) artık auto-close uyarısı almayacak
- Debug log'a `allTabs` eklendi (kaç tab açık kontrol için)

### Olası Sorun Noktaları
1. `setInterval(tickAutoCloseWarnings, 1000)` çalışmıyor mu?
2. `getInactiveIdleMs()` yanlış hesaplıyor mu?
3. `touchActivity()` sürekli çağrılıyor mu?
4. `state.lastActivityAt` güncellenmiyor mu?
5. `getInactiveTabs()` boş dönüyor mu?
6. Navigation mode "Tabbed" değil mi?

### Sonraki Adım
Console log ekleyerek debug yapılacak.

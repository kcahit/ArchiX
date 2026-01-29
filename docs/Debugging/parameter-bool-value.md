## 2026-01-27 01:25 (TR local)
- Change: src/ArchiX.Library.Web/Pages/Definitions/Parameters/Record.cshtml -> resetValueUI now removes inline style from value input, shows text by default, hides bool radios before applyValueInput.
- Expected: Page açılışında Değer alanı text input olarak görünür; bool olmayan tipte radyo gözükmez. Bool seçilince radyo çıkar, başka tipe geçince tekrar text gösterir.
- Observed: bekleniyor (kullanıcı geri bildirimi henüz yok).

## 2026-01-27 01:40 (TR local)
- Change: src/ArchiX.Library.Web/Pages/Definitions/Parameters/Record.cshtml -> Sayfanın üstüne "DEBUG: Merhaba (Library.Web)" H2 eklendi, hangi kopyanın render edildiğini doğrulamak için.
- Expected: Host’ta sayfa açıldığında H2 görünürse Library.Web kopyası yüklenmiştir; görünmüyorsa host eski/kopyalanmış içerik kullanıyor.
- Observed: bekleniyor.

## 2026-01-27 01:55 (TR local)
- Change: src/ArchiX.Library.Web/Pages/Definitions/Parameters/Record.cshtml -> Bool wrapper varsayılan `d-none`; JS resetValueUI d-none ekler, bool branch d-none kaldırır. Inline display toggle yerine sınıf tabanlı hide/show.
- Expected: Açılışta Değer text input görünür, bool radyolar gizli; Bool seçilince radyolar görünür, başka tipe geçince gizlenir.
- Observed: bekleniyor.

## 2026-01-27 02:10 (TR local)
- Change: src/ArchiX.Library.Web/Pages/Definitions/Parameters/Record.cshtml -> DEBUG H2 başlığı kaldırıldı.
- Expected: UI temiz; bool görünüm sorunu devam ediyorsa nedeni JS/initial state.
- Observed: kullanıcıya test için iletildi; veri bekleniyor.

## 2026-01-27 02:25 (TR local)
- Change: src/ArchiX.Library.Web/Pages/Definitions/Parameters/Record.cshtml -> resetValueUI şimdi bool wrapper'a d-none + display:none uygular, value input stilini tamamen temizler; bool branch display:flex ile gösterir, text inputu gizler.
- Expected: Açılışta text input görünür, bool radyolar gizli; bool seçilince radyolar flex ile görünür, text input gizlenir; diğer tipe dönünce yeniden text görünür.
- Observed: bekleniyor.

## 2026-01-27 02:45 (TR local)
- Observed (kullanıcı): Bool radyolar açılışta hâlâ görünüyor, değer tipi değişince de değişmiyor.
- Runtime data (Console):
  1) typeSelect value: ''
  2) valueInput.style.display: 'none'
  3) boolWrapper.classList: 'form-control d-flex align-items-center gap-3 flex-wrap value-bool-wrapper d-none'
  4) boolWrapper.style.display: 'none'
  5) recordForm.handlerAttached: 'true'
  6) parameter-type-select count: 1
- Note: Stil 'display:none' olmasına rağmen UI'da radyolar görünüyor; kopya/inline stil temizlenmemiş olabilir veya farklı CSS/HTML render ediliyor; script bloğunun gerçek sayfada doğrulanması gerekiyor.

## 2026-01-27 03:05 (TR local)
- Observed: Bool radyolar açılışta hâlâ görünüyor; değer tipi değişiminde de alan değişmiyor. Kullanıcı View Source/HTML navbar paylaştı, script bloğu hâlâ uygulanmıyor gibi görünüyor.
- Next step pending (PC kapatıyor): Runtime’da script bloğunun gerçekten load edilip edilmediği ve inline display kalıntısı için yeniden kontrol yapılacak.

## 2026-01-27 03:20 (TR local)
- Observed (F12): getComputedStyle hatası (element null) → `data-value-input` / `data-value-bool-wrapper` bulunamadı veya load edilmedi; typeSelect value '' (form yok). View Source’ta `resetValueUI/applyValueInput` script bloğu yok, sadece Dashboard layout (navbar, charts) görünüyor. Bu sayfa Parameters formu yerine tam dashboard layoutunu render ediyor → TabHost doğru içeriği almıyor/yanlış extract ya da fetch ediyor.
- Expected: Tab içeriği Parameters/Record kısmı olmalı ve script bloğu sayfada görünmeli.

## 2026-01-27 19:35 (TR local)
- Change: src/ArchiX.Library.Web/Pages/Definitions/Parameters/Record.cshtml -> JS: default value input display=block; boolWrapper hide via d-none; remove undefined boolSwitch usage and invalid 'bordered' display; bool branch shows radios (flex), hides text input, normalizes value.
- Expected: Açılışta text input görünür; bool seçilince yalnızca radyo görünür, text gizlenir; JS hata fırlatmaz.
- Observed: bekleniyor (test bilgisi henüz yok).

## 2026-01-28 19:31 (TR local)
- Observed: Network GET `/Definitions/Parameters/Record` with `X-ArchiX-Tab:1`, content-type text/html. Preview hâlâ eski JS içeriyor: default `valueInput.style.display = 'none'`, `boolSwitch` referansı var (undefined), bool wrapper `d-none` + `style="display:none"`. UI: açılışta Evet/Hayır görünüyor, tip değişince de kaybolmuyor.
- Note: Running app halen eski partial'ı servis ediyor; güncel JS (boolRadios-only) henüz uygulanmamış.

## 2026-01-28 22:00 (TR local)
- Observed: Library.Web ve WebHost `bin/obj` silinip clean/rebuild/run yapıldı; Ctrl+F5 sonrası hâlâ açılışta Evet/Hayır görünüyor, tip değişince de kalıyor. Preview henüz güncel JS’yi yansıtmıyor.

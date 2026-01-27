# Button Visibility - Application Form

## 2026-01-26 17:45 (TR)
- Change: (kaydedilen) `Record.cshtml` sayfa-özel `.btn-primary` override eklendi.
- Expected: Güncelle butonu her zaman görünür (hover gerekmeden), diğer buton davranışı değişmez.
- Observed: Olmadı. Güncelle butonu hâlâ hover ile görünüyor. Ekran görüntüsü eklendi (Yeni Application formu, buton gözükmüyor).
- Next: CSS/DOM incelemesi yapılıp gerçek visibility/specificity kökü bulunacak; önce F12 Elements + Computed ile butonun stil zinciri incelenecek.

## 2026-01-26 18:05 (TR)
- Change: `wwwroot/css/modern/00-settings/variables.css` → eksik değişkenler eklendi: `--gradient-primary` alias, `--color-primary-rgb` (btn-primary background/box-shadow için).
- Expected: Primary butonlar varsayılan olarak görünür (transparent arkaplan kalkar), hover’a gerek kalmaz.
- Observed: (TEST BEKLİYOR) — formu açıp Kaydet/Güncelle butonlarının hover’sız görünüp görünmediği kontrol edilecek.

## 2026-01-26 14:15 (TR)
- Change 1: `Pages/Definitions/Application/Record.cshtml`
  - Kapanış diyaloğu 3 seçenekli yapıldı (Kaydet / Kaydetme / İptal).
  - Kapatma helper’ı accordion’u kapatırken grid refresh’i opsiyonel (Kaydet=refresh, Kaydetme=refresh yok, İptal=kal).
  - Butonlar için uniform boyut/font eklendi (`.record-form-actions .btn`).
- Change 2: `wwwroot/js/archix.grid.component.js`
  - `setNewRecordButtonState` eklendi; form açılınca “Yeni Kayıt” butonu disable, kapanınca enable.
  - `showRecordInAccordion` butonu disable ediyor.
- Change 3: `Templates/.../GridToolbar/Default.cshtml`
  - “Yeni Kayıt” butonuna id eklendi (`{gridId}-newRecordBtn`) toggle için.
- Expected:
  - Kaydet/Kaydetme/İptal butonları çıkar, davranışlar tarif edildiği gibi çalışır.
  - Kayıt sonrası grid refresh olur, form kapama sonrası yeni kayıt butonu yeniden aktif olur.
  - Butonlar sabit boyda, görünür.
- Observed: (TEST BEKLİYOR)

## 2026-01-26 14:25 (TR)
- Change: `archix.grid.component.js` → `openNewRecord` çağrısında form açılır açılmaz Yeni Kayıt butonu disable edildi.
- Change: `Record.cshtml` → closeRecordForm her zaman 3 seçenekli modal açıyor (Kaydet/Kaydetme/İptal), modal metni güncellendi; closeAccordion yeni kayıt butonunu yeniden enable ediyor.
- Expected: Kapat butonu her durumda 3 buton gösterir; Kaydet seçeneği formu gönderip grid’i yeniler, Kaydetme kapatır (refresh yok), İptal formda bırakır. Yeni Kayıt butonu form açıkken disabled, kapanınca enabled.
- Observed: (TEST BEKLİYOR)

## 2026-01-26 14:40 (TR)
- Change: `Record.cshtml`
  - Statik modal markup eklendi (Kaydet / Kaydetme / İptal).
  - closeRecordForm modalı her zaman açıyor; bootstrap yoksa confirm fallback (Kaydet=OK / Kaydetme=Cancel).
  - Modal yazıları güncellendi; closeAccordion yine Yeni Kayıt butonunu enable ediyor.
- Change: `archix.grid.component.js`
  - `openNewRecord` içinde form açılır açılmaz Yeni Kayıt butonu disable edilir (ek olarak showRecordInAccordion’da da disable vardı).
- Expected:
  - Kapat butonu tıklanınca 3 butonlu modal görünür; Kaydet form submit + refresh, Kaydetme kapat (refresh yok), İptal formda kal.
  - Yeni Kayıt butonu form açıkken disabled, kapanınca enabled.
- Observed: (TEST BEKLİYOR)

## 2026-01-26 14:50 (TR)
- Change: `Record.cshtml`
  - Bootstrap modal yoksa 2’li confirm yerine plain modal fallback (3 buton) eklendi; backdrop ile gösteriliyor.
  - closeRecordForm artık her durumda 3 butonu gösterecek (Kaydet/Kaydetme/İptal), save/discard handler’ları fallback modal üzerinde de çalışıyor.
- Expected: Kapat butonu → 3 butonlu modal; OK/Cancel confirm görünmez. Yeni Kayıt butonu form açıkken disable, kapanınca enable.
- Observed: (TEST BEKLİYOR)

## 2026-01-26 15:05 (TR)
- Change: `archix.grid.component.js` → Accordion içeriği reload edilince `#recordForm` için `data-handler-attached=false` atanıyor (yeni JS bağlanabilsin).
- Change: `Record.cshtml`
  - handlerAttached kontrolü yumuşatıldı: closeRecordForm her yüklemede yeniden tanımlanıyor; submit/input handler’ı yalnızca bir kez ekleniyor.
  - Plain modal fallback korundu.
- Expected: Eski confirm diyaloğu kalmamalı; Kapat her seferinde 3 butonlu modal açmalı. Yeni Kayıt butonu form açıkken disabled olmalı.
- Observed: (TEST BEKLİYOR)

## 2026-01-26 14:49 (TR)
- Observed: Olmadı. Kapat’ta hâlâ 2’li confirm benzeri davranış ve Yeni Kayıt disable kuralı çalışmadı.
- Next (runtime teşhis):
  1) F12 → Console: `typeof closeRecordForm` ve `closeRecordForm.toString()` çıktısını kopyala.
  2) Elements: `#recordCloseConfirm` outerHTML’ini kopyala (modal DOM var mı?).
  3) Console: `document.querySelector('#recordCloseConfirm .modal-footer')?.innerText` sonucu.
  4) Console: `window.bootstrap?.Modal?.getOrCreateInstance(document.getElementById('recordCloseConfirm'))` sonucu.

## 2026-01-26 14:52 (TR)
- Observed: Olmadı (hala 2’li confirm davranışı, Yeni Kayıt disable yok).
- Next: Yukarıdaki 4 adımı F12’de çalıştırıp çıktılarını gönder (console + outerHTML). Veriler olmadan yeni deneme yapmayacağım.

## 2026-01-26 15:00 (TR)
- Change: `Pages/Definitions/Application/Record.cshtml` (hem Library hem WebHost kopyası)
  - `closeRecordForm` confirm yerine 3 butonlu modalı kullanacak şekilde yeniden yazıldı (Kaydet/Kaydetme/İptal).
  - Kirli değilse (dirty=false) direkt kapanıyor, grid refresh yok; kirliyse modal açılıyor, Kaydet form submit + refresh, Kaydetme sadece kapat, İptal kal.
  - Plain modal fallback korunuyor; Bootstrap Modal varsa onu kullanıyor.
- Expected: 2’li confirm tamamen kalkacak; Kapat → 3 buton modal. Yeni Kayıt butonu form kapatılınca enable, açıkken disable (JS tarafı değişmedi).
- Observed: (TEST BEKLİYOR)

## 2026-01-26 15:10 (TR)
- Change: `Record.cshtml` (Library + WebHost)
  - close modal butonları için sabit genişlik ve açık mavi arka plan eklendi.
  - handleSave içine HTML5 validation kontrolü eklendi: `checkValidity()/reportValidity()` + ilk hatalı alana focus; validation fail ise submit iptal.
- Expected: Kaydet tıklandığında zorunlu alan eksikse modal kapanmadan uyarı/outline görülür ve ilk hatalı alana focus olur. 3 buton görünümü tutarlı.
- Observed: (TEST BEKLİYOR)

## 2026-01-26 15:18 (TR)
- Change: `Record.cshtml` (Library + WebHost)
  - `Form.Code` ve `Form.Name` inputlarına `required` eklendi.
  - Modal footer butonları için renkler tekilleştirildi (#667eea arka plan, beyaz yazı).
- Expected:
  - Kaydet tıklandığında Code/Name boş ise HTML5 validation uyarısı + focus; kayıt gönderilmez.
  - Modal butonları aynı renk/size görünür.
- Observed: (TEST BEKLİYOR)

## 2026-01-26 15:30 (TR)
- Change: `wwwroot/js/archix.grid.component.js` (Library + WebHost)
  - `setNewRecordButtonState` artık `disabled` + `aria-disabled` + `pointer-events:none` + `opacity-50` ile görsel/fonksiyonel disable yapıyor.
- Expected: Yeni Kayıt butonuna basıldığında gözle görülür şekilde disable olur, accordion kapanınca enable.
- Observed: (TEST BEKLİYOR)

## 2026-01-26 15:35 (TR)
- Change: `wwwroot/js/archix.grid.component.js` (Library + WebHost)
  - `setNewRecordButtonState` içine debug log eklendi (`[Grid] setNewRecordButtonState {gridId, disabled, exists}`).
- Expected: Console’da disable/enable çağrıları görülecek; sorun devam ederse gridId/elements takip edilebilir.
- Observed: (TEST BEKLİYOR)

## 2026-01-26 15:45 (TR)
- Observed: Yeni Kayıt butonu hâlâ disable olmuyor (diğer düzeltmeler OK).
- Next (runtime veri gerekli):
  1) Console: Yeni Kayıt’a tıklayınca `[Grid] setNewRecordButtonState` log’u geliyor mu? İçeriği nedir?
  2) Elements: Yeni Kayıt butonunun id’si ve `disabled`/`aria-disabled` durumunu tıklama sonrası kontrol et (render edilmiş DOM’da).
  3) Eğer log yoksa hangi JS dosyası yükleniyor? (Sources’ta `archix.grid.component.js` içeriğinde `pointer-events` satırını ara.)

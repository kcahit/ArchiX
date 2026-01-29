## 2026-01-27 00:00 (TR local)
- Change: src/ArchiX.Library.Web/Pages/Definitions/Parameters/Record.cshtml -> bool switch inline hizalama (Evet | toggle | Hayır) için form-switch yapılandırması güncellendi.
- Expected: Bool tipi seçilince toggle varsayılan Bootstrap görünümüyle aynı satırda; diğer tiplerde tamamen gizli.
- Observed: olmadı (kullanıcı: bool seçince sınır/toggle yerleşimi hatalı, başlangıçta da yanlış görünüyor). F12 verisi henüz yok.

## 2026-01-27 00:20 (TR local)
- Change: src/ArchiX.Library.Web/Pages/Definitions/Parameters/Record.cshtml -> Bool görünümü Bootstrap form-switch içine, Evet | toggle | Hayır sıralı ve border’lı form-control içine alındı; bool harici tiplerde text input, bool tipinde switch gösteriliyor.
- Expected: Bool seçilince border korunmuş, tek satırda Evet | toggle | Hayır; diğer tiplerde switch görünmez.
- Observed: kullanıcı hâlâ “olmuyor” diyor (görselde switch border’ın dışında vs.). F12 veri yok.

## 2026-01-27 00:35 (TR local)
- Change: src/ArchiX.Library.Web/Pages/Definitions/Parameters/Record.cshtml -> Bool tipi için switch yerine form-control içinde yatay Evet/Hayır radyo çifti; bool dışındaki tiplerde text input varsayılan.
- Expected: Açılışta text input görünsün; Bool seçilince form-control sınır korunarak aynı satırda Evet/Hayır radyo butonları gösterilsin.
- Observed: henüz geri bildirim yok (kullanıcıdan bekleniyor).

## 2026-01-27 01:05 (TR local)
- Change: src/ArchiX.Library.Web/Pages/Definitions/Parameters/Record.cshtml -> Label asp-for bağlandı; JS başlangıcında resetValueUI ile text input görünür, bool radyolar gizli/sıfırlı, sonra applyValueInput çalışıyor.
- Expected: Açılışta her tipte Değer alanı text olarak görünür; bool seçilirse radyolar gösterilir, başka tipe geçince radyolar gizlenir, text geri gelir.
- Observed: bekleniyor.

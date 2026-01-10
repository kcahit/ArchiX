Revize No: 1  
Tarih/Saat: 2026-01-10 21:00  
Github Issue: #32  

BÖLÜM 1: Parametreler ve Standart Davranış Sözleşmesi  
1.1 Analiz:  
1.1.1. Bu çalışma, Grid/Combo/Form ekranlarını tek bir standart akışa bağlamak için iki ana parametre ve sabit davranış kuralları tanımlar.  
1.1.2. Parametre-1: `IsFormOpenEnabled` (Default=0) — GridListe/GridTable gibi liste ekranlarında “form ekranı açılacak mı?” davranışını belirler.  
1.1.3. Parametre-2: `HasRecordOperations` (Default=0) — Form/FormRecord ekranında kayıt işlemleri (Değiştir/Sil vb.) yetkisini ve buton görünürlüğünü belirler.  
1.1.4. Karar-1: `IsFormOpenEnabled=0` iken form açılmayacak ve bunu açan “Değiştir” butonu görünmeyecek (kullanıcı aksiyonu oluşmayacak).  
1.1.5. Karar-2: Kapat uyarısında “İptal” seçeneği olacak (Evet/Hayır/İptal).  
1.1.6. Karar-3: Yeni kayıtta “Sil” görünmeyecek.  
1.1.7. Karar-4: FormRecord’ta buton adı “Değiştir” olacak (Kaydet yerine).  
1.1.8. Karar-5: Sil/Değiştir sonrası grid’e dönüşte filtre/sayfa korunacak.  
1.1.9. Karar-6: “Tekil dataset” kuralı “tek kayıt” anlamına gelir (tek record zorunluluğu).  

1.2 Teknik Tasarım:  
1.2.1. `IsFormOpenEnabled` ve `HasRecordOperations` parametreleri her çağrıda opsiyoneldir; set edilmezse default `0` kabul edilir.  
1.2.2. `IsFormOpenEnabled=0` senaryosunda “Değiştir” butonu render edilmez; böylece kullanıcı form açma aksiyonuna erişemez.  
1.2.3. `IsFormOpenEnabled=1` senaryosunda “Değiştir” tıklaması FormRecord ekranını açar.  
1.2.4. `HasRecordOperations=0` senaryosunda kayıt aksiyonları (Değiştir/Sil) görünmez veya pasif olur; aynı kural backend tarafında da enforce edilir.  
1.2.5. “Tek kayıt” dataset kuralı: form açılışında veri kümesi 1 kayıttan fazla ise kullanıcı tarafında anlamlı hata/uyarı ile işlem durdurulur.  
1.2.6. Grid state koruma: Form ekranına gidip dönüşte filtre/sayfa bilgisinin korunması zorunludur. Bu amaçla bir “geri dönüş bağlamı” taşınır (filtreler, sayfa numarası, varsa seçili satır anahtarı).  
1.2.7. Kapat uyarısı üç seçeneklidir:  
1.2.7.1. Evet: değişiklikleri kaydet → formu kapat → listeye dön (state korunur).  
1.2.7.2. Hayır: kaydetmeden kapat → listeye dön (state korunur).  
1.2.7.3. İptal: formda kal (kapatma iptal).  
1.2.8. Yeni kayıt modunda “Sil” render edilmez.  

1.3 Unit Test:  
1.3.1. Parametre default testleri: Parametre verilmediğinde `IsFormOpenEnabled=0` ve `HasRecordOperations=0` kabul ediliyor mu?  
1.3.2. “Tek kayıt” kuralı: 0 kayıt, 1 kayıt, 2+ kayıt senaryolarında beklenen sonuçlar.  
1.3.3. “Kapat” akışı: değişiklik yok/var senaryolarında Evet-Hayır-İptal seçeneklerinin etkisi.  
1.3.4. Yeni kayıt akışı: yeni modunda “Sil” görünmeme doğrulaması.  
1.3.5. State koruma: form dönüşünde filtre/sayfa bilgisinin korunması.  

1.4 Kullanıcı Testi:  
1.4.1. GridListe/Combo’da `IsFormOpenEnabled=0` iken “Değiştir” butonunun görünmediğini doğrula.  
1.4.2. GridTable’da “Yeni” tıkla → form açılır; “Sil” görünmemeli.  
1.4.3. Form üzerinde değişiklik yap → Kapat → uyarı gelir → Evet/Hayır/İptal üç seçeneği doğru çalışır mı?  
1.4.4. Değiştir/Sil sonrası listeye dönüşte filtre/sayfa bilgisinin korunduğunu doğrula.  
1.4.5. Tek kayıt zorunluluğu: tek kayıt dışı veri setinde sistemin kullanıcıyı doğru uyarıp engellediğini doğrula.  

BÖLÜM 2: GridListe / GridTable / Kombine Akışları  
2.1 Analiz:  
2.1.1. GridListe bir template olarak kullanılacak; satır içi “Yeni” kaldırılacak, “Yeni” aksiyonu GridTable üst alanda standardize edilecek.  
2.1.2. GridTable “Tanımlar” menüsündeki liste ekranlarının standart giriş noktası olacak.  
2.1.3. Kombine sayfası `IsFormOpenEnabled=0` ile çalışacak; ayrı form açma davranışı olmayacak.  

2.2 Teknik Tasarım:  
2.2.1. GridListe template’inde satır içindeki “Yeni” butonu görünmez olacak.  
2.2.2. GridListe template’inde “Değiştir” butonu sadece `IsFormOpenEnabled=1` iken render edilecek.  
2.2.3. GridTable üst aksiyon alanında “Yeni” butonu bulunacak.  
2.2.4. GridTable → “Yeni” tıklanınca form akışı `HasRecordOperations=1` ile başlatılacak.  
2.2.5. Grid state yönetimi: GridTable, filtre/sayfa bilgisini form ekranına iletecek; form kapanınca aynı bağlamla grid tekrar oluşturulacak.  

2.3 Unit Test:  
2.3.1. GridListe: `IsFormOpenEnabled=0` iken “Değiştir” render edilmez.  
2.3.2. GridListe: `IsFormOpenEnabled=1` iken “Değiştir” render edilir ve doğru akış başlar.  
2.3.3. GridTable: “Yeni” render edilir.  
2.3.4. GridTable: “Yeni” → form açılış parametreleri doğru set edilir.  
2.3.5. Grid state: filtre/sayfa korunur.  

2.4 Kullanıcı Testi:  
2.4.1. GridListe ekranında satır içi “Yeni” yok mu kontrol et.  
2.4.2. GridTable’da üstte “Yeni” var mı kontrol et.  
2.4.3. Filtre uygula + farklı sayfaya geç → form aç/kapat → aynı filtre/sayfa geri geldi mi kontrol et.  

BÖLÜM 3: Form.cshtml Dinamik Form ve Kapat/Kayıt Akışları  
3.1 Analiz:  
3.1.1. Form ekranı gelen tek kayda göre dinamik alan üretir.  
3.1.2. Veri yoksa; grid kolonları varsa onlara göre alan üretir; grid kolonları da yoksa kullanıcıya “veri yok” mesajı gösterir.  
3.1.3. Kayıt işlemleri parametresi ile butonlar yönetilir; yeni kayıtta Sil görünmez kuralı geçerlidir.  
3.1.4. Kapat davranışı kritik: değişiklik varsa kaydetme sorusu ve İptal seçeneği olmalıdır.  

3.2 Teknik Tasarım:  
3.2.1. Dataselector görünmez olacak; form açılışında tek kayıt zorunluluğu kontrol edilir.  
3.2.2. Alan üretim önceliği:  
3.2.2.1. Tek kayıt dataset → alanlar veri tiplerine göre oluşturulur.  
3.2.2.2. Dataset boş → grid kolonlarına göre oluşturulur.  
3.2.2.3. İkisi de yok → boş sayfa + “veri yok” notu.  
3.2.3. `HasRecordOperations=1` iken “Değiştir” aktif; `0` iken görünmez/pasif olur.  
3.2.4. Yeni kayıt modunda “Sil” render edilmez.  
3.2.5. Kapat uyarısı: Evet/Hayır/İptal.  
3.2.6. Değiştir tıklandığında kaydet yapılır ve formda kalınır.  
3.2.7. Sil tıklandığında onay alınır; evet ise silinir ve grid’e dönülür (state korunur).  

3.3 Unit Test:  
3.3.1. Tek kayıt doğrulaması (0/1/2+ kayıt).  
3.3.2. Field üretimi: dataset var/yok → doğru alan seti üretimi.  
3.3.3. `HasRecordOperations=0/1` → buton görünürlüğü/aktiflik.  
3.3.4. Yeni kayıt modunda Sil’in görünmemesi.  
3.3.5. Kapat uyarısı: Evet/Hayır/İptal akışı.  

3.4 Kullanıcı Testi:  
3.4.1. Tek kayıt geldiğinde alanlar doğru oluşuyor mu kontrol et.  
3.4.2. Veri yoksa “veri yok” notu doğru görünüyor mu kontrol et.  
3.4.3. Değişiklik yap → Kapat → İptal seç → formda kalıyor mu kontrol et.  
3.4.4. Değişiklik yap → Kapat → Evet seç → kaydedip çıkıyor mu kontrol et.  
3.4.5. Değişiklik yap → Kapat → Hayır seç → kaydetmeden çıkıyor mu kontrol et.  

BÖLÜM 4: FormRecord.cshtml (Grid Kaydı Odaklı Form)  
4.1 Analiz:  
4.1.1. FormRecord, grid satırından açılan tek kayıt formudur ve Form.cshtml template mantığını kullanır.  
4.1.2. Aktif satır verisi forma dolar; kayıt işlemleri yapılabilir; kaydetmeden çıkışta uyarı gelir.  
4.1.3. Buton adı “Değiştir” olacaktır.  
4.1.4. Değiştir/Sil sonrası grid’e dönüşte filtre/sayfa korunacaktır.  

4.2 Teknik Tasarım:  
4.2.1. FormRecord açılışında tek kayıt yüklenir ve form alanlarına bind edilir.  
4.2.2. “Değiştir” butonu kaydetme işlevini yürütür.  
4.2.3. “Sil” yalnız mevcut kayıtta görünür; yeni modda görünmez.  
4.2.4. Kapat uyarısı Evet/Hayır/İptal olmak zorundadır.  
4.2.5. Grid state (filtre/sayfa) geri dönüş bağlamı form boyunca taşınır ve dönüşte uygulanır.  

4.3 Unit Test:  
4.3.1. Tek kayıt açılışı ve binding.  
4.3.2. “Değiştir” → kaydet akışı.  
4.3.3. “Sil” → onay → sil → dönüş (state korunumu).  
4.3.4. Kapat uyarısı üç seçenek.  

4.4 Kullanıcı Testi:  
4.4.1. Grid’de filtre/sayfa uygula → FormRecord aç → kapat → aynı filtre/sayfa korunuyor mu kontrol et.  
4.4.2. Değiştir yap → beklenen kaydet davranışını doğrula.  
4.4.3. Sil yap → kayıt silinmiş mi ve state korunmuş mu kontrol et.  

BÖLÜM 5: Yan Menü (Application / Parameter / Dataset) Kapsam ve Bağlantılar  
5.1 Analiz:  
5.1.1. Side menu bölümleri (Application/Parameter/Dataset) veri seti ile çalışacak şekilde planlanmıştır; detayları sonra netleşecektir.  
5.1.2. Amaç, menüden seçilen öğenin hangi dataset ile GridTable açacağını belirleyen bir bağlayıcı akış oluşturmaktır.  

5.2 Teknik Tasarım:  
5.2.1. Application menüsü `Application.cs`, Parameter menüsü `Parameter.cs`, Dataset menüsü `ReportDataset.cs` üzerinden veri alacak şekilde kurgulanır.  
5.2.2. Menü item → dataset kimliği → GridTable açılışı zinciri kurulacak şekilde sözleşme belirlenir.  

5.3 Unit Test:  
5.3.1. Menü item üretimi ve dataset mapping (detaylar netleşince).  

5.4 Kullanıcı Testi:  
5.4.1. Menüden ilgili liste ekranına (GridTable) geçişlerin doğru çalıştığı manuel kontrol (detaylar netleşince).  

Yapılacak İşler:  
- İş Sıra No: 1  ==> (github Issue NO: #36)
  - BÖLÜM 1 / Satır: 1.2.1–1.2.8: Parametre sözleşmesi, default davranışlar, kapat uyarısı (Evet/Hayır/İptal), tek kayıt kuralı ve state koruma standartları.  
- İş Sıra No: 2  ==> (github Issue NO: #37)
  - BÖLÜM 2 / Satır: 2.2.1–2.2.5: GridListe’de satır içi “Yeni” kaldırma, `IsFormOpenEnabled` ile “Değiştir” render kontrolü, GridTable’da üst “Yeni” standardı ve state taşıma.  
	- İş Sıra No: 3  (github Issue NO: #38)
  - BÖLÜM 3 / Satır: 3.2.1–3.2.7: Form.cshtml tek kayıt zorunluluğu, dinamik alan üretimi, `HasRecordOperations` ile buton kontrolü, yeni kayıtta Sil gizleme, Kapat uyarısı 3 seçenek.  
- İş Sıra No: 4  ==> (github Issue NO: #39)
  - BÖLÜM 4 / Satır: 4.2.1–4.2.5: FormRecord.cshtml akışı (tek kayıt dolumu), “Değiştir” butonu, kaydetmeden çıkış uyarısı ve grid state korunumu.  
- İş Sıra No: 5  ==> (github Issue NO: #40)
  - BÖLÜM 5 / Satır: 5.2.1–5.2.2: Side menu → dataset → GridTable bağlama sözleşmesi (detaylar netleşince genişletme).  

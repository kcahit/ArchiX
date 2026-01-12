Revize No: 2  
Tarih/Saat: 2026-01-12  
Github Issue: #32  

# Dashboard Sol Menuden Sayfa Açma Kuralları (Revize 2)

Bu revize; Revize-1’deki Grid/Record standartlarını korurken, ekranları **Dataset Tool** mimarisi altında yeniden adlandırır, route/konum standardı getirir ve multi-instance (aynı sayfada çoklu tool) hedefini resmi sözleşmeye bağlar.

---

## 0) Terimler ve Eski → Yeni Eşleştirme Tablosu

### 0.1 Page (Tool/Host) eşleştirmesi
- Eski: `Raporlar/GridListe` → Yeni: `Pages/Tools/Dataset/DatasetGrid`
- Eski: `Raporlar/FormRecordDetail` → Yeni: `Pages/Tools/Dataset/DatasetRecord`

### 0.2 Component eşleştirmesi
- Eski: `GridTable` → Yeni: `Shared/Components/Dataset/DatasetGridTable`
- Eski: (Record ekranı UI) → Yeni: `Shared/Components/Dataset/DatasetRecordForm`
- Eski: `DatasetSelector` → Yeni: `Shared/Components/Dataset/DatasetSelector`

### 0.3 Route / URL standardı (hard contract)
- `DatasetGrid` route: `/Tools/Dataset/Grid`
- `DatasetRecord` route: `/Tools/Dataset/Record`

---

# BÖLÜM 1: Parametreler ve Standart Davranış Sözleşmesi

## 1.1 Analiz
1.1.1. Bu çalışma, dataset-driven Grid/Record ekranlarını tek bir standart akışa bağlamak için parametre ve davranış kurallarını tanımlar.
1.1.2. Parametre-1: `IsFormOpenEnabled` (Default=0) — Grid tarafında “record ekranı açılacak mı?” davranışını belirler.
1.1.3. Parametre-2: `HasRecordOperations` (Default=0) — Record ekranında kayıt işlemleri (Değiştir/Sil vb.) yetkisini ve buton görünürlüğünü belirler.
1.1.4. Karar-1: `IsFormOpenEnabled=0` iken record açılmayacak ve bunu açan “Değiştir” butonu görünmeyecek (kullanıcı aksiyonu oluşmayacak).
1.1.5. Karar-2: Kapat uyarısında “İptal” seçeneği olacak (Evet/Hayır/İptal).
1.1.6. Karar-3: Yeni kayıtta “Sil” görünmeyecek.
1.1.7. Karar-4: Record ekranında buton adı “Değiştir” olacak (Kaydet yerine).
1.1.8. Karar-5: Sil/Değiştir sonrası grid’e dönüşte filtre/sayfa korunacak.
1.1.9. Karar-6: “Tekil dataset” kuralı “tek kayıt” anlamına gelir (tek record zorunluluğu).

## 1.2 Teknik Tasarım
1.2.1. `IsFormOpenEnabled` ve `HasRecordOperations` parametreleri her çağrıda opsiyoneldir; set edilmezse default `0` kabul edilir.
1.2.2. `IsFormOpenEnabled=0` senaryosunda “Değiştir” butonu render edilmez; kullanıcı record açma aksiyonuna erişemez.
1.2.3. `IsFormOpenEnabled=1` senaryosunda “Değiştir” tıklaması `DatasetRecord` ekranını açar.
1.2.4. `HasRecordOperations=0` senaryosunda kayıt aksiyonları (Değiştir/Sil) görünmez veya pasif olur; aynı kural backend tarafında da enforce edilir.
1.2.5. “Tek kayıt” kuralı: record açılışında veri kümesi 1 kayıttan fazla ise işlem fail-closed durdurulur.
1.2.6. Grid state koruma: Record ekranına gidip dönüşte filtre/sayfa bilgisinin korunması zorunludur. Bu amaçla bir “geri dönüş bağlamı” (`ReturnContext`) taşınır.
1.2.7. Kapat uyarısı üç seçeneklidir:
1.2.7.1. Evet: değişiklikleri kaydet → formu kapat → listeye dön (state korunur).
1.2.7.2. Hayır: kaydetmeden kapat → listeye dön (state korunur).
1.2.7.3. İptal: formda kal (kapatma iptal).
1.2.8. Yeni kayıt modunda “Sil” render edilmez.

## 1.3 Unit Test
1.3.1. Parametre default testleri: Parametre verilmediğinde `IsFormOpenEnabled=0` ve `HasRecordOperations=0` kabul ediliyor mu?
1.3.2. “Tek kayıt” kuralı: 0 kayıt, 1 kayıt, 2+ kayıt senaryolarında beklenen sonuçlar.
1.3.3. “Kapat” akışı: değişiklik yok/var senaryolarında Evet-Hayır-İptal seçeneklerinin etkisi.
1.3.4. Yeni kayıt akışı: yeni modunda “Sil” görünmeme doğrulaması.
1.3.5. State koruma: record dönüşünde filtre/sayfa bilgisinin korunması.

## 1.4 Kullanıcı Testi
1.4.1. `DatasetGridTable` içinde `IsFormOpenEnabled=0` iken “Değiştir” butonunun görünmediğini doğrula.
1.4.2. Grid’de filtre uygula + farklı sayfaya geç → record aç/kapat → aynı filtre/sayfa geri geldi mi kontrol et.
1.4.3. Record üzerinde değişiklik yap → Kapat → uyarı gelir → Evet/Hayır/İptal üç seçeneği doğru çalışır mı?
1.4.4. Değiştir/Sil sonrası grid’e dönüşte filtre/sayfa bilgisinin korunduğunu doğrula.
1.4.5. Tek kayıt zorunluluğu: tek kayıt dışı veri setinde sistemin kullanıcıyı doğru uyarıp engellediğini doğrula.

---

# BÖLÜM 2: DatasetGrid / DatasetGridTable Akışları

## 2.1 Analiz
2.1.1. `DatasetGrid` liste ekranlarının standart giriş noktasıdır.
2.1.2. `DatasetGrid` toolbar’da dataset seçimi ile rapor çalıştırır ve grid’i doldurur.
2.1.3. `DatasetGrid` aynı sayfada birden fazla instance olarak render edilebilir.

## 2.2 Teknik Tasarım
2.2.1. `DatasetGrid` route: `/Tools/Dataset/Grid`.
2.2.2. Grid bileşeni: `DatasetGridTable`.
2.2.3. `DatasetGrid` ekranında `DatasetSelector` **görünür** olacak (dataset seçimi burada yapılır).
2.2.4. Grid’de “Değiştir” butonu sadece `IsFormOpenEnabled=1` iken render edilecek.
2.2.5. Grid → Record navigasyonu `DatasetRecord` route’una yapılacak.
2.2.6. Grid state yönetimi: Grid, filtre/sayfa bilgisini `ReturnContext` ile record ekranına iletecek; record kapanınca aynı bağlamla grid tekrar oluşturulacak.

## 2.3 Unit Test
2.3.1. `DatasetGrid` route’a GET ile erişim.
2.3.2. Dataset seçilmeden “Raporla”: boş/başlangıç davranışı.
2.3.3. Dataset seçilip “Raporla”: kolon/satırların render edilmesi.
2.3.4. `IsFormOpenEnabled=0` iken “Değiştir” render edilmemesi.

## 2.4 Kullanıcı Testi
2.4.1. `DatasetGrid` ekranında dataset seç → “Raporla” → grid doldu mu kontrol et.
2.4.2. Aynı sayfada iki farklı dataset ile iki grid instance çalışabiliyor mu kontrol et.
2.4.3. Filtre uygula + farklı sayfaya geç → record’a git → geri dön → state korunmuş mu kontrol et.

---

# BÖLÜM 3: DatasetRecord / DatasetRecordForm Akışları

## 3.1 Analiz
3.1.1. `DatasetRecord`, dataset + `RowId?` ile tek kayıt ekranıdır.
3.1.2. `RowId` yoksa (null/boş): yeni kayıt modudur.
3.1.3. `RowId` varsa: edit/view modudur.
3.1.4. Aynı sayfada aynı dataset için farklı `RowId` ile çoklu `DatasetRecord` instance açılabilir.
3.1.5. `DatasetRecord` içinde `DatasetSelector` **daima gizli** olacaktır.

## 3.2 Teknik Tasarım
3.2.1. `DatasetRecord` route: `/Tools/Dataset/Record`.
3.2.2. `DatasetRecord` parametre sözleşmesi (minimum):
- `ReportDatasetId` (zorunlu)
- `RowId` (opsiyonel)
- `ReturnContext` (opsiyonel)
- `HasRecordOperations` (opsiyonel; UI + backend enforce)
3.2.3. `Mode` parametresi kullanılmayacaktır. New/Edit ayrımı yalnızca `RowId` üzerinden yapılır.
3.2.4. Tek kayıt zorunluluğu: `RowId` ile yüklenen sonuç 1 satır değilse fail-closed.
3.2.5. Yeni kayıt modunda “Sil” görünmez.

## 3.3 Unit Test
3.3.1. Tek kayıt doğrulaması (0/1/2+ kayıt).
3.3.2. `HasRecordOperations=0/1` → buton görünürlüğü/aktiflik ve backend enforce.
3.3.3. Yeni kayıt modunda Sil’in görünmemesi.

## 3.4 Kullanıcı Testi
3.4.1. Grid’de filtre/sayfa uygula → record aç → kapat → aynı filtre/sayfa korunuyor mu kontrol et.
3.4.2. Değiştir/Sil sonrası beklenen kaydet/sil davranışını doğrula ve grid’e dönüşte state korunmuş mu kontrol et.

---

# BÖLÜM 4: ReturnContext (Grid State) Sözleşmesi

## 4.1 Tanım
4.1.1. `ReturnContext`, Grid ekranının UI state’ini taşımak için kullanılan base64 JSON değeridir.
4.1.2. Taşınacak minimum alanlar: `Search`, `Page`, `ItemsPerPage`.

## 4.2 Akış
4.2.1. Grid → Record geçişinde `returnContext` query string ile taşınır.
4.2.2. Record → Grid dönüş linki `returnContext`’i geri taşır ve grid restore eder.

## 4.3 Unit Test
4.3.1. Encode/decode: base64 JSON çözülüyor mu?
4.3.2. Decode bozuksa fail-safe ignore ediyor mu?

## 4.4 Kullanıcı Testi
4.4.1. Arama+sayfa+itemsPerPage kombinasyonu uygula → record’a git → geri dön → aynı durum geliyor mu?

---

# BÖLÜM 5: Multi-instance ve DOM İzolasyon Sözleşmesi

## 5.1 Analiz
5.1.1. Aynı sayfada birden fazla `DatasetGridTable` ve birden fazla `DatasetRecordForm` çalıştırılabilir.
5.1.2. Bu nedenle DOM id çakışması ve JS state çakışması sistematik olarak engellenmelidir.

## 5.2 Teknik Tasarım (zorunlu)
5.2.1. Her instance benzersiz bir `InstanceId`/DOM prefix ile çalışır.
5.2.2. Çakışmaması gereken minimum alanlar:
- DOM id’leri (`*-searchInput`, `*-itemsPerPageSelect`, `*-pagination`, modal/offcanvas id’leri)
- JS state map key (instance bazlı)
- Inline onclick hedefleri (instance parametresi doğru geçmeli)
- `DatasetSelector` select/button id’leri (prefix’li)
5.2.3. Offcanvas/Modal id’leri global olamaz. Örn: `archix-row-editor` id’si instance bazlı türetilmelidir (`archix-row-editor-{InstanceId}`).

## 5.3 InstanceId naming standardı
5.3.1. Grid: `dsgrid-{ReportDatasetId}` (gerekirse `-n` suffix)
5.3.2. Record: `dsrec-{ReportDatasetId}-{RowId|new}`

## 5.4 Unit Test
5.4.1. Aynı sayfada iki grid instance: arama/paging birbirini etkilemiyor mu?
5.4.2. Aynı sayfada iki record instance: modal/offcanvas id çakışması oluşuyor mu?

## 5.5 Kullanıcı Testi
5.5.1. Aynı sayfada iki grid aç → birinde arama yap → diğer etkilenmemeli.
5.5.2. Aynı sayfada iki record aç → birini kapat → diğer kapanmamalı.

---

# BÖLÜM 6: Yan Menü (Application / Parameter / Dataset) Kapsam ve Bağlantılar

## 6.1 Analiz
6.1.1. Side menu bölümleri (Application/Parameter/Dataset) dataset ile çalışacak şekilde planlanmıştır; detayları sonra netleşecektir.
6.1.2. Amaç, menüden seçilen öğenin hangi dataset ile `DatasetGrid` açacağını belirleyen bir bağlayıcı akış oluşturmaktır.

## 6.2 Teknik Tasarım
6.2.1. Menü item → dataset kimliği → `/Tools/Dataset/Grid` zinciri kurulacak şekilde sözleşme belirlenir.

## 6.3 Unit Test
6.3.1. Menü item üretimi ve dataset mapping (detaylar netleşince).

## 6.4 Kullanıcı Testi
6.4.1. Menüden ilgili liste ekranına (`DatasetGrid`) geçişlerin doğru çalıştığı manuel kontrol (detaylar netleşince).

---

# Yapılacak İşler (Revize-2 sırası)

- İş Sıra No: 1  ==> (github Issue NO: ___)
  - Eski → yeni sayfa/component rename/move işlemleri + route güncellemesi.
  - `_Sidebar` linkleri güncelleme.
  - `DatasetSelector` RunEndpoint güncelleme.
  - Grid → Record yönlendirme güncelleme.
  - Record back link üretimi + state restore.

- İş Sıra No: 2  ==> (github Issue NO: ___)
  - `ReturnContext` standardının net uygulanması (encode/decode + restore).

- İş Sıra No: 3  ==> (github Issue NO: ___)
  - Multi-instance izolasyon işleri:
    - Offcanvas/Modal id’lerini InstanceId’li hale getirme
    - JS state map’lerini InstanceId’ye göre izole etme

- İş Sıra No: 4  ==> (github Issue NO: #36) ==> tamamlandı
  - Parametre sözleşmesi, default davranışlar, kapat uyarısı (Evet/Hayır/İptal), tek kayıt kuralı ve state koruma (önceki commit).

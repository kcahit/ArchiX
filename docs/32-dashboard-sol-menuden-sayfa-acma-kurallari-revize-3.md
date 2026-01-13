Revize No: 3  
Tarih/Saat: 2026-01-12  
Github Issue: #32  

# Dashboard Sol Menuden Sayfa Açma Kuralları (Revize 3)

Bu revize, `docs/#32 Dashboard Sol Menuden Sayfa Acma Kurallari.md` (Revize-1) dokümanının **numaralandırma ve test kapsamı standardını** koruyarak; thread’de mutabık kalınan **DatasetGrid/DatasetRecord + multi-instance + ReturnContext + route standardı** kararlarını **eksiksiz** şekilde monte eder.

---

## 0) Terimler / Map / Konum ve Route Standardı

### 0.1 Eski → Yeni isim eşleştirmesi
0.1.1. Page (Tool/Host)
- `GridListe` → `DatasetGridPage`
- `FormRecordDetail` (eski FormRecord) → `DatasetRecordPage`

0.1.2. Component (UI)
- `GridTable` → `DatasetGridViewComponent`
- (Record ekranı UI) → `DatasetRecordViewComponent`
- `DatasetSelector` → `DatasetSelectorViewComponent` (zaten bu isim repo’da var, değiştirme)

0.1.3. Dizin standardı
- Tool sayfaları: `Pages/Tools/Dataset`
- UI component’leri: `Shared/Components/Dataset`

0.1.4. Route / URL standardı (hard contract)
- `DatasetGridPage` route: `/Tools/Dataset/Grid`
- `DatasetRecordPage` route: `/Tools/Dataset/Record`

---

# BÖLÜM 1: Parametreler ve Standart Davranış Sözleşmesi

## 1.1 Analiz
1.1.1. Bu çalışma, dataset tabanlı Grid/Record ekranlarını tek bir standart akışa bağlamak için parametre ve sabit davranış kuralları tanımlar.
1.1.2. Parametre-1: `IsFormOpenEnabled` (Default=0) — Grid tarafında “record ekranı açılacak mı?” davranışını belirler.
1.1.3. Parametre-2: `HasRecordOperations` (Default=0) — `DatasetRecord` ekranında kayıt işlemleri (Değiştir/Sil vb.) yetkisini ve buton görünürlüğünü belirler.
1.1.4. Karar-1: `IsFormOpenEnabled=0` iken record açılmayacak ve bunu açan “Değiştir” butonu görünmeyecek (kullanıcı aksiyonu oluşmayacak).
1.1.5. Karar-2: Kapat uyarısında “İptal” seçeneği olacak (Evet/Hayır/İptal).
1.1.6. Karar-3: Yeni kayıtta “Sil” görünmeyecek.
1.1.7. Karar-4: `DatasetRecord`’ta buton adı “Değiştir” olacak (Kaydet yerine).
1.1.8. Karar-5: Sil/Değiştir sonrası grid’e dönüşte filtre/sayfa korunacak.
1.1.9. Karar-6: “Tekil dataset” kuralı “tek kayıt” anlamına gelir (tek record zorunluluğu).

## 1.2 Teknik Tasarım
1.2.1. `IsFormOpenEnabled` ve `HasRecordOperations` parametreleri her çağrıda opsiyoneldir; set edilmezse default `0` kabul edilir.
1.2.2. `IsFormOpenEnabled=0` senaryosunda “Değiştir” butonu render edilmez.
1.2.3. `IsFormOpenEnabled=1` senaryosunda “Değiştir” tıklaması `DatasetRecord` ekranını açar.
1.2.4. `HasRecordOperations=0` senaryosunda kayıt aksiyonları (Değiştir/Sil) görünmez/pasif olur; aynı kural backend tarafında da enforce edilir.
1.2.5. “Tek kayıt” dataset kuralı: Record açılışında veri kümesi 1 kayıttan fazla ise işlem kullanıcıya anlamlı hata ile durdurulur (fail-closed).
1.2.6. Grid state koruma: `DatasetRecord` ekranına gidip dönüşte filtre/sayfa bilgisi korunur. Bu amaçla `ReturnContext` taşınır.
1.2.7. Kapat uyarısı üç seçeneklidir:
1.2.7.1. Evet: değişiklikleri kaydet → record’u kapat → listeye dön (state korunur).
1.2.7.2. Hayır: kaydetmeden kapat → listeye dön (state korunur).
1.2.7.3. İptal: record’da kal (kapatma iptal).
1.2.8. Yeni kayıt modunda “Sil” render edilmez.

## 1.3 Unit Test
1.3.1. Parametre default testleri: Parametre verilmediğinde `IsFormOpenEnabled=0` ve `HasRecordOperations=0` kabul ediliyor mu?
1.3.2. “Tek kayıt” kuralı: 0 kayıt, 1 kayıt, 2+ kayıt senaryolarında beklenen sonuçlar.
1.3.3. “Kapat” akışı: değişiklik yok/var senaryolarında Evet-Hayır-İptal seçeneklerinin etkisi.
1.3.4. Yeni kayıt akışı: yeni modunda “Sil” görünmeme doğrulaması.
1.3.5. State koruma: record dönüşünde filtre/sayfa bilgisinin korunduğunun doğrulanması.

## 1.4 Kullanıcı Testi
1.4.1. `DatasetGridTable` içinde `IsFormOpenEnabled=0` iken “Değiştir” butonunun görünmediğini doğrula.
1.4.2. Grid’de filtre uygula + farklı sayfaya geç → record aç/kapat → aynı filtre/sayfa geri geldi mi kontrol et.
1.4.3. Record üzerinde değişiklik yap → Kapat → uyarı gelir → Evet/Hayır/İptal üç seçeneği doğru çalışır mı?
1.4.4. Değiştir/Sil sonrası grid’e dönüşte filtre/sayfa bilgisinin korunduğunu doğrula.
1.4.5. Tek kayıt zorunluluğu: tek kayıt dışı veri setinde sistemin kullanıcıyı doğru uyarıp engellediğini doğrula.

---

# BÖLÜM 2: DatasetGrid / DatasetGridTable Akışları

## 2.1 Analiz
2.1.1. `DatasetGrid` bir template olarak kullanılacak; toolbar’da dataset seçimi ile rapor çalıştırılıp grid doldurulacaktır.
2.1.2. `DatasetGrid` liste ekranlarının standart giriş noktasıdır.
2.1.3. `DatasetGrid` aynı sayfada birden fazla instance olarak render edilebilir.
2.1.4. `DatasetGrid` içinde `DatasetSelector` görünürdür; dataset seçimi burada yapılır.

## 2.2 Teknik Tasarım
2.2.1. `DatasetGrid` route: `/Tools/Dataset/Grid` (hard contract).
2.2.2. Grid UI component’i: `DatasetGridTable`.
2.2.3. Grid toolbar’da “Raporla” ile dataset çalıştırılır.
2.2.4. “Değiştir” aksiyonu sadece `IsFormOpenEnabled=1` iken render edilir.
2.2.5. Grid → Record navigasyonu `DatasetRecord` route’una yapılır.
2.2.6. Grid state yönetimi: Grid, filtre/sayfa bilgisini `ReturnContext` ile record ekranına iletir; record kapanınca aynı bağlamla grid restore edilir.

## 2.3 Unit Test
2.3.1. Grid’de dataset seçilmeden çalıştırma davranışı.
2.3.2. Grid’de dataset seçilip çalıştırıldığında kolon/satırların render edilmesi.
2.3.3. `IsFormOpenEnabled=0` iken “Değiştir” render edilmemesi.
2.3.4. `IsFormOpenEnabled=1` iken “Değiştir” aksiyonunun doğru route’a gittiğinin doğrulanması.

## 2.4 Kullanıcı Testi
2.4.1. `DatasetGrid` ekranında dataset seç → “Raporla” → grid doldu mu kontrol et.
2.4.2. Filtre uygula + paging yap → record’a git → geri dön → state korunuyor mu kontrol et.
2.4.3. Aynı sayfada iki farklı dataset ile iki grid instance çalıştır (multi-instance smoke).

---

# BÖLÜM 3: DatasetRecord / DatasetRecordForm Akışları

## 3.1 Analiz
3.1.1. `DatasetRecord`, dataset + `RowId?` ile tek kayıt ekranıdır.
3.1.2. `RowId` yoksa: yeni kayıt modudur.
3.1.3. `RowId` varsa: edit/view modudur.
3.1.4. `DatasetRecord` içinde `DatasetSelector` **daima gizli** olacaktır.

## 3.2 Teknik Tasarım
3.2.1. `DatasetRecord` route: `/Tools/Dataset/Record` (hard contract).
3.2.2. Query sözleşmesi (minimum):
3.2.2.1. `ReportDatasetId` (zorunlu)
3.2.2.2. `RowId` (opsiyonel)
3.2.2.3. `ReturnContext` (opsiyonel)
3.2.2.4. `HasRecordOperations` (opsiyonel; UI + backend enforce)
3.2.3. `Mode` parametresi kullanılmayacaktır. New/Edit ayrımı yalnızca `RowId` üzerinden yapılır.
3.2.4. Tek kayıt zorunluluğu: `RowId` ile load edilen sonuç 1 kayıt değilse fail-closed.
3.2.5. Yeni kayıt modunda “Sil” render edilmez.

## 3.3 Unit Test
3.3.1. Tek kayıt doğrulaması (0/1/2+ kayıt).
3.3.2. `HasRecordOperations=0/1` → buton görünürlüğü/aktiflik.
3.3.3. Backend enforce: `HasRecordOperations=0` iken update/delete reddi.
3.3.4. Yeni kayıt modunda “Sil” görünmeme doğrulaması.

## 3.4 Kullanıcı Testi
3.4.1. Grid’den kayıt aç → geri dön → state korunuyor mu kontrol et.
3.4.2. `HasRecordOperations=0` iken butonlar yok mu/pasif mi kontrol et.
3.4.3. Yeni modunda “Sil” yok mu kontrol et.

---

# BÖLÜM 4: ReturnContext Sözleşmesi (Grid State)

## 4.1 Analiz
4.1.1. `ReturnContext`, grid UI state’ini record ekranına taşır ve geri dönüşte restore sağlar.

## 4.2 Teknik Tasarım
4.2.1. `ReturnContext` base64 JSON’dur.
4.2.2. Minimum alanlar: `Search`, `Page`, `ItemsPerPage`.
4.2.3. Grid → Record geçişinde query string olarak taşınır.
4.2.4. Record → Grid dönüş linki `ReturnContext`’i geri taşır.

## 4.3 Unit Test
4.3.1. Encode/decode doğru çalışıyor mu?
4.3.2. Bozuk context geldiğinde fail-safe ignore ediyor mu?

## 4.4 Kullanıcı Testi
4.4.1. Arama + page + itemsPerPage set et → record’a git → geri dön → aynı durum geliyor mu?

---

# BÖLÜM 5: Multi-instance / DOM İzolasyon Sözleşmesi

## 5.1 Analiz
5.1.1. Aynı HTML sayfasında birden fazla tool instance render edilebilir.
5.1.2. Bu nedenle DOM id, modal/offcanvas id ve JS state çakışmaları engellenmelidir.

## 5.2 Teknik Tasarım
5.2.1. Her instance benzersiz bir `InstanceId`/DOM prefix ile çalışır.
5.2.2. Çakışmaması gereken minimum alanlar:
5.2.2.1. DOM id’leri (`*-searchInput`, `*-itemsPerPageSelect`, `*-pagination`, modal/offcanvas id’leri)
5.2.2.2. JS state map key (instance bazlı)
5.2.2.3. Inline onclick hedefleri
5.2.2.4. `DatasetSelector` select/button id’leri
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
6.1.1. Side menu bölümleri (Application/Parameter/Dataset) dataset ile çalışacak şekilde planlanmıştır.
6.1.2. Amaç, menüden seçilen öğenin hangi dataset ile `DatasetGrid` açacağını belirleyen standart zinciri kurmaktır.

## 6.2 Teknik Tasarım
6.2.1. Menü item → dataset kimliği → `/Tools/Dataset/Grid` zinciri kurulacaktır.
6.2.2. Menü linkleri hard contract route’lara göre üretilecektir.

## 6.3 Unit Test
6.3.1. Menü item üretimi ve dataset mapping (detaylar netleşince).

## 6.4 Kullanıcı Testi
6.4.1. Menüden ilgili liste ekranına geçişlerin doğru çalıştığı manuel kontrol (detaylar netleşince).

---

# Yapılacak İşler (Revize-3 sırası)

  - İş Sıra No: 1  ==> (github Issue NO: #43)
  - (Zorunlu) Rename/move + route hard contract geçişi.
  - `_Sidebar` linkleri güncelleme.
  - `DatasetSelector` RunEndpoint güncelleme.
  - Grid → Record yönlendirme güncelleme.
  - Record back link üretimi + state restore.

  - İş Sıra No: 2  ==> (github Issue NO: #44)
  - `ReturnContext` standardının net uygulanması (encode/decode + restore).

- İş Sıra No: 3  ==> (github Issue NO: #45)
  - Multi-instance izolasyon işleri:
    - Offcanvas/Modal id’lerini InstanceId’li hale getirme
    - JS state map’lerini InstanceId’ye göre izole etme

- İş Sıra No: 4  ==> (github Issue NO: #36) ==> tamamlandı
  - Parametre sözleşmesi, default davranışlar, kapat uyarısı (Evet/Hayır/İptal), tek kayıt kuralı ve state koruma (önceki commit).

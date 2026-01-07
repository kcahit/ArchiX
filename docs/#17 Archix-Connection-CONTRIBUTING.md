# #17 ArchiX – Connection / Scope / Dataset-Driven Kombine (CONTRIBUTING)

Tarih: 2026-01-05  
Kapsam: Bu doküman; aşağıdaki iki karar dokümanını **eksiksiz** birleştirerek “uygulamaya dönük, soru bırakmayan” tek bir teknik referans üretir:

- `docs/#17 architecture-decisions-multi-db-instance-scope-kombine.md`
- `docs/#16 architecture-decisions-dataset-driven-kombine-report.md`

Amaç: ArchiX tabanlı projelerde (ERP/CRM/WMS vb.) **connection yönetimi**, **güvenlik/policy**, **secret yönetimi**, `Application` kavramı ve **dataset-driven Kombine raporlama** mimarisini; her başlık altında (1) teknik tasarım sözleşmeleri ve (2) test senaryolarıyla birlikte standart hale getirmek.

> Öncelik sırası: **Güvenlik > Performans/Stabilite > Kullanıcı dostu uygulama**

---

## Dokümanı okuma kuralı
Her başlık altında iki alt bölüm vardır:
- `### Teknik Tasarım`: Uygulamada uygulanacak tasarım/kontrat.
- `### Test Senaryoları`: Bu kuralın doğrulanması için minimum test/checklist.

---

## 1) Temel Varsayımlar ve Hedef (Instance / Tenant / İzolasyon)

### Teknik Tasarım
- Müşteri bazında izolasyon, **ayrı instance (deployment)** ile sağlanır.
- Bir instance içerisinde tek müşteri bulunur (customer data isolation; tenant-discriminator yerine deployment sınırından gelir).

Neden:
- Güvenlik: müşteri verisi izolasyonunu en güçlü sınırdan (deployment/instance) almak.
- Operasyon: müşteri bazında yedekleme/restore, performans tuning, incident izolasyonu.
- Mimari sadeleşme: tenant switching, request başına tenant resolution, global filtreler vb. karmaşıklıkların azalması.

### Test Senaryoları
- Bir instance içerisinde “tenant switching / request başına tenant resolution” gerektiren bir akış bulunmamalıdır.
- Deployment sınırı dışında (aynı instance içinde) ikinci müşteri verisini ayıran bir mekanizma varsayılmamalıdır.

---

## 2) `Application` Kavramı (Ürün/Proje) ve Scope

### Teknik Tasarım
- `Application` = **ürün/proje** kavramıdır (ERP, CRM, WMS vb.).
- `Application` **müşteri/tenant değildir**.

`Application` içine connection konmaz:
- `Application` “ürün”ü temsil ettiği için connection detayları ürün kavramına ait değildir.
- Connection bilgisini `Application` içine koymak;
  - projelerin birden fazla DB’ye çıkması,
  - environment ayrımları,
  - secret rotasyonu,
  - farklı bağlantı profilleri
  gibi ihtiyaçlarda modeli kilitler.

Scope yaklaşımı:
- `Application` ile ilişkilendirilen ayarlar/policy’ler (parametreler) **ürün scope**’udur.
- Müşteri ayrımı instance/deployment ile sağlandığı için “tenant scope” bu seviyede taşınmaz.

### Test Senaryoları
- `Application` entity/modelinde connection string veya bağlantı profilini temsil eden alanlar bulunmamalıdır.
- Connection tanımları `Application` yerine parametre/konfig yapısından çözülmelidir.

---

## 3-1) Connection Yönetimi (Çoklu DB + Alias/Key)

### Teknik Tasarım
- Tek bir instance **birden fazla DB** ile konuşabilir (operasyonel/raporlama/entegrasyon).
- Bu nedenle bağlantı “tek connection string” değil; **bağlantı seti / bağlantı profili** gibi ele alınır.

DB dataset’lerde connection seçimi:
- DB kaynaklı dataset’ler (`sp`, `view`, `table`) **mutlaka** önceden tanımlı bir **`ConnectionName`** (alias) ile çalışır.
- Dataset kayıtları:
  - yeni connection/şifre içermez,
  - “connection denemesi ekleme” içermez.
- `ConnectionName` çözümü sistemin genel parametre/konfig yapısındaki tanım üzerinden yapılır.
- Server bilgisi ayrıca dataset’te tutulmaz; connection içinden türetilir.

### Test Senaryoları
- DB dataset kaydı oluşturma/düzenleme akışında host/user/password gibi alanlar bulunmamalıdır.
- DB dataset çalıştırılırken mutlaka bir alias çözümleme adımı olmalıdır.

---
### 3-2) UI Notu – Dataset Selector Bileşeni (Unutulmasın)

### Teknik Tasarım
- Dataset selector UI (dropdown + `Raporla` butonu) **ayrı bir component** olarak ele alınacaktır (grid’e bağımlı olmayacak).
- Grid sayfalarında (GridListe, Kombine) hedef yerleşim:
  - `DatasetSelector` → `Toolbar` → (`Pivot`) → `Grid`
	- `Pivot` GridListe sayfasında yoktur. kombine da ise vardır.

- dataset selector, `GridToolbar` içinde **gömülü** şekilde kullanılacak (layout tutarlılığı için).
- Dataset selector’ın gösterimi parametrik olmalıdır:
  - Kullanıcı dataset seçecekse görünür.
  - Sayfa dataset/veri ile açılıyorsa gizli.
	- Şİmdilik raporla butonu tıklanınca "raporlar yüklenecek mesajı olacak

### Test Senaryoları
- Dataset selector component’i grid olmadan bağımsız bir sayfada render edilebilir olmalıdır.
- GridListe sayfasında sıralama `DatasetSelector` → `GridToolbar` → `GridTable` şeklinde doğrulanmalıdır.
- Kombine sayfasında sıralama `DatasetSelector` → `GridToolbar` → `Pivot` → `GridTable` şeklinde doğrulanmalıdır.

### Revize (2026-01-07 13:15)

Bu revize; bu thread’de netleşen `3-2 Dataset Selector` gereksinimlerini, repo gerçekleri ile çelişmeyecek şekilde “uygulamaya dönük / soru bırakmayan” standart olarak günceller.

#### Teknik Tasarım (Revize 3-2)

**3-2.0) Öncelik**
- Karar sırası: **Güvenlik > Performans/Stabilite > Kullanıcı dostu uygulama**.
- Belirsiz/uygunsuz durumda işlem yapılmaz (**fail-closed**).

**3-2.1) Tam bağımsız component (GridToolbar bağımlılığı kaldırılacak)**
- `DatasetSelector`, `GridToolbar` / `GridTable` olmadan da tek başına herhangi bir Razor Page’de render edilebilmelidir.
- `GridToolbar` içinde *embed* kullanım (layout tutarlılığı) devam edebilir; ancak `DatasetSelector`’ın **model/sözleşme bağımlılığı** `GridToolbar`’a bağlı olmayacaktır.
- Repo tespiti (mevcut durum):
  - `DatasetSelectorViewComponent` şu an `Invoke(GridToolbarViewModel model)` imzasıyla `GridToolbarViewModel`’a bağlıdır.
  - `DatasetSelector` view’ları `@model GridToolbarViewModel` kullanır.
  - Bu durum “grid’den bağımsız component” şartını tam karşılamaz.
- Hedef sözleşme:
  - Minimal bir model tanımlanır: `DatasetSelectorViewModel`.
  - `DatasetSelectorViewComponent` birincil olarak `Invoke(DatasetSelectorViewModel model)` ile çalışır.
  - `GridToolbar` içinde kullanımda `GridToolbarViewModel` → `DatasetSelectorViewModel` map edilerek component beslenir.
- `DatasetSelectorViewModel` için minimum alanlar (zorunlu):
  - `Id`: DOM prefix (aynı sayfada birden fazla selector için).
  - `IsVisible`: selector görünür/gizli.
  - `Options`: dropdown seçenekleri (**sadece Approved dataset**).
  - `SelectedReportDatasetId`: seçili dataset id (nullable).
  - `RunEndpoint`: “Raporla” butonunun POST edeceği endpoint.
  - `RunText`, `Placeholder`: UI metinleri.
  - `Mode`: kullanım modu
    - `GridMultiRow` (grid raporları için)
    - `FormSingleRow` (form ekranları için)

**3-2.2) Yerleşim kuralı (grid sayfaları)**
- Grid sayfalarında hedef yerleşim:
  - `DatasetSelector` → `Toolbar` → (`Pivot`) → `Grid`
- `Pivot`:
  - GridListe’de yoktur
  - Kombine’de vardır
- `DatasetSelector`, toolbar içinde embed edilebilir; ancak bağımsız render (toolbar olmadan) mümkün olmalıdır.

**3-2.3) Görünürlük / mod kuralı**
- `DatasetSelector`’ın görünmesi parametrik olmalıdır:
  - Kullanıcı dataset seçecekse: **görünür** (`IsVisible = true`)
  - Sayfa dataset/veri ile açılıyorsa: **gizli** (`IsVisible = false`)
- Örnek akışlar:
  - Kullanıcı menüden tıklar → sayfa seçim bekler → selector görünür.
  - Kullanıcı “Stok Kartı Aç” gibi bir butonla veya listeden “detaya git” akışıyla `id` göndererek sayfayı açar → sayfa kaydı yükler → selector gizlidir.

**3-2.4) Form ekranları: tek kayıt zorunluluğu (Security-first / fail-closed)**
- Form ekranları **tek kayıt (single row)** ile çalışır.
- `Mode = FormSingleRow` iken dataset sonucu:
  - `RowCount == 1` → form alanları doldurulur.
  - `RowCount == 0` veya `RowCount > 1` → işlem reddedilir (**fail-closed**).
- Bu kontrol yalnızca UI tarafında bırakılamaz; **server-side zorunlu** uygulanır.
- Gerekçe: çoklu kayıt dönen dataset form ekranına uygun değildir; yanlış kayıt açılmasına/veri karışmasına yol açabilir.

**3-2.5) Run endpoint / hook-up zorunluluğu (repo ile uyum)**
- Repo tespiti: mevcut `DatasetSelector` JS, `RunEndpoint`’e **POST** atıyor; ancak örnek sayfalarda (`GridListe`, `Kombine`) karşılayan handler yoksa çağrı çalışmaz.
- Bu nedenle şu iki yaklaşımdan biri seçilmelidir (en az biri zorunlu):
  1) “Raporla” butonu gerçekten çalışacaksa: ilgili Razor Page(ler)de `OnPost...` handler yazılacak ve endpoint bağlanacak,
  2) “Şimdilik sadece mesaj gösterilecek” ise: POST çağrısı kaldırılacak / no-op yapılacak.
- Doküman hedefi “dataset-driven” çalışma olduğu için tercih: (1) handler + endpoint.

**3-2.6) Kombine sayfası uyumu**
- `Kombine.cshtml` bu sözleşmeye göre çalışır hale getirilmelidir:
  - selector model besleme (options/visibility/endpoint)
  - yerleşim: `DatasetSelector` → `GridToolbar` → `Pivot` → `GridTable` sırası korunacak

#### Test Senaryoları (Revize)

- `DatasetSelector` component’i `GridToolbar` olmadan bağımsız bir Razor Page’de render edilebilmelidir.
- `GridToolbar` içinde embed çalışırken `DatasetSelector` doğrudan `GridToolbarViewModel` ile beslenmemelidir (map ile `DatasetSelectorViewModel` verilmelidir).
- Detay sayfası `id` ile açıldığında `DatasetSelector` gizli olmalıdır.
- Form dataset çalıştırma sonucunda:
  - `RowCount == 1` → form doldurulur,
  - `RowCount == 0` veya `RowCount > 1` → fail-closed (işlem reddedilir).
- `RunEndpoint` için POST çağrısını karşılayan Razor Page handler yoksa test başarısız sayılır (endpoint/hook-up zorunlu).
- `Kombine` sayfasında sıralama `DatasetSelector` → `GridToolbar` → `Pivot` → `GridTable` doğrulanmalıdır.

#### 3-2 İçin Kalan İşler (Sadece Bu Başlık) – Uygulama Sırası (13:50 2026-01-07) .TAMAMLANDI (2025-01-07 15:40)

1) `DatasetSelectorViewModel` ekle (minimal, bağımsız model). TAMAMLANDI.
   - Amaç: `DatasetSelector` artık `GridToolbarViewModel`’a bağlı kalmadan render edilebilsin.

2) `DatasetSelectorViewComponent`’i `Invoke(DatasetSelectorViewModel model)` olacak şekilde değiştir.  TAMAMLANDI.
   - Mevcut repo: `Invoke(GridToolbarViewModel model)` (bağımlılık burada).

3) `Templates/Modern/.../DatasetSelector/Default.cshtml` view’ını `@model DatasetSelectorViewModel` yapacak şekilde güncelle.  TAMAMLANDI.
   - Mevcut repo: view `@model GridToolbarViewModel`.

4) `GridToolbar` embed kullanımını koru: `GridToolbarViewModel` → `DatasetSelectorViewModel` map ederek component çağrısını güncelle.  TAMAMLANDI.
   - Amaç: toolbar içinde gömülü kullanım devam etsin ama model bağımlılığı kalksın.

5) `RunEndpoint` / hook-up’ı gerçekten çalışır hale getir. TAMAMLANDI.
   - Repo tespiti: JS `RunReportEndpoint`’e POST atıyor; sayfalarda handler yoksa bu çağrı boşa gider.
   - En az bir Razor Page’de (GridListe veya Kombine) `OnPost...` handler + endpoint wiring yapılacak.

6) `Kombine` sayfasını yeni sözleşmeye göre uyarlayıp doğrula (selector + endpoint + yerleşim). TAMAMLANDI.
   - Yerleşim: `DatasetSelector` → `GridToolbar` → `Pivot` → `GridTable`.

7) Form sayfaları için dataset-run akışında server-side `RowCount == 1` zorunluluğunu uygula (fail-closed).
   - Not: Bu madde “3-2 selector UI”nin değil, selector’ın **FormSingleRow modu**nun güvenlik sözleşmesidir; bu yüzden en sona konur ama tamamlanmadan 3-2 “bitti” sayılmaz.

3-2 İŞLERİN HEPSİ TAMAMLANDI (2025-01-07 15:40)

----
## 4) Secret Yönetimi (Güvenlik Önceliği) ==> TAMAMLANDI (2026-01-07 16:40)

### Teknik Tasarım
Prensip:
- “Connection bilgisini yönetmek” ile “secret saklamak” ayrıdır.
- Veritabanında (özellikle `_ArchiX`) saklanan yapı, tercihen connection’ın kendisi değil; connection’a giden **referans/anahtar (alias)** olmalıdır.

Kesin karar:
- Tenant DB’lerine ait **User/Password** gibi secret’lar `_ArchiX` içinde **asla** saklanmaz.
- `_ArchiX` tarafında yalnızca secret’a giden bir referans saklanır.

Asgari secret referansı standardı:
- Minimum desteklenecek format: `PasswordRef = ENV:<NAME>`
- Şifre uygulama çalıştığı ortamda (host/tenant) environment variable olarak sağlanır.

> Not: İleride güvenli secret store (örn. Key Vault vb.) ihtiyacı olursa yeni ref formatları eklenebilir; ancak bu dokümandaki minimum standart `ENV:`’dir.

### Test Senaryoları
- `_ArchiX` tarafındaki parametre/tablolar içinde password alanı veya password değeri bulunmamalıdır.
- `PasswordRef` çözümlenemiyorsa bağlantı reddedilmelidir (fail-closed).

---
4) Razor Pages UI Entegrasyonu (Kombine / GridListe) – Revize (2026-01-07 16:03 )
Öncelik sırası: Güvenlik > Performans/Stabilite > Kullanıcı dostu uygulama
Belirsiz durumda işlem yapılmaz (fail-closed).
Kapsam ve Amaç
Bu işin hedefi:
•	Razor Pages tarafında sayfaların oluşması, DatasetSelector entegrasyonu, endpoint wiring ve minimum test kapsamı.
•	Şimdilik fake data ile ilerlemek (UI iskeleti ve akışlar tamamlanacak).
Bu işin kapsamı dışı:
•	Dataset dropdown seçeneklerinin DB’den “ApprovedOnly” olarak çekilmesi.
•	Grid kolon/satırlarının dataset sonucuna göre dinamik oluşması.
•	Pivot’un dataset sonucundan beslenmesi / pivot-grid senkronizasyonu.

Teknik Tasarım
4.1) DatasetSelector component sözleşmesi (UI contract)
•	DatasetSelector tam bağımsız bir component’tir; GridToolbar/GridTable gibi bileşenlere model bağımlılığı olmayacaktır.
•	DatasetSelector, hem:
•	herhangi bir Razor Page’de tek başına render edilebilir,
•	hem de GridToolbar içinde embed edilerek kullanılabilir.
•	DatasetSelector yalnızca DatasetSelectorViewModel ile beslenir:
•	Id: DOM prefix (zorunlu)
•	IsVisible
•	Options (şimdilik fake data olabilir)
•	SelectedReportDatasetId (nullable)
•	RunEndpoint (örn. "/Raporlar/Kombine?handler=Run")
•	RunText, Placeholder
•	Mode: GridMultiRow / FormSingleRow
4.2) Yerleşim (layout) sözleşmesi
•	Grid tabanlı rapor sayfalarında hedef sıralama:
•	DatasetSelector → GridToolbar → (Pivot) → GridTable
•	GridListe sayfasında Pivot yoktur.
•	Kombine sayfasında Pivot vardır.
•	DatasetSelector embed edilebilir; ancak bağımsız component olması zorunludur.
4.3) Run endpoint / hook-up zorunluluğu (fail-closed)
•	DatasetSelector “Raporla” butonu tıklanınca mutlaka RunEndpoint’e POST eder.
•	RunEndpoint server tarafında karşılanmıyorsa bu sözleşme ihlalidir.
•	Endpoint standardı:
•	Kombine: POST /Raporlar/Kombine?handler=Run&reportDatasetId=<id>
•	GridListe: POST /Raporlar?handler=Run&reportDatasetId=<id>
•	Server handler minimum davranış:
•	başarı: 200 OK
•	hata/exception/uygunsuz istek: 400 BadRequest (fail-closed)
4.4) “Raporla” butonu enable/disable davranışı (UI contract)
•	Dropdown başlangıçta null gelir.
•	“Raporla” butonu:
•	seçim yokken disabled
•	seçim yapılınca Enabled
•	başarılı run sonrası aynı seçim için disabled
•	seçim değişince tekrar Enabled
•	Bu davranış client-side uygulanır; güvenlik için server-side fail-closed her zaman geçerlidir.
4.5) Bu işte “fake data” kuralı (kapsam sınırı)
•	Dropdown options şimdilik fake olabilir.
•	Grid/Pivot şimdilik fake olabilir.
•	Ancak RunEndpoint’ler gerçek handler olarak çalışır olmalı ve testlerle doğrulanmalıdır.
4.6) Pivot + Detaylı Liste (gelecek uyumluluğu)
•	Nihai hedefte pivot ve detaylı liste aynı dataset run sonucundan beslenecektir.
•	Bu işte pivotun “UI olarak var olması” yeterlidir; dataset sonucu ile besleme sonraki işlere bırakılır.
---
Test Senaryoları (minimum / repo uyumlu)
1.	Run endpoint mevcut ve çalışır olmalı
•	GridListe:
•	executor success → 200 OK
•	exception → 400 BadRequest
•	Kombine:
•	executor success → 200 OK
•	exception → 400 BadRequest
2.	FormSingleRow fail-closed
•	RowCount == 1 → 200 OK + model doldurulur
•	RowCount == 0 veya RowCount > 1 → 400 BadRequest
3.	DatasetSelector bağımsız render
•	Sadece DatasetSelectorViewModel ile render edilebilir olmalı (grid bileşenlerine bağımlılık olmamalı).
4.	GridToolbar embed mapping
•	GridToolbarViewModel → DatasetSelectorViewModel map edilerek invoke yapılmalıdır.
---
İş Listesi (GitHub Issue Planı) – İş #4 (uygulama sırası 2025-01-07 16:03)

Aşağıdaki işler sıralıdır. Bir iş bitmeden sonraki işe geçilmez. Her adım derlenebilir ve testleri çalışır olmalıdır.
4.0) Kapsam netleştirici kural-1 (DB ApprovedOptions bu işte yok)
•	Bu işte Options DB’den çekilmeyecek.
•	Options listesi fake/in-memory olabilir.
•	DB’den ApprovedOnly listeleme kuralı korunur; ancak bu işin teslim kriteri değildir (sonraki işe taşınır).
4.1) Kapsam netleştirici kural-2 (Kombine Pivot sample data ile kalabilir)
•	Kombine pivot alanı bu işte sample/fake data ile çalışabilir.
•	Pivot’un dataset run sonucundan beslenmesi ve pivot-grid senkronizasyonu bu işin kapsamı dışıdır.
4.2) Kombine ve GridListe sayfalarında DatasetSelector entegrasyonunu standardize et
•	RunEndpoint doğru handler’a bağlı olmalı (?handler=Run).
•	Id değerleri sayfa bazında benzersiz ve tutarlı olmalı (örn. gridListe, kombineGrid).
4.3) Run endpoint sözleşmesini netleştir ve fail-closed uygula
•	reportDatasetId yok/boş/uygunsuz ise BadRequest (fail-closed).
•	Exception catch: BadRequest.
4.4) DatasetSelector UI behavior sözleşmesini repo standardı haline getir
•	initial disabled, change enabled, success sonrası disabled, seçim değişince enabled.
•	Davranış component içinde standardize olmalı; sayfa bazlı dağınık JS üretilmemeli.
4.5) GridToolbar embed kullanımını koru, fakat bağımsızlığı bozma
•	GridToolbar içinde invoke edilirken DatasetSelectorViewModel ile çağır.
•	DatasetSelector hiçbir şekilde GridToolbarViewModel bağımlılığı almamalı.
4.6) Minimum test paketini tamamla
•	GridListe run endpoint testleri
•	Kombine run endpoint testleri
•	FormSingleRow fail-closed testleri
•	(gerekirse) component bağımsızlığına yönelik düşük seviye test(ler)
4.7) Kapsam sınırı doğrulaması (bilinçli fake data kabulü)
•	Dropdown options fake olabilir.
•	Grid/Pivot fake olabilir.
•	Ancak RunEndpoint’ler gerçek handler olarak çalışır olmalı (wiring + test zorunlu).

4 İŞLERİN HEPSİ TAMAMLANDI (2025-01-07 16:40)

---

## 5) Connection Security Policy (Zorunlu)

### Teknik Tasarım
- Tüm DB bağlantıları için merkezi bir **connection policy** yaklaşımı esastır.

Policy zorunlulukları (kontrol listesi):
- Şifreli iletişim zorunluluğu (Encrypt)
- Güvenilir olmayan sertifika kabulünün engellenmesi veya kontrollü olması (TrustServerCertificate)
- Integrated Security kullanımının kontrollü olması
- Hedef sunucu / IP aralığı whitelist

Audit:
- Bağlantı denemeleri/audit kayıtları tutulur.
- Audit kayıtlarında ham connection string **maskelenmiş** olarak saklanır.

### Test Senaryoları
- Policy ihlali durumunda (örn. Encrypt kapalıysa) bağlantı engellenmelidir veya policy mode gereği uyarı verilmelidir.
- Audit kayıtlarında açık password veya tam connection string bulunmamalıdır (mask zorunlu).

Revize (2026-01-07 17:00)
Bu revize, “5) Connection Security Policy” bölümündeki sözleşmeyi multi-db (application birden fazla DB) gerçeğiyle uyumlu olacak şekilde daha net hale getirir ve “masking” şartını garanti (no-leak) seviyesinde tanımlar.
Teknik Notlar (Net Sözleşme)
5.0) Kapsam (bu bölüm neyi kapsar?)
•	Bu bölümde “tüm DB bağlantıları” ifadesi şu anlama gelir:
Application runtime sırasında (dataset executor / raporlama / entegrasyon vb.) uygulamanın bağlandığı tüm DB alias’ları ve tüm connection string üretimleri.
•	Bu ifade “tek connection string” değil, çoklu DB / çoklu alias gerçeğini kapsar. (örn. operasyonel DB, raporlama DB, entegrasyon DB)
•	Bu bölümün amacı; hangi DB olursa olsun aynı güvenlik sözleşmesinin merkezi uygulanmasıdır.
5.1) Merkezi policy zorunluluğu (fail-closed)
•	Uygulama runtime’da oluşturulan her DB bağlantısı, bağlantı kurulmadan önce mutlaka merkezi ConnectionPolicy değerlendirmesinden geçmelidir.
•	Belirsiz/uygunsuz durumda bağlantı kurulmaz (fail-closed).
(örn. Encrypt yoksa, whitelist boşsa, kural ihlali varsa)
5.2) Policy kontrol listesi (değişmedi, netleştirildi) Aşağıdaki kontroller merkezi policy’de zorunludur:
•	Encrypt zorunlu
•	TrustServerCertificate kontrollü / yasak (policy’ye göre)
•	Integrated Security kontrollü (policy’ye göre)
•	Hedef sunucu / IP aralığı whitelist (host veya CIDR)
5.3) Whitelist boşsa davranış (security-first)
•	Whitelist boş ise sistem “varsayılan izin ver” yaklaşımıyla davranmaz.
•	Policy Mode davranışı geçerlidir:
•	Enforce → bağlantı engellenir (Blocked)
•	Warn → uyarı üretilir (Warn) (ama güvenlik gereksinimine göre ileride Enforce’a taşınacaktır)
5.4) Audit masking (no-leak garantisi)
•	Audit kayıtlarında ham connection string tam haliyle saklanamaz.
•	Masking yalnızca “karakter kırpma” mantığıyla bırakılmayacak; anahtar bazlı (key-based) maskeleme zorunludur:
•	Password / Pwd gibi secret alanlarının değeri audit’e asla açık yazılamaz.
•	Masking sonucu, şifre veya secret içeriğini hiçbir koşulda geri üretilebilir şekilde içermemelidir.
Test Notları (Netleştirme)
•	Policy ihlali halinde (Encrypt=False, TrustServerCertificate=True yasakken, whitelist dışı hedef vb.) sonuç:
•	Enforce → Blocked
•	Warn → Warn
•	Audit kaydında:
•	açık password bulunmamalı,
•	connection string’in ham hali birebir bulunmamalı,
•	en azından secret alanlar kesin maskelenmiş olmalı.

#### Yapılacak İşler (İş #5 – Uygulama Sırası). tamamlandı (2026-01-07 17:05)

1) Masking sözleşmesini kodda garanti hale getir (no-leak)
   - Audit’e yazılan connection string masking’i “karakter kırpma” yaklaşımından çıkarılacak.
   - `Password` / `Pwd` gibi alanlar **anahtar bazlı** yakalanıp kesin maskelenecek.
   - Hedef: Audit kaydında açık password ve ham connection string bulunmayacak.

2) Policy değerlendirmesi + audit çağrı noktaları (runtime DB bağlantıları) doğrulaması
   - Application runtime’da açılan tüm DB bağlantılarının policy’den geçtiği doğrulanacak.
   - Multi-db (çok alias) senaryoda her alias için aynı policy uygulanmalı.

3) Test ekle (regresyon)
   - Audit masking testleri:
     - `Password=` içeren farklı connection string formatlarında (ör. `Password=...`, `Pwd=...`) sızıntı olmamalı.
     - Masked string içinde secret değeri asla yer almamalı.
   - Policy testleri (varsa genişlet):
     - Enforce modda whitelist boş → Blocked
     - Encrypt yok → Blocked

4) Build temizliği
   - Build sonrası Warning/Message kalmayacak şekilde son kontrol yapılacak.
   
  
işlerin hepsi tamlandı (2026-01-07 17:05).

---

## 6) Connection Tanımının Nerede Duracağı (Bootstrap + Parametre) ==>  tamamlandı (2026-01-07 17:30).


### Teknik Tasarım
Bootstrap:
- Tenant/host, sistem altyapısını kullanabilmek için önce `ArchiX.Library` konfig DB’si olan `_ArchiX`’e bağlanabilmelidir.
- Bootstrap bağlantı host tarafında konfigürasyon ile sağlanır (örn. `ConnectionStrings:ArchiXDb`).

Tenant uygulama DB connection’ları:
- Tenant’ın kendi DB’leri (örn. `Archix.Arsanmak`) host projesi tarafından kullanılacak olsa da connection tanımı `_ArchiX` içindedir.

Parametre standardı (tek satır / çok alias):
- `_ArchiX.Parameters` içinde **tek satır**
  - `ApplicationId = 1` (Global zorunlu application)
  - `Group = "ConnectionStrings"`
  - `Key = "ConnectionStrings"`
  - `Value` = JSON (tek value içinde çok alias)

JSON formatı:
- Connection string içinde `;` bulunduğu için delimiter tabanlı formatlar kırılgan kabul edilir.
- Bu nedenle `Value` JSON olmalıdır.

Not:
- Her alias bir connection profile objesidir.
- `Auth` temel olarak `SqlLogin` varsayılır.
- `PasswordRef` sadece `SqlLogin` için vardır.

### Test Senaryoları
- `_ArchiX` ayağa kalkmadan tenant DB connection’ları çözümlenemez (bootstrap bağımlılığı doğrulanmalı).
- Parametre çözümlemesi: doğru `Group`/`Key` ile tek satırdan tüm alias’lar okunabilmelidir.

Revize (2026-01-07 17:25)
Bu revize, ## 6) Connection Tanımının Nerede Duracağı (Bootstrap + Parametre) bölümünü repo gerçekleriyle ve güvenlik sözleşmesiyle uyumlu hale getirir. Özellikle “tenant DB bağlantısı” için Windows/Integrated Security kullanımının kabul edilmediği (login ile bağlantı) netleştirilir.
Teknik Notlar (Net Sözleşme)
6.0) Bootstrap bağlantısı (değişmedi)
•	Host/tenant instance, _ArchiX DB’sine ayağa kalkmak için host config üzerinden bağlanır:
•	ConnectionStrings:ArchiXDb
•	_ArchiX ayağa kalkmadan tenant DB alias’ları çözümlenemez (bootstrap bağımlılığı).
6.1) Tenant uygulama DB bağlantıları (SqlLogin zorunluluğu)
•	Tenant uygulama DB bağlantıları Parameters içindeki Group="ConnectionStrings", Key="ConnectionStrings" satırındaki JSON’dan çözülür.
•	Tenant DB bağlantılarında Windows/IntegratedSecurity kullanılmaz.
•	Gerekçe: uygulama host’u tenant ortamında Windows domain/host yetkisi varsayamaz.
•	Bu nedenle tenant DB’ler için varsayılan ve kabul edilen auth:
•	Auth = "SqlLogin"
•	SqlLogin için zorunlu alanlar:
•	User (zorunlu)
•	PasswordRef (zorunlu; minimum format ENV:<NAME>; çözümlenmezse fail-closed)
6.2) JSON standardı (tek satır / çok alias)
•	Parameters içinde tek satır:
•	ApplicationId = 1
•	Group = "ConnectionStrings"
•	Key = "ConnectionStrings"
•	Value = JSON (alias -> profile)
•	Her alias tek bir “connection profile” objesidir.
•	JSON zorunludur; delimiter tabanlı formatlar kabul edilmez.
6.3) Repo uyum notu (örnek seed düzeltme gereksinimi)
•	Repo’daki ConnectionStringsStartup demo seed’i, Auth alanında IntegratedSecurity örneği içeriyorsa bu standartla uyumsuz kabul edilir.
•	Demo seed örneği dahi olsa, tenant DB tarafında SqlLogin örneği verilmelidir.
Yapılacak İşler (İş #6 – Uygulama Sırası)
1.	ConnectionStringsStartup demo seed’i SqlLogin olacak şekilde güncelle
•	Auth = "SqlLogin"
•	User alanı eklenecek (örn. sa veya archix)
•	PasswordRef alanı eklenecek (örn. ENV:ARCHIX_DB_DEMO_PASSWORD)
•	Not: gerçek password kesinlikle yazılmayacak.
2.	Test / kontrol checklist (minimum)
•	Auth = "SqlLogin" iken PasswordRef çözümlenemezse fail-closed çalıştığı doğrulanmalı (mevcut testler ile).

İş #6 tamamlandı (2026-01-07 17:30).


---

## 7) Teknoloji ve Çoklu Provider Yaklaşımı

### Teknik Tasarım
- Default hedef: SQL Server + EF Core.
- Ancak bu “tek hedef” değildir; tenant ne kullanırsa (örn. Oracle kısmen mevcut), ihtiyaç olursa diğerleri de eklenir.
- Connection ve dataset yürütme katmanı, farklı DB provider’larına genişlemeye uygun tasarlanmalıdır.

### Test Senaryoları
- Provider bağımlı parçalar soyutlanmış olmalıdır (en azından tasarım seviyesinde).
- En az bir provider ile (SQL Server) uçtan uca dataset çalıştırma doğrulanmalıdır.

Revize (2026-01-07 17:40)
Bu revize, ## 7) Teknoloji ve Çoklu Provider Yaklaşımı bölümünü repo gerçekleriyle uyumlu hale getirir. Dokümanda geçen “Oracle kısmen mevcut” ifadesi korunur; ancak mevcut repo’da Oracle desteğinin aktif/çalışır olmadığı, yalnızca bir iskelet (placeholder) bulunduğu netleştirilir.
Teknik Notlar (Net Sözleşme)
7.0) Şu an desteklenen “aktif” provider
•	Şu an runtime (tenant DB bağlantıları + dataset executor) tarafında aktif ve desteklenen tek provider: SqlServer.
•	Repo gerçekleri (mevcut durum):
•	ConnectionStringBuilderService yalnızca Provider = "SqlServer" kabul eder; diğer provider’larda NotSupportedException fırlatır.
•	Provisioning tarafında ArchiXDatabase provider seçimi şu an yalnızca SqlServer provisioner’ını çalıştırır.
7.1) Oracle “kısmen mevcut” ifadesinin anlamı (netleştirme)
•	Oracle tarafında repo’da bir provisioner dosyası bulunabilir (OracleArchiXDbProvisioner).
•	Ancak bu dosya:
•	aktif provider listesine bağlı değildir,
•	provider seçimi tarafından kullanılmıyordur,
•	implementasyonu TODO’dur.
•	Bu nedenle “Oracle kısmen mevcut” = “repo’da iskelet/placeholder var” anlamındadır; çalışır özellik değildir.
7.2) Multi-provider hedefi (tasarım prensibi – ileri uyumluluk)
•	Tasarım hedefi: Connection profile modeli, executor katmanı ve provisioning akışı; ileride farklı DB provider’ları eklenebilecek şekilde parçalı (genişletilebilir) kalmalıdır.
•	Ancak yeni provider eklemek “cfg ile açılır” seviyesinde değildir; kod + test + entegrasyon işleri gerektirir.
Yapılacak İşler (İş #7 – Uygulama Sırası)
1.	Dokümana göre “aktif provider” netliği korunsun
•	Şu an tek aktif provider: SqlServer.
2.	Oracle’ı “aktif” yapmak istenirse (ayrı iş kalemi)
•	ArchiXDatabase provider alias + GetProvisioner() içine Oracle ekleme
•	ConnectionStringBuilderService içinde provider’e göre builder oluşturma (SqlServer dışı destek)
•	Dataset executor / DB erişim katmanında provider’e göre connection/command üretimi
•	Minimum test paketi (provider seçimi + basic connection build + executor smoke)

 iş #7 tamamlandı. sistem hazır herhangi bir şey yapılmadı. (2026-01-07 17:35).

---

## 8) Dataset-Driven Kombine (Tanım Tabanlı Rapor)

### Teknik Tasarım
- **Dataset tablosu `ArchiX.Library` / `_ArchiX` içinde olacak.**

Dataset kayıtları:
- Dataset tanımları DB’de tutulur ve `BaseEntity`’den inherit eder.
- Dropdown’da listelenecek dataset’ler yalnızca **onaylı** olanlardır.
  - Onay kriteri: `StatusId == BaseEntity.ApprovedStatusId`

Dataset temel alanları:
- `ConnectionName` (DB kaynaklı)
- `FileName`
- `DisplayName`
- `Type`:
  - DB: `sp`, `view`, `table`
  - File: `json`, `ndjson`, `csv`, `txt`, `xml`, `xls`, `xlsx`

Source family:
- Kısa vadede: DB ve File.
- Uzun vadede: API/Queue gibi aileler gelebilir.

### Test Senaryoları
- Approved olmayan dataset dropdown’da görünmemelidir.
- `Type` değerine göre DB/File ayrımı doğru yapılmalıdır.

---

## 9) Kombine Rapor Sayfası (Dinamik Çalışma Modeli)

### Teknik Tasarım
Amaç:
- `Kombine` sayfası, kullanıcı seçimine göre farklı kaynaklardan veri çekip aynı grid altyapısında gösterir.

Seçim ve kaynak ayrımı:
- Seçilen seçenek bir “rapor/dataset tanımı”dır.
- Tanım içinde kaynak türü açıkça belirtilir.
  - Yalnız view adı/json adı yeterli kabul edilmez.

Beklenen davranış:
- doğru connection alias seçilir,
- policy kuralları uygulanır,
- uygun veri kaynağı işlenir,
- grid’e uygun kolon/row üretilir.

### Test Senaryoları
- “Kaynak türü belirsiz” tanımlar reddedilmeli veya çalıştırılamamalıdır.
- Policy uygulanmadan DB kaynağı çalışmamalıdır.

---

## 10) File Dataset – Dosya Konumu ve Güvenlik

### Teknik Tasarım
- File dataset path:
  - Root: parametre tablosu
  - Alt dizin(ler): dataset kaydı
  - FileName: dataset kaydı
- Amaç: path traversal riskini azaltmak.

Parametre şeması:
- File dataset’lerde parametre tanımı/şeması dataset kaydının içinde bulunur.
- Parametre ekranı bu şemaya göre dinamik oluşur.

### Test Senaryoları
- Root dışına çıkma (.. / absolute path) engellenmelidir.
- Parametre ekranı şemaya göre oluşmalı ve şema olmayan durumda güvenli şekilde hata vermelidir.

---

## 11) SP Dataset Parametre Davranışı

### Teknik Tasarım
- SP dataset’lerde kullanıcıdan input parametresi alınmaz.
- SP output parametreleri filtre ekranı olarak gösterilmez.
- SP’lerde limit garantisi olmayabileceği için streaming sırasında limit korumaları zorunludur.

### Test Senaryoları
- SP dataset UI’sında input parametre alanı olmamalıdır.
- Büyük SP çıktısı limitlere göre kesilmelidir.

---

## 12) UI Davranışları (Kombine + GridListe Toolbar)

### Teknik Tasarım
- Toolbar’a dataset seçici eklenir.
- Dropdown başlangıçta `null` gelir.
- Listede sadece `Approved` dataset’ler bulunur.
- Dropdown’da `DisplayName` gösterilir.

“Raporla” butonu:
- Dropdown `null` değilse ve seçim değiştiyse enabled.
- Başarılı rapor çalıştıktan sonra disabled.
- Dataset seçimi değişince yeniden enabled.

### Test Senaryoları
- İlk açılışta rapor butonu disabled olmalıdır.
- Dataset değişince enabled olmalıdır.
- Başarılı çalıştırmadan sonra disabled olmalıdır.

---

## 13) Pivot Analiz ve Detaylı Liste (Tek Dataset, Tek Gerçek Veri)

### Teknik Tasarım
- Pivot Analiz ve Detaylı Liste aynı dataset çıktısından beslenir.
- Sahte data yoktur; veri dataset çıktısıdır.
- Grid filtreleri ile pivot senkronu ilk sürümde zorunlu değildir.

### Test Senaryoları
- Pivot ve detaylı liste aynı run-id/aynı sonuç setinden üretilebilmelidir.

---

## 14) Kolonlar ve Formatlama

### Teknik Tasarım
- Dataset’den gelen kolonlar dinamik alınır.
- Kolon yapısı/isimleri dataset tasarımcısının sorumluluğundadır.
- Kolon başlıkları ve formatlar sistemde tanımlı standartlara göre uygulanır.
- Gelecekte: kolon seçme ve kullanıcı bazında kolon tercihleri saklama gelebilir.

### Test Senaryoları
- Farklı kolon setleriyle (değişken schema) grid render edebilmelidir.
- Tarih/para/sayı formatları beklenen standartlara göre görünmelidir.

---

## 15) Limitler ve Koruma Politikaları (Performans + Stabilite)

### Teknik Tasarım
Birincil kontrol:
- `MaxCells = rows * cols`
- `allowedRows = floor(MaxCells / colCount)`

Hard limit:
- `HardMaxRows`, `HardMaxCols` üst sınırları zorunludur.

Metin taşması:
- `MaxCellChars` limiti kabul edilmiştir.
- `MaxTotalChars` / `MaxBytes` secondary guard olabilir.

Ölçüm maliyeti:
- Pre-check + streaming sırasında ucuz kontroller.
- Pahalı ölçümlerden kaçın.

Precedence:
- Limitler sistem parametrelerinde ve dataset üzerinde tanımlanabilir.
- Boş ise parametre tablosu geçerlidir.
- İkisi doluysa daha sıkı olan (min) kazanır.

### Test Senaryoları
- MaxCells aşıldığında satır sayısı otomatik düşmelidir.
- HardMaxRows/Cols kesin uygulanmalıdır.
- MaxCellChars aşıldığında hücre kırpılmalıdır (veya güvenli davranış).
- Dataset limiti ile parametre limiti çakışınca daha sıkı olan seçilmelidir.

---

## 16) Tutarlılık Kuralları (Özet)

### Teknik Tasarım
1. Instance = müşteri izolasyonu. Tenant switching scope’a taşınmaz.
2. `Application` = ürün/proje. Connection detayı bu entity’ye eklenmez.
3. Connection yönetimi çoklu DB’yi destekler. Bağlantı “tek string” değil “profil”dür.
4. Connection policy zorunludur. Encrypt/whitelist vb. kurallar merkezi uygulanır; audit maskeli tutulur.
5. `Kombine` tanım (dataset) tabanlıdır. Kaynak türü açıkça belirtilir; seçime göre dinamik yükleme yapılır.
6. Dataset tablosu `ArchiX.Library` / `_ArchiX` içindedir.
7. Secret’lar `_ArchiX` içinde tutulmaz; minimum `ENV:` referansı ile yönetilir.

### Test Senaryoları
- Bu maddelerin her biri, ilgili başlıklardaki test checklist’leri ile kapsanmış olmalıdır.

---

## 17) Örnek DB düşünce modeli (Not)

### Teknik Tasarım
Örnek DB yapısı:

- `ArchiX.Library` → `_ArchiX` (library/config DB)
- `ERP` → örnek:
  - `Archix.ERP.Master` (ana veriler)
  - `Archix.ERP.Arsanmak2024` (müşteri/yıl bazlı operasyon verisi)
  - `Archix.ERP.Arsanmak2025`
  - `Archix.ERP.Arsanmak2025.Test`

### Test Senaryoları
- Bu bölüm örnek niteliğindedir; zorunlu test içermez.

---

## Son Not
- Bu doküman “tek gerçek” olarak değerlendirilmelidir.
- Karar değişirse: değişiklik nedeni, tarih ve etkilenen alanlar mutlaka not edilmelidir.

---

## İş Listesi (GitHub Issue Planı)

Aşağıdaki işler **sıralıdır**. Bir iş bitmeden (merge/commit edilmeden) bir sonraki işe başlanmamalıdır. Amaç: her işin tek başına çalışır/derlenir ve test edilebilir olması.

### 1) Bootstrap + Connection Alias Çözümleme Altyapısı
**Kapsadığı bölümler:** 3, 4, 5, 6, 7

- `_ArchiX` bağlantısının (bootstrap) host tarafında standartlaştırılması
- `_ArchiX.Parameters` içindeki `Group="ConnectionStrings"`, `Key="ConnectionStrings"` satırından JSON okuma
- `PasswordRef = ENV:<NAME>` çözümleme (fail-closed)
- Provider-agnostic bağlantı profil modeli (en az SQL Server destekli)
- ConnectionPolicy zorunlu uygulanacak şekilde çağrı noktalarının belirlenmesi
- Audit maskleme kuralının uygulanacağı sözleşmenin netleştirilmesi

---
✅ Tamamlandı: 2026-01-05  (saat: 13:00 )
Kod Dosyaları
•	ConnectionProfile.cs (eklendi)
•	IConnectionProfileProvider.cs (eklendi)
•	ISecretResolver.cs (eklendi)
•	ArchixParameterConnectionProfileProvider.cs (eklendi)
•	EnvSecretResolver.cs (eklendi)
•	ConnectionStringBuilderService.cs (eklendi)
•	ConnectionsServiceCollectionExtensions.cs (eklendi)
•	ConnectionStringsStartup.cs (eklendi)
•	Program.cs (değişti)
Test Dosyaları
•	EnvSecretResolverTests.cs (eklendi)
•	ConnectionStringBuilderServiceTests.cs (eklendi)
•	ConnectionPolicyAuditorTests.cs (eklendi)
•	ConnectionStringsStartupTests.cs (eklendi)
Notlar:
•	Saat kısmını sen doldur (örn. 17:45).
•	Program.cs gerçekten “değişti” (DI’a AddArchiXConnections() eklendi).
---

### 2) Dataset Tablosu ve Dataset Tanımı (DB/File) + Approved Filtre
**Kapsadığı bölümler:** 8, 3, 2

- Dataset tablosunun `_ArchiX` içinde konumlandırılması
- Dataset `Type` değerleri (DB: sp/view/table; File: json/ndjson/csv/txt/xml/xls/xlsx)
- `StatusId == Approved` filtre kuralı
- `ConnectionName`, `FileName`, `DisplayName` alanlarının standardı

- Tamamlandı : 2026-01-06  (saat: 15:05 )
- 
### 3) Dataset Executor (DB + File) ve Limit/Guard Mekanizması
**Kapsadığı bölümler:** 9, 10, 11, 15, 5

- DB executor: `sp/view/table` çalıştırma (input param yok; output param yok)
- File executor: root + subpath + filename; traversal koruması
- Pre-check + streaming sırasında limitler:
  - `MaxCells`, `HardMaxRows`, `HardMaxCols`, `MaxCellChars`
  - Global vs dataset limit precedence (min kazanır)
- Policy + audit executordan önce uygulanır

### 4) Razor Pages UI Entegrasyonu (Kombine / GridListe)
**Kapsadığı bölümler:** 12, 13, 14, 9

- Toolbar’a dataset seçici
- “Raporla” butonu enable/disable davranışı
- Tek dataset çıktısından hem Pivot hem Detaylı Liste üretimi
- Dinamik kolon/formatlama kuralları

### 5) Test Paketi ve Regresyon Checklist
**Kapsadığı bölümler:** 1–17 (özellikle 4, 5, 8, 10, 11, 15)

- Security:
  - `_ArchiX` içinde secret yok
  - `PasswordRef` çözümlenemeyince fail-closed
  - Audit maskeli
  - Policy ihlali yakalanıyor
- Functional:
  - Approved filtre
  - SP parametre alınmıyor
  - File traversal engeli
- Performance/Stability:
  - Limitler (MaxCells/HardMax/MaxCellChars)

> Not: İş #5, önceki işler tamamlandıktan sonra güvenle genişletilir.

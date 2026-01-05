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

## 3) Connection Yönetimi (Çoklu DB + Alias/Key)

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

## 4) Secret Yönetimi (Güvenlik Önceliği)

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

---

## 6) Connection Tanımının Nerede Duracağı (Bootstrap + Parametre)

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

---

## 7) Teknoloji ve Çoklu Provider Yaklaşımı

### Teknik Tasarım
- Default hedef: SQL Server + EF Core.
- Ancak bu “tek hedef” değildir; tenant ne kullanırsa (örn. Oracle kısmen mevcut), ihtiyaç olursa diğerleri de eklenir.
- Connection ve dataset yürütme katmanı, farklı DB provider’larına genişlemeye uygun tasarlanmalıdır.

### Test Senaryoları
- Provider bağımlı parçalar soyutlanmış olmalıdır (en azından tasarım seviyesinde).
- En az bir provider ile (SQL Server) uçtan uca dataset çalıştırma doğrulanmalıdır.

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

### 2) Dataset Tablosu ve Dataset Tanımı (DB/File) + Approved Filtre
**Kapsadığı bölümler:** 8, 3, 2

- Dataset tablosunun `_ArchiX` içinde konumlandırılması
- Dataset `Type` değerleri (DB: sp/view/table; File: json/ndjson/csv/txt/xml/xls/xlsx)
- `StatusId == Approved` filtre kuralı
- `ConnectionName`, `FileName`, `DisplayName` alanlarının standardı

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

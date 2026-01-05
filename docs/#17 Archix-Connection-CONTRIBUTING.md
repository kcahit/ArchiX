# #17 ArchiX – Connection / Scope / Dataset-Driven Kombine (CONTRIBUTING)

Tarih: 2026-01-05  
Kapsam: Bu doküman; aşağıdaki iki karar dokümanını **eksiksiz** birleştirerek “uygulamaya dönük, soru bırakmayan” tek bir teknik referans üretir:

- `docs/#17 architecture-decisions-multi-db-instance-scope-kombine.md`
- `docs/#16 architecture-decisions-dataset-driven-kombine-report.md`

Amaç: ArchiX tabanlı projelerde (ERP/CRM/WMS vb.) **connection yönetimi**, **güvenlik/policy**, **secret yönetimi**, `Application` kavramı ve **dataset-driven Kombine raporlama** mimarisini; tasarım ilkeleri + veri modeli + UI davranışları + limit/koruma politikalarıyla birlikte standart hale getirmek.

> Öncelik sırası: **Güvenlik > Performans/Stabilite > Kullanıcı dostu uygulama**

---

## 1) Temel Varsayımlar ve Hedef (Instance / Tenant / İzolasyon)

### 1.1 Hedeflenen işletim modeli
- Müşteri bazında izolasyon, **ayrı instance (deployment)** ile sağlanır.
- Bir instance içerisinde tek müşteri bulunur (customer data isolation; tenant-discriminator yerine deployment sınırından gelir).

### 1.2 Neden bu model?
- Güvenlik: müşteri verisi izolasyonunu en güçlü sınırdan (deployment/instance) almak.
- Operasyon: müşteri bazlı yedekleme/restore, performans tuning, incident izolasyonu.
- Mimari sadeleşme: tenant switching, request başına tenant resolution, global filtreler vb. karmaşıklıkların azalması.

---

## 2) `Application` Kavramı (Ürün/Proje) ve Scope

### 2.1 Karar
- `Application` = **ürün/proje** kavramıdır (ERP, CRM, WMS vb.).
- `Application` **müşteri/tenant değildir**.

### 2.2 Neden `Application` içine connection konmuyor?
- `Application` “ürün”ü temsil ettiği için connection detayları ürün kavramına doğrudan ait değildir.
- Connection bilgisini `Application` içine koymak;
  - projelerin birden fazla DB’ye çıkması,
  - environment ayrımları,
  - secret rotasyonu,
  - farklı bağlantı profilleri
  gibi ihtiyaçlarda modeli gereksiz şekilde kilitler.

### 2.3 Scope yaklaşımı
- `Application` ile ilişkilendirilen ayarlar/policy’ler (parametreler) **ürün scope**’udur.
- Müşteri ayrımı instance/deployment ile sağlandığı için “tenant scope” bu seviyede taşınmaz.

---

## 3) Connection Yönetimi (Çoklu DB + Alias/Key)

### 3.1 Bağlantı çeşitliliği
- Tek bir instance yine de **birden fazla DB** ile konuşabilir.
  - Örn. ERP: operasyonel DB + raporlama DB + entegrasyon DB.
- Bu nedenle bağlantı kavramı “tek connection string” değil; **bağlantı seti / bağlantı profili** gibi ele alınır.

### 3.2 DB dataset’lerde connection seçimi (zorunlu)
- DB kaynaklı dataset’ler (`sp`, `view`, `table`) **mutlaka** önceden tanımlı bir **`ConnectionName`** (alias) ile çalışır.
- Dataset kayıtları:
  - yeni connection/şifre içermez,
  - “connection denemesi ekleme” içermez.
- `ConnectionName` çözümü sistemin genel parametre/konfig yapısındaki tanım üzerinden yapılır.
- Server bilgisi ayrıca dataset’te tutulmaz; connection içinden türetilir.

---

## 4) Secret Yönetimi (Güvenlik Önceliği)

### 4.1 Prensip
- “Connection bilgisini yönetmek” ile “secret saklamak” ayrıdır.
- Veritabanında (özellikle `_ArchiX`) saklanan yapı, tercihen connection’ın kendisi değil; connection’a giden **referans/anahtar (alias)** olmalıdır.

### 4.2 Kesin karar: Şifre `_ArchiX` içinde tutulmaz
- Tenant DB’lerine ait **User/Password** gibi secret’lar `_ArchiX` içinde **asla** saklanmaz.
- `_ArchiX` tarafında yalnızca secret’a giden bir referans saklanır.

### 4.3 Asgari secret referansı standardı
- Minimum desteklenecek format: `PasswordRef = ENV:<NAME>`
- Şifre uygulama çalıştığı ortamda (host/tenant) environment variable olarak sağlanır.

> Not: İleride güvenli secret store (örn. Key Vault vb.) ihtiyacı olursa yeni ref formatları eklenebilir; ancak bu dokümandaki minimum standart `ENV:`’dir.

---

## 5) Connection Security Policy (Zorunlu)

### 5.1 Politika yaklaşımı
- Tüm DB bağlantıları için merkezi bir **connection policy** yaklaşımı esastır.

### 5.2 Policy zorunlulukları (kontrol listesi)
- Şifreli iletişim zorunluluğu (Encrypt)
- Güvenilir olmayan sertifika kabulünün engellenmesi veya kontrollü olması (TrustServerCertificate)
- Integrated Security kullanımının kontrollü olması
- Hedef sunucu / IP aralığı whitelist

### 5.3 Audit ve veri sızıntısı kontrolü
- Bağlantı denemeleri/audit kayıtları tutulur.
- Audit kayıtlarında ham connection string **maskelenmiş** olarak saklanır.
- Amaç:
  - bağlantı güvenliği ihlallerini izlemek,
  - secret sızıntısını önlemek.

---

## 6) Connection Tanımının Nerede Duracağı (Bootstrap + Parametre)

### 6.1 Bootstrap connection (zorunlu başlangıç)
- Tenant/host, sistem altyapısını kullanabilmek için önce `ArchiX.Library` konfig DB’si olan `_ArchiX`’e bağlanabilmelidir.
- Bu bootstrap bağlantı, host tarafında konfigürasyon ile sağlanır (örn. `ConnectionStrings:ArchiXDb`).

### 6.2 Tenant uygulama DB connection’ları `_ArchiX` içinde tutulur
- Tenant’ın kendi DB’leri (örn. `Archix.Arsanmak`) host projesi tarafından kullanılacak olsa da connection tanımı `_ArchiX` içindedir.

### 6.3 Parametre standardı (tek satır / çok alias)
Tenant DB bağlantı seti, `_ArchiX` içindeki `Parameters` tablosunda **tek satır** olarak tutulur:

- `ApplicationId = 1` (Global zorunlu application)
- `Group = "ConnectionStrings"`
- `Key = "ConnectionStrings"`
- `Value` = JSON (tek value içinde çok alias)

### 6.4 JSON formatı (delimiter sorunu yok)
- Connection string içinde `;` bulunduğu için delimiter tabanlı formatlar kırılgan kabul edilir.
- Bu nedenle `Value` JSON olmalıdır.

Örnek şema (provider-agnostic, güvenlik odaklı):

- Her alias bir connection profile objesidir.
- `Auth` temel olarak `SqlLogin` varsayılır.
- `PasswordRef` sadece `SqlLogin` için vardır.

---

## 7) Teknoloji ve Çoklu Provider Yaklaşımı

### 7.1 Varsayılan / hedef olmayan teknoloji
- Default hedef: SQL Server + EF Core.
- Ancak bu “tek hedef” değildir; tenant ne kullanırsa (örn. Oracle kısmen mevcut), ihtiyaç olursa diğerleri de eklenir.

### 7.2 Tasarım çıktısı
- Connection ve dataset yürütme katmanı, ileride farklı DB provider’larına genişlemeye uygun tasarlanmalıdır.

---

## 8) Dataset-Driven Kombine (Tanım Tabanlı Rapor)

### 8.1 Dataset tablosu nerede?
- **Dataset tablosu `ArchiX.Library` / `_ArchiX` içinde olacak.**

### 8.2 Dataset kayıtları DB’de tutulur
- Dataset tanımları DB’de tutulur ve `BaseEntity`’den inherit eder.
- Dropdown’da listelenecek dataset’ler yalnızca **onaylı** olanlardır.
  - Onay kriteri: `StatusId == BaseEntity.ApprovedStatusId`

### 8.3 Dataset temel alanları
Dataset tanımında bulunması gerekenler:
- **ConnectionName**: DB kaynaklı dataset’lerde hangi connection alias’ın kullanılacağını belirtir.
- **FileName**:
  - DB için: SP/View/Table adı
  - File için: dosya adı
- **DisplayName**: UI dropdown’da görünen isim
- **Type**:
  - DB: `sp`, `view`, `table`
  - File: `json`, `ndjson`, `csv`, `txt`, `xml`, `xls`, `xlsx`

### 8.4 Source family (DB/File) ve genişleyebilirlik
- Kısa vadede kaynak ailesi: **DB** ve **File**.
- Uzun vadede yeni aileler (örn. API/Queue) gelebilir.
- Type kurgusu büyümeye uygun tasarlanmalıdır.
  - İstenirse `DatasetSourceType` ve `DatasetType` master tablolarla genişletilebilir.
  - Mevcut minimum hedef: `Type` bilgisinden DB/File çıkarımı yapılabilmesi.

---

## 9) Kombine Rapor Sayfası (Dinamik Çalışma Modeli)

### 9.1 Amaç
- `Kombine` sayfası, kullanıcının seçimine göre farklı kaynaklardan veri çekip aynı grid altyapısında gösterecek şekilde tasarlanır.

### 9.2 “Seçim” ve “Kaynak” ayrımı
- Seçilen seçenek bir “rapor tanımı / dataset tanımı” olarak ele alınır.
- Tanım içinde:
  - hangi veri kaynağının kullanılacağı,
  - grid kolonları,
  - (varsa) filtreleme/sıralama davranışı
  bulunur.

### 9.3 Kaynak tipinin açık olması
- Sadece “view adı” veya “json dosya adı” vermek yeterli değildir.
- Kaynağın türü açıkça belirtilmelidir (DB view, JSON, SP, Table, ileride API/Queue).
- Heuristik (uzantıdan tahmin vb.) kırılgan kabul edilir.

### 9.4 Beklenen backend davranışı
Seçilen dataset tanımına göre:
- doğru connection alias seçilir,
- connection policy uygulanır,
- uygun veri kaynağı yürütülür,
- grid’e uygun kolon/row üretilir.

---

## 10) File Dataset – Dosya Konumu ve Güvenlik

### 10.1 Root + alt dizin yaklaşımı
File dataset path şu şekilde oluşur:
- **Root**: sistem genelinde Parametre tablosunda tanımlıdır
- **Alt dizin(ler)**: dataset kaydında tanımlıdır
- **FileName**: dataset kaydında tanımlıdır

Amaç: path traversal riskini azaltmak (dataset yazan kişinin ana dizin dışına çıkmasını engellemek).

### 10.2 File dataset’lerde parametre şeması
- File dataset’lerde parametre tanımı/şeması dataset kaydının içinde bulunur.
- Parametre ekranı bu şemaya göre dinamik oluşur.

---

## 11) SP Dataset Parametre Davranışı

### 11.1 Kullanıcıdan parametre alınmaz
- SP dataset’lerde kullanıcıdan input parametresi alınmayacaktır.
- SP’nin output parametreleri de kullanıcıya filtre ekranı olarak gösterilmez.

### 11.2 Limit zorunluluğu
- Müşteri DB’lerinde SP’lerin `@MaxRows` benzeri garanti edilen limit parametreleri olmayabilir.
- Bu nedenle uygulama tarafında streaming sırasında limit korumaları zorunludur.

---

## 12) UI Davranışları (Kombine + GridListe Toolbar)

### 12.1 Dataset seçimi UI’da yeni bir obje
- Kombine ve GridListe toolbar’ına dataset seçici eklenir.
- Dropdown başlangıçta `null` gelir.
- Listede sadece `Approved` dataset’ler bulunur.
- Dropdown’da `DisplayName` gösterilir.

### 12.2 “Raporla” butonu
- Dropdown `null` değilse ve seçim değiştiyse enabled.
- Başarılı rapor çalıştıktan sonra disabled.
- Dataset seçimi değişince yeniden enabled.

---

## 13) Pivot Analiz ve Detaylı Liste (Tek Dataset, Tek Gerçek Veri)

1. Pivot Analiz ve Detaylı Liste aynı dataset çıktısından beslenir.
2. Sahte data yoktur; gerçek veri dataset çıktısıdır.
3. Grid filtreleri ile pivot senkronu ilk sürümde zorunlu değildir; gerekirse farklı yaklaşım uygulanabilir.

---

## 14) Kolonlar ve Formatlama

### 14.1 Kolonların tamamı alınır
- Dataset’den gelen kolonlar dinamik alınır.
- Kolon yapısı/isimleri dataset tasarımcısının sorumluluğundadır.

### 14.2 Başlık ve format
- Dataset’e bağlı tasarımda raporda görünecek kolon adları tanımlanabilir.
- Tarih/para/sayı formatları sistemde tanımlı standart formatlara göre uygulanır.

### 14.3 Gelecek planı
- İleride kolon seçme, kullanıcı bazında kolon tercihleri saklama gelebilir.
- Tasarım “dataset default” + “user override” modeline genişleyebilir olmalıdır.

---

## 15) Limitler ve Koruma Politikaları (Performans + Stabilite)

### 15.1 Birincil kontrol: hücre bütçesi
- `MaxCells = rows * cols`
- `allowedRows = floor(MaxCells / colCount)`

### 15.2 Hard limitler
- `HardMaxRows` ve `HardMaxCols` gibi üst sınırlar zorunludur.

### 15.3 Metin taşması limiti
- `MaxCellChars` (veya benzeri) limiti kabul edilmiştir.
- `MaxTotalChars` / `MaxBytes` secondary guard olabilir.

### 15.4 Ölçüm maliyeti
- Kontroller iki aşamalı olmalıdır:
  - Pre-check (policy + doğrulama)
  - Streaming sırasında ucuz kontroller (row/col/cell)
- `MaxBytes` gibi pahalı ölçümlerden kaçınılır.

### 15.5 Global vs dataset limit precedence
- Limitler hem sistem parametrelerinde hem dataset üzerinde tanımlı olabilir.
- Boş ise parametre tablosu geçerlidir.
- İkisi doluysa daha sıkı olan (min) kazanır:
  - Dataset > parametre ise parametre üst sınırdır.
  - Dataset < parametre ise dataset limiti geçerlidir.

---

## 16) Tutarlılık Kuralları (Özet)

1. Instance = müşteri izolasyonu. Tenant switching scope’a taşınmaz.
2. `Application` = ürün/proje. Connection detayı bu entity’ye eklenmez.
3. Connection yönetimi çoklu DB’yi destekler. Bağlantı “tek string” değil “profil”dür.
4. Connection policy zorunludur. Encrypt/whitelist vb. kurallar merkezi uygulanır; audit maskeli tutulur.
5. `Kombine` tanım (dataset) tabanlıdır. Kaynak türü açıkça belirtilir; seçime göre dinamik yükleme yapılır.
6. Dataset tablosu `ArchiX.Library` / `_ArchiX` içindedir.
7. Secret’lar `_ArchiX` içinde tutulmaz; minimum `ENV:` referansı ile yönetilir.

---

## 17) Örnek DB düşünce modeli (Not)

Örnek DB yapısı:

- `ArchiX.Library` → `_ArchiX` (library/config DB)
- `ERP` → örnek:
  - `Archix.ERP.Master` (ana veriler)
  - `Archix.ERP.Arsanmak2024` (müşteri/yıl bazlı operasyon verisi)
  - `Archix.ERP.Arsanmak2025`
  - `Archix.ERP.Arsanmak2025.Test`

---

## Son Not
Bu doküman “tek gerçek” olarak değerlendirilmelidir. Karar değişirse:
- değişiklik nedeni,
- tarih,
- etkilenen alanlar
mutlaka not edilmelidir.

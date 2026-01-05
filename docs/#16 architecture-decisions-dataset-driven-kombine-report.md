# ArchiX – Karar Özeti (Dataset-Driven Kombine Rapor / Çoklu Kaynak / Limitler)

Tarih: 2026-01-04  
Kapsam: #16 “Kombine Raporun datasetlerle çalışır duruma gelmesi” kapsamında; dataset tanımı, kaynak yönetimi (DB/File), connection seçimi, parametre davranışı, UI akışı, limit/koruma politikaları ve genişleyebilirlik kararları.

Bu doküman, sohbet boyunca alınan kararları **eksiksiz** ve **kod örneği olmadan** kalıcı hale getirmek içindir.

> Not: Bu doküman, önceki karar dokümanı olan `docs/architecture-decisions-multi-db-instance-scope-kombine.md` ile birlikte okunmalıdır. Bu dosyada özellikle dataset tabanlı Kombine raporlama tasarımının ayrıntılı kararları yer alır.

---

## 1) Ürün / Instance / Scope Yaklaşımı (Temel Kabuller)

1. **`Application` = ürün/proje** (ERP, CRM vb.).
2. **Müşteri bazında ayrım ayrı instance (deployment)** ile sağlanır.
3. `Application` içinde connection tutulmaz.
4. Connection bilgisi “çalışma zamanı kaynak seçimi”dir ve dataset/parametre/policy üzerinden yönetilir.

---

## 2) Connection ve DB Erişimi (Güvenlik + Çoklu DB)

### 2.1 Connection yönetimi (çoklu DB)
- Bir instance birden fazla DB’ye erişebilir.
- Bağlantı seçimi “tek connection string” üzerinden değil, **alias/key** üzerinden yapılır.

### 2.2 DB dataset’lerde connection seçimi
- DB kaynaklı dataset’ler (`sp`, `view`, `table`) **mutlaka** sistemde önceden tanımlı bir **`ConnectionName`** (alias) ile çalışır.
- Dataset kayıtları, yeni connection/şifre gibi bilgileri içermez; “connection denemesi ekleme” yoktur.
- `ConnectionName` çözümü sistemin genel parametre/konfig yapısında var olan tanım üzerinden yapılır.
- Server bilgisi ayrıca dataset’te tutulmaz; connection içinden türetilir.

### 2.3 Connection policy zorunluluğu
- Her DB çalıştırma öncesi connection policy (encrypt, trustcert, whitelist vb.) uygulanır.
- Audit kayıtları connection string’i maskeli tutar.

---

## 3) Dataset Modeli (DB Tasarım Tablosu)

### 3.1 Dataset kayıtları DB’de tutulur
- Dataset tanımları DB’de tutulur ve **`BaseEntity`**’den inherit eder.
- Dropdown’da listelenecek kayıtlar yalnızca **aktif** ve **onaylı** (Approved) olanlardır.

### 3.2 Dataset temel alanları
Dataset tanımında aşağıdaki bilgiler bulunur:
- **DB Name / ConnectionName**: DB kaynaklı dataset’lerde hangi connection alias’ın kullanılacağını belirler.
- **File Name**: SP/View/Table veya dosyanın gerçek adı (object name / file name).
- **Display Name**: UI dropdown’da görünen isim.
- **Type**: 
  - DB: `sp`, `view`, `table`
  - File: `json`, `ndjson`, `csv`, `txt`, `xml`, `xls`, `xlsx`

### 3.3 Source family (DB/File) ve ileriye dönük genişleme
- Kısa vadede kaynak ailesi **DB** ve **File** olmak üzere ikiye ayrılır.
- Uzun vadede yeni aileler (örn. API/Queue) gelebileceği kabul edilir.
- Bu nedenle kaynak ailesi/type kurgusu büyümeye uygun tasarlanmalıdır.
  - İstenirse: `DatasetSourceType` (db/file/api/queue) ve `DatasetType` (sp/view/json/…) şeklinde master tablolarla genişletilebilir.
  - Mevcut aşamada minimum gereksinim: `Type` bilgisinden DB/File çıkarımı yapılabilmesi.

---

## 4) File Dataset Dosya Konumu ve Güvenlik

### 4.1 Root + alt dizin yaklaşımı
- File dataset’ler için dosya yolu şu şekilde oluşturulur:
  - **Root dizin**: sistem genelinde **Parametre tablosunda** tanımlıdır.
  - **Alt dizin(ler)**: dataset kaydında tanımlıdır.
  - **FileName**: dataset kaydında tanımlıdır.
- Amaç: dataset yazan kişinin ana dizin dışına çıkmasını engellemek (path traversal riskini azaltmak).

### 4.2 Dosya kaynaklarında parametre şeması
- File dataset’lerde parametre tanımı/şeması **dataset kaydının içinde** bulunur.
- Parametre ekranı bu şemaya göre dinamik oluşur.

---

## 5) SP Dataset Parametre Davranışı

### 5.1 Kullanıcıdan parametre alma
- SP dataset’lerde **kullanıcıdan input parametresi alınmayacaktır**.
- SP’nin çıkış/çıktı parametreleri kullanıcıya filtre ekranı olarak gösterilmez.

### 5.2 SP boyutu ve limit zorunluluğu
- Yeni yazılan SP’lerin `@MaxRows` gibi parametreleri garanti edeceği varsayılamaz (müşteri DB’leri kontrol dışı).
- Bu nedenle uygulama tarafında limit korumaları (streaming sırasında kesme gibi) zorunlu kabul edilir.

---

## 6) UI – Kombine ve GridListe Toolbar Genişletmesi

### 6.1 Yeni üçüncü obje: Dataset seçimi
- Kombine ve GridListe sayfalarının toolbar’ına yeni bir dataset seçici eklenir:
  - Dropdown başlangıçta `null` (seçilmemiş) gelir.
  - Listede sadece **aktif** + **onaylı** dataset’ler bulunur.
  - Dropdown’da `DisplayName` gösterilir.

### 6.2 “Raporla” butonu davranışı
- “Raporla” butonu:
  - Dropdown `null` değilse ve seçim değiştiyse **enabled**.
  - Başarılı rapor çalıştıktan sonra **disabled**.
  - Dataset seçimi değişince yeniden enabled.

---

## 7) Pivot Analiz ve Detaylı Liste – Tek Dataset, Tek Gerçek Veri

1. Pivot Analiz ve Detaylı Liste **aynı dataset çıktısından** beslenir.
2. Detaylı liste ve pivot sahte data değil; dataset’den gelen gerçek veri ile oluşur.
3. Filtrelerin (grid filter/search) pivot ile nasıl senkronlanacağı ilk sürümde kesin bağlanmak zorunda değildir; gerekirse farklı bir yaklaşım uygulanabilir.

---

## 8) Kolonlar ve Formatlama

### 8.1 Kolonların tamamı alınır
- Dataset’den gelen veri/kolonlar dinamik olarak alınır.
- Tasarımcı dataset’i üretirken kolon yapısını/isimlerini doğru kurgulamakla sorumludur.

### 8.2 Kolon başlıkları ve format
- Dataset’in bağlı olduğu tasarımda raporda görünecek kolon adları tanımlanabilir.
- Tarih/para/sayı gibi formatlar sistemde tanımlı standart formatlara göre uygulanır.
- Grid başlık ve satırları bu yapı ile dinamik oluşturulur.

### 8.3 Gelecek planı (kapsam dışı ama tasarımı etkileyecek)
- İleride kolon seçme, kullanıcı bazında kolon tercihlerini saklama gibi yetenekler eklenecektir.
- Bu nedenle mevcut tasarım, “dataset default” + “user override” modeline genişleyebilir olmalıdır.

---

## 9) Limitler ve Koruma Politikaları (Performans + Stabilite)

### 9.1 Birincil kontrol: hücre bütçesi
- İçerik boyutu bilinmediği için birincil yaklaşım hücre sayısını kontrol etmektir:
  - **`MaxCells`** = `rows * cols` bütçesi
  - `allowedRows = floor(MaxCells / colCount)` yaklaşımı

### 9.2 Hard limitler
- `HardMaxRows` ve `HardMaxCols` gibi üst sınırlar tanımlanır.
- Bu limitler UI/pivot performansını korumak için zorunlu kabul edilir.

### 9.3 Metin taşması için hücre limiti
- “Tek kolonda devasa metin” gibi durumlar için **`MaxCellChars`** (veya benzeri) limit önerisi kabul edilmiştir.
- `MaxTotalChars` / `MaxBytes` gibi toplam limitler “secondary guard” olarak kullanılabilir.

### 9.4 Ölçüm maliyeti
- Limit kontrolleri iki aşamalı kurgulanmalıdır:
  - Pre-check (policy + doğrulama)
  - Streaming sırasında ucuz kontroller (row/col/cell)
- `MaxBytes` gibi ölçümler pahalı yapılmamalı; gerekiyorsa `MaxTotalChars` gibi daha ucuz yaklaşımlar tercih edilmelidir.

### 9.5 Global vs dataset limit precedence
- Limitler hem sistem parametrelerinde hem dataset üzerinde tanımlanabilir.
- **Boş ise parametre tablosu geçerlidir.**
- İkisi doluysa **daha sıkı olan (min) kazanır**:
  - Dataset değeri parametrelerden büyük olsa bile parametre tablosu üst sınırdır.
  - Dataset değeri daha küçükse dataset limiti geçerlidir.

---

## 10) Örnek DB düşünce modeli (Not)

- `ArchiX.Library` → `_ArchiX` (library/config DB)
- `ERP` → master DB ve müşteri/yıl bazlı operasyon DB’leri (instance yaklaşımına göre bu kısım farklı deployment’lara ayrılabilir)

---

## Son Not
Bu doküman; dataset tabanlı raporlamaya geçişte kararların “tek gerçek” kaydıdır. Karar değişirse:
- değişiklik nedeni,
- tarih,
- etkilenen alanlar
not edilmelidir.

Dataset tablosu ArchiX.Library (_ArchiX) içinde olacak
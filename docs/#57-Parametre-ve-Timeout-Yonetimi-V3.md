# #57 — Parametre ve Timeout Yönetimi (DB Driven)

(Revize 3: 22.01.2026)

> Not: Bu dokümanın kapanmış/final hali `docs/#57-Parametre-ve-Timeout-Yonetimi-V4.md` dosyasıdır.

## 1) Amaç

### 1.A Analiz / Tasarım
1.1 Amaç: Timeout ve benzeri parametrik kuralların **tamamının DB Parametre tablosu üzerinden** yönetilebilir hale getirilmesi.
1.2 “Hiçbir kural koda gömülü kalmayacak.” prensibi: runtime davranışlar DB parametresinden okunacak.
1.3 Bu iş kapsamında “timeout” kelimesi yalnız UI session değil; aşağıdaki grupları kapsar:
   - UI (TabHost) session/idle ve tab request timeout
   - Outbound HTTP (retry + timeout)
   - Güvenlik (AttemptLimiter: window/maxAttempts/cooldown)
1.4 Bu doküman, `docs/Timeout Parametre Listesi - Son.csv` dosyasındaki satırları **tek tek yorumlayarak** teknik tasarım ve iş planı üretir.

### 1.B Güvenlik Prensibi (Öncelik)
1.5 Büyük kural: **Önce güvenlik (saldırı riski dahil), sonra performans.**
1.6 Parametreler DB’den okunamadığında uygulama çalışmayacak (fail-open yok). Kullanıcı kararı: “DB koparsa uygulama tamamı çalışamaz.”

---

## 2) Mevcut Sistem Referansı (#42)

### 2.A Analiz / Tasarım
2.1 Projede DB’den JSON parametre okuma paterni zaten var: `docs/#42-tabbed-tum-sayfalar-her-zaman-tab-page-altinda.md`.
2.2 #42’de kritik yaklaşım:
   - **Tek satır JSON parametre** modeli (Group+Key+ApplicationId)
   - `ApplicationId=1` (sende net: “application bazlı id=1 olacak”)
   - JSON value + template + description ile admin yönetimi
2.3 #42’de TabbedOptions JSON örneğinde şunlar var:
   - `tabbed.maxOpenTabs`, `tabAutoCloseMinutes`, `autoCloseWarningSeconds`, `sessionTimeoutWarningSeconds` vb.
2.4 #57 için çıkarım: Timeout ile ilgili parametreler de aynı yaklaşımda **tek JSON** veya “az sayıda JSON parametre” olarak yönetilebilir.

---

## 3) Scope, Application Bağı ve Kaynak Önceliği (Kullanıcı Kararı)

### 3.A Scope (Mevcut karar)
3.1 (İlk karar) Scope: **Application bazlı** ve pratikte `ApplicationId=1` ile global çalışacak.

### 3.B Tasarım Hatası / Yeni Gereksinim: Application ↔ Parameter ilişkisi many-to-many
3.2 Düzeltme (kullanıcı netleştirdi): “many-to-many” değil, master/detail tasarım.
3.3 Yeni hedef model:
   - `Parameters` (master/definition)
     - Parametrenin kimliği ve tanımı: `Group`, `Key`, `ParameterDataTypeId`, `Description`, `Template`, vb.
   - `ParameterApplications` (detail/value)
     - Değerin application’a göre karşılığı: `ParameterId`, `ApplicationId`, `Value` (gerekirse `RowVersion`, `StatusId`).
     - Unique: `(ParameterId, ApplicationId)`.
3.4 Çalışma kuralı (fallback mantığı):
   - Eğer bir parametre için sadece `ApplicationId=1` satırı varsa, bu değer “tüm application’lar için ortak default” sayılır.
   - Eğer aynı `ParameterId` için `ApplicationId=2` gibi özel bir satır da varsa:
     - `ApplicationId=2` sorgusunda önce `ApplicationId=2` satırı aranır.
     - Bulunamazsa `ApplicationId=1` satırına fallback yapılır.
3.5 Bu kural, “scope” ihtiyacını DB üzerinden çözer ve #57’deki “ApplicationId=1 global” yaklaşımını bozmadan genişletir.
3.6 Etki: Parametre okuma sorguları değişir; admin ekranı ve seed/migration etkilenir.
3.7 Bu nedenle #57 iki aşamaya ayrılır:
   - (A) **Önce** yeni şema (master/detail) + migrate + okuma servisleri uyarlama
   - (B) Sonra timeout parametrelerinin DB’ye taşınması

### 3.C Kaynak Önceliği (Precedence)
3.7 Kullanıcı kararı: Config her zaman DB’den yönetilecek; `appsettings` veya environment override kullanılmayacak.
3.8 Teknik sonuç:
   - Parametre servisinin tek kaynağı DB.
   - Uygulama başlangıcında DB erişimi doğrulanamazsa **startup fail** (uygulama açılmayacak).

### 3.D Fallback
3.9 Kullanıcı kararı: DB down ise uygulama çalışmamalı.
3.10 Teknik sonuç:
   - “Hard-coded default ile devam et” gibi fallback yok (özellikle güvenlik için istenmiyor).
   - Bu nedenle parametre servisi kritik dependency’dir.

---

## 4) Parametre Envanteri (CSV Yorumu) — Ok Satırları

Kaynak: `docs/Timeout Parametre Listesi - Son.csv`

> Aşağıdaki maddeler CSV’de `Durum=ok` olan satırların her birinin sistemdeki etkisini ve DB’de nasıl tutulacağını açıklar.

### 4.1 UI / TabHost (Frontend)

4.1.1 (No 1) `UI.SessionTimeoutSeconds` (`sessionTimeoutSeconds`)
- Anlam: Kullanıcı etkileşimi yoksa “oturum zaman aşımı” akışı kaç saniye sonra devreye girer.
- Hedef default: 645 sn.
- Etki alanı:
  - TabHost idle tracking.
  - Session warning ve logout timer ile birlikte çalışır.
- Not: #42’de `fullPage.sessionTimeoutWarningSeconds` gibi alanlar var; çakışma yaşamamak için **tek kaynak** seçilecek.

4.1.2 (No 2) `UI.SessionWarningSeconds` (`sessionWarningSeconds`)
- Anlam: Timeout’a kaç saniye kala uyarı gösterileceği.
- Hedef default: 45 sn.
- Güvenlik etkisi: düşük/orta. Daha çok UX.

4.1.3 (No 3) `UI.TabRequestTimeoutMs` (`tabRequestTimeoutMs`)
- Anlam: Tab içeriği fetch edilirken istek yanıt vermezse kaç ms sonra “timeout” kabul edilecek.
- Hedef default: **45000 ms (45 sn)**.
- Güvenlik etkisi: düşük. Performans/UX etkisi: yüksek.

4.1.4 (No 4) `UI.MaxOpenTabs` (`maxOpenTabs`)
- Anlam: Kullanıcının açabileceği maksimum tab sayısı.
- Default: 15.
- Not: #42’de zaten `tabbed.maxOpenTabs` var. İki ayrı kaynak olursa yönetim zorlaşır.
  - Tasarım kararı: `maxOpenTabs` **timeout parametresi değildir** ve **UI/TabbedOptions** içinde kalacaktır (tek kaynak).

### 4.2 Outbound HTTP (Backend)

4.2.1 (No 5) `HTTP.RetryCount` (`retryCount`)
- Anlam: Dış servislere giden HTTP çağrıları hata aldığında kaç defa daha denenecek.
- Hedef default: **2** (toplam 3 deneme: ilk + 2 retry).
- Güvenlik: Retry saldırı yüzeyi artırabilir (aynı endpoint’e yük bindirir). Bu nedenle üst limite ihtiyaç var.

4.2.2 (No 6) `HTTP.BaseDelayMs` (`baseDelayMs`)
- Anlam: Retry denemeleri arasındaki ilk bekleme süresi.
- Hedef default: **250ms**.

4.2.3 (No 7) `HTTP.TimeoutSeconds` (`timeoutSeconds`)
- Anlam: Dış servise giden bir isteğin maksimum süresi.
- Hedef default: **30 saniye**.
- Not: Mevcut kodda 30s/100s gibi birden fazla timeout default’u var. DB’ye geçişte tek timeout standardı olmalı.

### 4.3 Güvenlik — AttemptLimiter

4.3.1 (No 8) `Security.AttemptLimiter.Window` (`window`)
- Anlam: Denemeler hangi süre penceresi içinde sayılır (örn 10 dk).
- Hedef default: **600 sn (10 dakika)**.
- Güvenlik: çok yüksek etki.

4.3.2 (No 9) `Security.AttemptLimiter.MaxAttempts` (`maxAttempts`)
- Anlam: Bu pencere içinde izin verilen maksimum deneme sayısı.
- Hedef default: **5**.
- Güvenlik: çok yüksek etki.

4.3.3 (No 10) `Security.AttemptLimiter.Cooldown` (`cooldownSeconds`)
- Anlam: Limit aşılınca kaç saniye bloklansın.
- Hedef default: 300 sn.

---

## 5) DB Parametre Modeli (Teknik Tasarım)

### 5.A Parametre Kaydı Formatı (Mevcut model ile)
5.1 Mevcut kod modelinde parametre benzersizliği: `Group + Key + ApplicationId`.
5.2 Bu model `src/ArchiX.Library/Entities/Parameter.cs` üzerinde açıkça var.
5.3 Ayrıca `AppDbContext` seed içinde `Application Id=1 (Global)` tanımlı.
5.4 #57 için iki alternatif (mevcut şema devam ederse):

**Alternatif A — Çoklu Parametre (3 JSON)**
- `Group=UI`, `Key=TimeoutOptions`, `ApplicationId=1`, `DataType=Json`
- `Group=HTTP`, `Key=HttpPoliciesOptions`, `ApplicationId=1`, `DataType=Json`
- `Group=Security`, `Key=AttemptLimiterOptions`, `ApplicationId=1`, `DataType=Json`

**Alternatif B — Tek Parametre (1 JSON Root)**
- `Group=System`, `Key=RuntimePolicies`, `ApplicationId=1`, `DataType=Json`
- JSON: `{ version, ui:{...}, http:{...}, security:{...}, refresh:{...} }`

5.5 Öneri (güvenlik + yönetilebilirlik): **Alternatif A**.
- Sebep: Admin ekranında değişiklikler daha net ayrılır, hata etkisi daralır.
- Tek JSON’da bir typo tüm sistemi kilitleyebilir.

### 5.B JSON Field Name Mapping
5.6 CSV’deki `JsonFieldName` kolonları JSON iç alan adlarıdır.
5.7 Örnek:
- UI JSON: `{ "sessionTimeoutSeconds": 645, "sessionWarningSeconds": 45, "tabRequestTimeoutMs": 45000, "maxOpenTabs": 15 }`
- UI Timeout JSON: `{ "sessionTimeoutSeconds": 645, "sessionWarningSeconds": 45, "tabRequestTimeoutMs": 45000 }`
- HTTP JSON: `{ "retryCount": 2, "baseDelayMs": 250, "timeoutSeconds": 30 }`
- Security JSON: `{ "window": 600, "maxAttempts": 5, "cooldownSeconds": 300 }`

### 5.C UI tarafı kesin ayrım: Tab davranışı vs Timeout
5.8 Kullanıcı kararı: `maxOpenTabs` **yerinde kalacak**.
- Kaynak: DB `UI/TabbedOptions` (Id=3) içindeki `tabbed.maxOpenTabs`.
- Bu alan #57 kapsamında **yeni timeout parametresi** altına taşınmayacak.

5.9 #57 kapsamında UI için DB’ye taşınacak yeni timeout alanları (yalnızca 3 adet):
- `sessionTimeoutSeconds`
- `sessionWarningSeconds`
- `tabRequestTimeoutMs`

5.10 Etki: UI tarafında iki farklı config objesi olacak:
- `window.ArchiX.UiOptions` (kaynak: `UI/TabbedOptions`) → tab davranışı (örn `maxOpenTabs`, nested tab vb.)
- `window.ArchiX.TimeoutOptions` (kaynak: `UI/TimeoutOptions`) → timeout davranışı (3 alan)

---

## 6) Parametre Okuma, Cache, Güvenlik

### 6.A “DB Down ise uygulama çalışmasın” gereksinimi
6.1 Uygulama start olurken parametrelerin DB’den okunabildiği doğrulanmalı.
6.2 Okuma başarısızsa exception ile startup fail edilmeli.

### 6.B Cache gerekli mi?
6.3 Evet, performans için cache gerekir ama güvenlik yaklaşımıyla.
6.4 Cache’in amacı DB’yi korumak (yük/latency) ve request başına DB erişimini engellemektir.

### 6.C Cache nerede olmalı?
6.5 Kullanıcı bazlı değil; **application bazlı** (çünkü scope application=1).
6.6 `IMemoryCache` (process memory) yeterli olabilir.
6.7 Çoklu instance (scale-out) varsa, “değişiklik yayılımı” için invalidation gerekir (Redis pub/sub vs). Bu karar daha sonra.

### 6.D Cache güvenlik riski oluşturur mu?
6.8 Evet, iki risk var:
- “Stale config” riski: admin değiştirir ama uygulama hemen görmez.
- “Cache poisoning” riski: parametre değeri yetkisiz şekilde değişirse bütün instance etkilenir.

6.9 Önlemler:
- Parametre update endpoint’i ciddi yetki kontrolü + audit.
- Parametre JSON validasyonu (schema/range) başarısızsa kaydetme engellenmeli.
- Cache TTL kısa tutulabilir veya admin kaydederken cache invalidation yapılır.

6.10 Netleşen yaklaşım (Audit): Parametre değişiklikleri için audit kayıtları DB’de tutulacak (raporlanabilir). Uygulama log altyapısı şimdilik askıda; entegrasyon daha sonra.

6.11 Cache yaklaşımı: Parametreler **lazy-load + in-memory cache (IMemoryCache)** ile okunacak.
6.12 Startup kritik kontrol: Uygulama açılışında **kritik parametre setleri** (Security/PasswordPolicy + Security/AttemptLimiterOptions + HTTP/HttpPoliciesOptions + UI/TimeoutOptions) DB’den okunup parse edilerek doğrulanacak. Başarısızsa startup fail.
6.13 Canlı güncelleme: Admin update sonrası “hemen yansısın” kesin gereksinimi yoksa, ilk fazda **TTL ile eventual consistency** yeterli kabul edilir.
6.14 Güvenlik notu (kullanıcı pasif edilirse): Bu iş (#57) sadece “parametre ve timeout yönetimi” kapsamıdır.
   - “User pasif edildi ama oturumu kapatmadı” gibi durumlar **authentication/session yönetimi** (cookie, token, session store) kapsamında ele alınır.
   - Ancak #57 kapsamında UI idle timeout’un DB’den yönetilebilir olması, “kullanıcı yıllarca açık kalsın” riskini azaltmaya yardımcı olur.

---

## 7) Değişiklik Yayılımı (Refresh) — Parametrik Olmalı

### 7.A Kullanıcı isteği
7.1 “Bu genel parametre yönetimi mi? hepsi için geçerli olabilir. Bu da parametrik bir tanım olmalı.”

### 7.B Tasarım
7.2 Öneri: Refresh/TTL ayrı bir sistem parametresi olsun.
- Örn: `Group=System`, `Key=ParameterRefresh`, JSON: `{ "cacheTtlSeconds": 60 }`
7.3 Bu TTL tüm parametreler için geçerli olabilir.
7.4 Güvenlik parametreleri için ayrı TTL gerekebilir (opsiyon): `{ securityCacheTtlSeconds }`.

7.5 İlk faz için yaklaşım: **grup bazlı TTL**.
   - Gerekçe: Security parametreleri daha hızlı güncellenebilsin; UI daha uzun TTL ile DB yükü düşsün.
7.6 Hedef default’lar (DB parametresiyle yönetilecek):
   - `Security`: 30 sn
   - `HTTP`: 60 sn
   - `UI`: 300 sn
7.7 Operasyon kararı: Şimdilik **tek sunucu (single instance)** çalışılacak. İleride scale-out ihtiyacı olursa, cache invalidation için Redis/pub-sub veya ConfigVersion yaklaşımı ayrıca ele alınacak.

---

## 8) UI Tarafı (JavaScript) Parametreleri Nasıl Alacak?

### 8.A Mevcut yapı
8.1 TabHost JS (`archix-tabhost.js`) bir `config` objesi ile yönetiliyor.
8.2 #42’de `TabbedOptions` gibi JSON parametre DB’den okunup UI’a taşınıyor.

### 8.B Modern yaklaşım (Razor Pages)
8.3 En güvenli ve basit yöntem: Razor Layout/Page render sırasında server-side config’i üretip JS’e enjekte etmek.
- Örn: `<script>window.ArchiX = window.ArchiX || {}; window.ArchiX.Config = {...};</script>`
8.4 Bu config’in kaynağı DB parametre servisidir.
8.5 Böylece JS doğrudan DB’ye ulaşmaz (güvenlik sınırı net).

8.7 Seçilen yöntem: **Server-side render ile layout içine JSON enjekte** (Razor).
   - Gerekçe (güvenlik): UI parametreleri için ayrıca bir API endpoint açıp CORS/CSRF/authorization yüzeyi büyütülmez.
   - Gerekçe (performans): Ek HTTP round-trip yok; HTML ile birlikte gelir.
8.8 Not: JS’e verilen config sadece “data” içerir; çalıştırılabilir kod veya HTML şablonu taşınmaz.

### 8.C Etki analizi (kırılma olmaması için)
8.9 Mevcut durumda TabHost JS iki farklı kaynaktan değer okuyabiliyor/okuyacak şekilde düzenlenmelidir:
- `maxOpenTabs` → `window.ArchiX.UiOptions.tabbed.maxOpenTabs`
- Timeout alanları (3 adet) → `window.ArchiX.TimeoutOptions.{sessionTimeoutSeconds, sessionWarningSeconds, tabRequestTimeoutMs}`

8.10 Etkilenecek dosyalar (en az):
- `src/ArchiX.Library.Web/wwwroot/js/archix/tabbed/archix-tabhost.js` (asıl kaynak)
- `src/ArchiX.WebHost/wwwroot/js/archix/tabbed/archix-tabhost.js` (build sırasında kopyalanan sürüm)
- Razor Layout / server-side injection noktası (ArchiX config’i JS’e basan view/layout dosyası)

8.11 Geriye dönük uyumluluk (mevcut müşteriler/seed kırılmasın):
- Eğer `UI/TimeoutOptions` daha sisteme eklenmediyse, TabHost JS default değerlerle çalışmaya devam eder.
- Eğer `maxOpenTabs` sadece `UI/TabbedOptions` içinde varsa (mevcut durum), TabHost JS bunu oradan okur.

---

## 9) Validasyon (Min/Max) — Konuşulacak Alan

### 9.A Kullanıcı notu
9.1 “Admin düşünsün ama sen istersen koda gömülü falan olabilir. bunu konuşalım.”

### 9.B Öneri
9.2 Validasyon iki katmanlı olmalı:
- Admin UI: kullanıcı hatasını erken yakalamak için.
- Backend: DB’ye hatalı/tehlikeli değer girmesini engellemek için (güvenlik).

9.3 “Koda gömülü validasyon” kural sayılmaz; koruma bariyeridir.
- Bu, “kural DB’den olsun” ilkesini bozmaz.

9.4 Seçilen yaklaşım: **Admin UI + Backend** birlikte validasyon yapacak.
   - Admin UI: daha iyi UX; hata erken görülür.
   - Backend: “kötü niyetli/yanlış” istekleri kesin engeller (security boundary).
9.5 Validasyon tipi:
   - Tip kontrolü (int/time-span)
   - Range (min/max)
   - JSON schema doğrulaması (parametre JSON formatı)
9.6 Range sınırları “iş kuralı” değil, koruma bariyeridir ve koda gömülü tutulabilir.

---

## 10) Audit / Yetkilendirme

### 10.A Gereksinim
10.1 “Olabilir.” kararı var.
10.2 Parametre değişiklikleri audit edilirse:
- Hangi kullanıcı değiştirdi
- Ne zaman
- Eski değer / yeni değer
- Hangi key

### 10.B Öneri
10.3 Parametre değişim ekranı sadece admin rolü.
10.4 Audit/loglama bu iş kapsamında **yapılmayacak**. (İleride tüm sistem için merkezi loglama ele alınacak.)

---

## 11) Mevcut Parametre Kayıtları (DB Seed) ve Kullanım Analizi (Etki Analizi)

### 11.A Mevcut Seed Parametreler
11.1 `AppDbContext.OnModelCreating` içinde şu parametre seed’leri var:
   - (Id=1) `Group=TwoFactor`, `Key=Options`, `ApplicationId=1`, `DataType=Json`
   - (Id=2) `Group=Security`, `Key=PasswordPolicy`, `ApplicationId=1`, `DataType=Json`
   - (Id=3) `Group=UI`, `Key=TabbedOptions`, `ApplicationId=1`, `DataType=Json`

### 11.B Bu parametreler nerelerde kullanılıyor?
11.2 `UI/TabbedOptions` kullanımı:
   - `tests/ArchiX.Library.Tests/Tests/PersistenceTests/TabbedOptionsSeedTests.cs` içinde seed varlığı test ediliyor.
   - `archix-tabhost.js` tarafında `maxOpenTabs` vb. config alanları kullanılıyor; bu config’in kaynağı #42’de DB parametre olarak tasarlanmış.

11.3 `Security/PasswordPolicy` kullanımı:
   - `src/ArchiX.Library/Runtime/Security/PasswordPolicyProvider.cs` DB’den okuyor ve `IMemoryCache` ile cache ediyor.
   - `src/ArchiX.Library/Runtime/Security/PasswordPolicyAdminService.cs` admin update + schema validation + integrity kontrolleri içeriyor.
   - Bu iki dosya, parametre sisteminin “runtime read + admin write + cache + invalidate” desenini zaten göstermiş oluyor.

11.4 `TwoFactor/Options` kullanımı:
   - DI tarafında `SecurityServiceCollectionExtensions.TwoFactor.cs` config section ile `TwoFactorOptions` bind ediyor (şu an appsettings binding). Ancak DB’de seed satırı bulunuyor.
   - Bu, sistemde “aynı konunun hem appsettings hem DB parametre” ile yönetiliyor olabileceğini gösterir (çakışma riski).

11.5 Parameter model refactor (many-to-many + value tabloları) bu üç parametreyi de etkiler.
   - TabbedOptions (#42) kırılırsa tabbed navigasyon/limit/auto-close davranışları etkilenir.
   - PasswordPolicy kırılırsa login/güvenlik akışları etkilenir.
   - TwoFactor Options kırılırsa 2FA akışları etkilenir.

### 11.C Many-to-many refactor etkisi (risk analizi)
11.6 Mevcut şema: `Parameter.ApplicationId` FK.
11.7 Yeni şema ihtiyacı: Application ↔ ParameterDefinition arasına join/value.
11.8 Etkiler:
   - Sorgular değişecek (bugün `db.Parameters.Where(p => p.ApplicationId==1 && p.Group==... && p.Key==...)`).
   - Unique index değişecek (bugün `Group+Key+ApplicationId`).
   - Seed/migration değişecek.
   - Admin ekranları (Parameter edit) değişecek.
   - Cache key’leri (ör. `PasswordPolicy:{appId}`) aynı kalabilir ama veri kaynağı query değişir.
11.9 Bu nedenle “timeout parametrelerini DB’ye taşıma” işi ile “Parameter şema refactor” işi farklı risk/proje büyüklüğü.

---

## 12) Unit Test Planı

### 12.A Analiz / Tasarım
12.1 DB parametre okunamazsa uygulama start olmaz (integration test / startup test).
12.2 JSON parse edilmezse:
- Admin kaydı engellenmeli (unit test)
- Runtime’da da hata net loglanmalı.
12.3 UI config üretilen endpoint/layout test:
- Üretilen JSON’da beklenen field’lar var.
12.4 HTTP policy test:
- RetryCount/BaseDelayMs/TimeoutSeconds DB’den gelince handler’larda o değerlerin uygulandığı doğrulanır.
12.5 AttemptLimiter test:
- Window/MaxAttempts/Cooldown değerlerinin DB’den okunup etkili olduğu doğrulanır.

---

## 13) Yapılacak İşler (İş Sırası — Yapılış Sırası)

### 13.A Analiz / Tasarım
13.1 Parametre anahtarları netleştirme (Alternatif A mı B mi?)
13.2 `docs/Timeout Parametre Listesi - Son.csv` içindeki 1..10 için DB JSON şeması çıkarma.
13.3 #42 TabbedOptions ile timeout alanlarının ayrıştırılması:
- `tabbed.maxOpenTabs` **TabbedOptions içinde kalacak** (tek kaynak).
- `sessionTimeoutSeconds`, `sessionWarningSeconds`, `tabRequestTimeoutMs` → yeni `UI/TimeoutOptions` parametresine taşınacak.
13.4 Etki analizi: Hangi dosyaların kırılabileceği ve geriye dönük uyumluluk planı (8.9-8.11).
13.5 Parametre refresh/TTL parametresi tasarımı.
13.6 Admin UI validasyon yaklaşımı (min/max).
13.7 Audit taslağı (iptal edildi / sonraya bırakıldı).

13.8 NEW: Parameter şema refactor etki analizi (many-to-many + value ayrıştırma)
- Mevcut kullanılan parametreler: `UI/TabbedOptions`, `Security/PasswordPolicy`, `TwoFactor/Options`
- Query/seed/admin UI/test etkileri
- Geçiş stratejisi: migration + data move + backward compatibility (gerekirse)

### 13.B Implementasyon
13.9 Parametre okuma servisi + cache uygulaması.
13.10 Startup check: DB erişimi yoksa fail.
13.11 UI config injection (Razor pages layout): `UiOptions` + `TimeoutOptions` ayrı ayrı enjekte.
13.12 TabHost JS okuma noktalarını güncelle:
   - `maxOpenTabs` → `UiOptions.tabbed.maxOpenTabs`
   - timeout alanları → `TimeoutOptions.*`
13.13 HTTP policy değerlerini DB’den okutma.
13.14 AttemptLimiter değerlerini DB’den okutma.

13.13 (Ayrı iş olabilir) Parameter şema refactor implementasyonu
- Yeni tablolar
- Data migration
- Okuma servislerinin uyarlanması
- Admin ekranlarının uyarlanması

### 13.C Test
13.15 Unit/integration test ekleme (12. bölüm).

---

## 99) Kapanış

Bu dokümanda açık soru/öneri bırakılmadı. Gereksinimler ve tasarım kararları ilgili bölümlere işlenmiştir.

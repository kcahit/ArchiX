# #42 — Tüm Sayfaların Her Zaman Tab Page Altında Toplanması (Tabbed)

(Revize 2: 15.01.2026 00:37

## 1) Amaç

### 1.A Analiz / Tasarım
1.1 `navigationMode: "Tabbed"` olduğunda bu dokümandaki Tabbed kuralları çalışır.
1.2 Uygulamanın tüm ekranları tek bir sağ çalışma alanında (tab host) “desktop gibi” çalışır.
1.3 Tam sayfa (FullPage) açılış davranışı kullanılmaz (parametre ile aksi seçilirse değişir).

### 1.B Unit Test
1.4 `navigationMode="Tabbed"` iken Tabbed akışlarının aktif olduğu doğrulanır.
1.5 `navigationMode="FullPage"` iken Tabbed’e özgü limit/auto-close/requireTabContext gibi kuralların devre dışı kaldığı doğrulanır.

---

## 2) UI / Navigasyon

### 2.A Analiz / Tasarım
2.1 Uygulama içindeki tüm navigasyon aksiyonları tab açar (sidebar + sayfa içi link + buton + grid aksiyonları + rapor linkleri).
2.2 Multi-instance serbest: aynı ekran/route aynı parametrelerle bile tekrar tab açılabilir.
2.3 Tab adları unique üretilir; aynı ad tekrar ederse `_001`, `_002` … suffix eklenir (menü adı + suffix).
2.4 Tab kapanınca bir önceki tab aktif olur; hiç tab kalmazsa `Home/Dashboard` otomatik açılır.
2.5 Max tab limiti dolunca yeni tab açılmaz; uyarı: “Maksimum tab limiti doldu. Lütfen bir tab kapatınız.”

### 2.B Unit Test
2.6 Aynı route/parametre ile birden fazla tab açılabildiği doğrulanır.
2.7 Unique başlık: aynı başlıkla 3 tab açınca `_001/_002` eklenmesi doğrulanır.
2.8 Son tab kapanınca `Home/Dashboard` otomatik açılması doğrulanır.
2.9 Max tab doluyken yeni tab açma engeli + uyarı mesajı doğrulanır.

---

## 3) Nested Tab (Parametre)

### 3.A Analiz / Tasarım
3.1 Tab altında tab (nested tabs) parametre ile açılıp kapatılır.
3.2 Nested tab hiyerarşisi **kesin** sidebar hiyerarşisi ile aynı olmak zorundadır.

### 3.B Unit Test
3.3 `enableNestedTabs=true` iken nested tab açıldığı doğrulanır.
3.4 `enableNestedTabs=false` iken nested tab açılmadığı doğrulanır.
3.5 Sidebar hiyerarşisi dışındaki nested düzenin engellendiği (fail-closed) doğrulanır.

---

## 4) Response Card (401/403/404)

### 4.A Analiz / Tasarım
4.1 401/403/404 gibi durumlarda tab alanında standart response card gösterilir.
4.2 Response card butonları: `Kapat` + `Kopyala` (zorunlu).
4.3 `Kopyala` içeriği: `TraceId` + uyarı mesajı metni (hassas bilgi yok).

### 4.B Unit Test
4.4 401/403/404 için response card render edildiği doğrulanır.
4.5 Kopyala içeriğinde TraceId ve mesaj olduğu, hassas bilgi olmadığı doğrulanır.
4.6 Kapat tıklanınca tabın kapandığı doğrulanır.

---

## 5) Güvenlik Kararları

### 5.A Analiz / Tasarım
5.1 Tabbed model güvenlik sınırı değildir; güvenlik endpoint seviyesinde sağlanır. (Dışarıdan linkle gelen için güvenlik hissi verir.)
5.2 Default-deny: yeni sayfalar dahil tüm endpoint’ler varsayılan kapalı; anonim sayfalar bilinçli istisna ile açılır.
5.3 Yetki sistemi geçici: altyapı kurulacak, sistem çalışır halde olacak; ancak gerekli DB tablo ve class’lar oluşturulmadığı için şimdilik “yetki var” kabul edilerek ilerlenir (sonra revize).
5.4 State-changing işlemler POST/PUT/DELETE + CSRF/antiforgery.
5.5 `requireTabContext=true` iken direct URL ile gelirse sadece mesaj + sadece `Kapat` butonu.
5.6 Mesaj sabit: “Bu ekrana link ile giriş yapılamaz. Bu ekran yalnızca uygulama içinden açılmalıdır.”
5.10 `requireTabContext` doğrulaması teknik işareti: Tab konteynerinden yapılan istekler `X-ArchiX-Tab: 1` header’ı ile gönderilir. `requireTabContext=true` iken bu header yoksa direct URL kabul edilir.

### 5.B Unit Test
5.7 Default-deny: `[AllowAnonymous]` olmayan sayfalara anonim erişimin engellendiği doğrulanır.
5.8 State-changing endpoint’lerin GET ile çalışmadığı doğrulanır.
5.9 `requireTabContext=true` iken direct URL isteğinde yalnız mesaj + sadece `Kapat` çıktığı doğrulanır.

---

## 6) Performans / Auto-close / Dirty

### 6.A Analiz / Tasarım
6.1 `maxOpenTabs = 15` (parametre).
6.2 Limit dolunca “Block” + uyarı: “Açık tab sayısı 15 limitine geldi. Lütfen açık tablardan birini kapatınız.”
6.3 `tabAutoCloseMinutes = 10` (parametre).
6.4 `autoCloseWarningSeconds = 30` (parametre).
6.5 Idle: kullanıcı hareket/etkileşim yaptıkça süre çalışmaz; son harekete göre hesaplanır (hareket timer reset).
6.5.1 Idle reset event seti (karar): `pointerdown`, `pointermove`, `keydown`, `wheel`, `scroll`.
6.5.2 Event dinleme şekli (karar): eventler uygulama genelinde tek noktadan dinlenir (tab başına ayrı ayrı değil).
6.6 Auto-close uyarısında kapatılacak tab adı yazılır.
6.6.1 Auto-close kapsamı (karar): Auto-close uyarısı ve kapanış sadece **inactive tab**’lar için çalışır (aktif tab için çalışmaz).
6.7 Uyarıda erteleme için numeric input var (dakika).
6.8 Erteleme min=1, max=`tabAutoCloseMinutes` (10); max üstü yazılamaz.
6.9 Refresh/reload yasak (raporlar/filtreler/yarım kayıtlar uçmamalı).
6.10 “Sayfayı Aç” sadece ilgili tabı aktif eder; refresh yok.
6.11 Merkezi Dirty: tab bazında `isDirty`; form değişince `true` olur; save standardı gelince başarılı kaydetmede `false` resetlenecek.
6.11.1 Dirty kapsamı (karar): Şimdilik sadece **record** ekranlarında değişiklik takibi yapılır.
6.11.2 Dirty tetikleme (minimum kural): record ekranı içinde herhangi bir form elemanında (input/select/textarea) değişiklik olursa `isDirty=true` olur.
6.11.3 Grid paging/sorting gibi UI aksiyonları bu revizyonda dirty sayılmaz (ileride ihtiyaç olursa ayrıca ele alınır).
6.12 Auto-close buton seti:
- `isDirty == true` ⇒ `Kapatmayı Ertele`(+dakika input), `Kaydetmeden Kapat`, `Sayfayı Aç`
- `isDirty == false` ⇒ `Kapatmayı Ertele`(+dakika input), `Sayfayı Aç`

### 6.B Unit Test
6.13 Idle reset: mouse/keyboard/scroll ile timer resetlendiği doğrulanır.
6.14 Warning’in kapanmadan 30 sn önce geldiği doğrulanır.
6.15 Warning’de tab adının gösterildiği doğrulanır.
6.16 Erteleme input’unun 1..10 aralığına zorlandığı doğrulanır.
6.17 `isDirty=true` iken “Kaydetmeden Kapat” butonunun çıktığı doğrulanır.
6.18 `isDirty=false` iken “Kaydetmeden Kapat” butonunun çıkmadığı doğrulanır.
6.19 “Sayfayı Aç” tıklanınca tabın aktif olup refresh olmadığı doğrulanır.

---

## 7) Parametreler (DB) — Tek JSON Parametresi

### 7.A Analiz / Tasarım
7.0 `Parameter.Description` max uzunluk: `1000`. (DB şeması migration ile alter edilecek.)
7.1 Tek parametre satırı: `Group=UI`, `Key=TabbedOptions`, `ApplicationId=1`, `ParameterDataTypeId=15(Json)`, `Value/Template/Description`.
7.2 JSON formatı: üstte `navigationMode`, altta `tabbed:{...}` ve `fullPage:{...}` blokları.
7.3 AppDbContext seed (HasData): bu parametre satırı `OnModelCreating(ModelBuilder)` içinde `modelBuilder.Entity<Parameter>().HasData(...)` ile eklenecek.

#### 7.3.1 Description (copy-paste)
`#42 UI navigation options JSON. Format: {version:int,navigationMode:'Tabbed|FullPage',tabbed:{maxOpenTabs:int,onMaxTabReached:{behavior:'Block',message:string},enableNestedTabs:bool,requireTabContext:bool,tabAutoCloseMinutes:int,autoCloseWarningSeconds:int,tabTitleUniqueSuffix:{format:string,start:int}},fullPage:{defaultLandingRoute:string,openReportsInNewWindow:bool,confirmOnUnsavedChanges:bool,deepLinkEnabled:bool,errorMode:string,enableKeepAlive:bool,sessionTimeoutWarningSeconds:int}}`

#### 7.3.2 Value (JSON copy-paste)
```json
{
  "version": 1,
  "navigationMode": "Tabbed",
  "tabbed": {
    "maxOpenTabs": 15,
    "onMaxTabReached": {
      "behavior": "Block",
      "message": "Maksimum tab limiti doldu. Lütfen bir tab kapatınız."
    },
    "enableNestedTabs": true,
    "requireTabContext": true,
    "tabAutoCloseMinutes": 10,
    "autoCloseWarningSeconds": 30,
    "tabTitleUniqueSuffix": { "format": "_{000}", "start": 1 }
  },
  "fullPage": {
    "defaultLandingRoute": "/Dashboard",
    "openReportsInNewWindow": false,
    "confirmOnUnsavedChanges": true,
    "deepLinkEnabled": true,
    "errorMode": "DefaultErrorPage",
    "enableKeepAlive": true,
    "sessionTimeoutWarningSeconds": 60
  }
}
```

#### 7.3.3 Template (JSON copy-paste)
```json
{
  "version": 1,
  "navigationMode": "Tabbed",
  "tabbed": {
    "maxOpenTabs": 15,
    "onMaxTabReached": {
      "behavior": "Block",
      "message": "Maksimum tab limiti doldu. Lütfen bir tab kapatınız."
    },
    "enableNestedTabs": false,
    "requireTabContext": true,
    "tabAutoCloseMinutes": 10,
    "autoCloseWarningSeconds": 30,
    "tabTitleUniqueSuffix": { "format": "_{000}", "start": 1 }
  },
  "fullPage": {
    "defaultLandingRoute": "/Dashboard",
    "openReportsInNewWindow": false,
    "confirmOnUnsavedChanges": true,
    "deepLinkEnabled": true,
    "errorMode": "DefaultErrorPage",
    "enableKeepAlive": true,
    "sessionTimeoutWarningSeconds": 60
  }
}
```

### 7.B Unit Test
7.4 DB’de `Group+Key+ApplicationId` unique kuralı doğrulanır.
7.5 JSON parse edilebilirliği (valid JSON) doğrulanır.
7.6 `navigationMode` değişince doğru bloğun kullanıldığı doğrulanır.

---

## 8) Yapılacak İşler (İş Sırası — Yapılış Sırası)

8.1 Tab Host Shell + Navigation Intercept (Github Issue No: #47) ==> tamamlandı.
Kapsadığı kararlar: `2.1`, `2.2`, `2.3`, `2.4`
Unit Test: `2.6`, `2.7`, `2.8`

8.2 Max Tab Limit Enforcement + Uyarı (Github Issue No: #48) ==> tamamlandı.
Kapsadığı kararlar: `2.5`, `6.1`, `6.2`, `7.2 (tabbed.maxOpenTabs)`, `7.3.2 (onMaxTabReached)`
Unit Test: `2.9`

8.3 Auto-close Timer + Idle Reset + Warning UI (Github Issue No: #49 ) ==> tamamlandı.
Kapsadığı kararlar: `6.3`, `6.4`, `6.5`, `6.6`, `6.7`, `6.8`, `6.10`
Unit Test: `6.13`, `6.14`, `6.15`, `6.16`, `6.19`

8.4 Dirty Merkezi Mimari + Auto-close Aksiyon Seti (Github Issue No: #50 )
Kapsadığı kararlar: `6.11`, `6.12`, `6.9`, `6.10`
Unit Test: `6.17`, `6.18`, `6.19`

8.5 Response Card Standardı + Copy (Github Issue No: #51 )
Kapsadığı kararlar: `4.1`, `4.2`, `4.3`
Unit Test: `4.4`, `4.5`, `4.6`

8.6 requireTabContext Gate (Desktop davranışı) (Github Issue No: #52 )
Kapsadığı kararlar: `5.5`, `5.6`
Unit Test: `5.9`

8.7 DB Parametre Seed + Okuma/Parse (HasData dahil) (Github Issue No: #53 )
Kapsadığı kararlar: `7.0`, `7.1`, `7.2`, `7.3`, `7.3.1`, `7.3.2`, `7.3.3`
'7.0' migration kodu yazmayacağız. EF Core ile Auto-Migration yapılacak. Update-Database komutu ile DB şeması güncellenecek.
Unit Test: `7.4`, `7.5`, `7.6`

8.8 Nested Tabs (Sidebar Hiyerarşisi ile Birebir) (Github Issue No: #54 )
Kapsadığı kararlar: `3.1`, `3.2`
Unit Test: `3.3`, `3.4`, `3.5`

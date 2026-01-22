# #57 — Parametre ve Timeout Yönetimi (DB Driven)

(Revize 4: 22.01.2026)

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
   - `ApplicationId=1` ile global default
   - JSON value + template + description ile admin yönetimi
2.3 #42’de seedlenen `UI/TabbedOptions` (Id=3) tab davranışlarını yönetiyor (örn `tabbed.maxOpenTabs`).

---

## 3) Scope, Application Bağı ve Kaynak Önceliği (Kullanıcı Kararı)

### 3.A Scope
3.1 Scope: **Application bazlı**.
3.2 Default değer `ApplicationId=1` ("sistem") satırıdır.

### 3.B Yeni hedef şema: master/detail (ParameterDefinition + ParameterValue)
3.3 Yeni hedef model:
   - `Parameters` (definition/master)
     - Unique: `(Group, Key)`
   - `ParameterApplications` (value/detail)
     - Unique: `(ParameterId, ApplicationId)`
3.4 Fallback kuralı:
   - `ApplicationId = X` için değer yoksa `ApplicationId = 1` satırı geçerlidir.

### 3.C Kaynak önceliği
3.5 Config kaynağı **yalnız DB**.
3.6 DB erişimi yoksa uygulama **startup fail**.

---

## 4) Parametre Envanteri (CSV Yorumu) — Ok Satırları

Kaynak: `docs/Timeout Parametre Listesi - Son.csv`

> Bu bölüm yalnız `Durum=ok` satırlarının DB parametresine taşınacak kısmını kapsar.

### 4.1 UI / TabHost (Frontend)

4.1.1 `UI.SessionTimeoutSeconds` → `sessionTimeoutSeconds`
4.1.2 `UI.SessionWarningSeconds` → `sessionWarningSeconds`
4.1.3 `UI.TabRequestTimeoutMs` → `tabRequestTimeoutMs`

4.1.4 `UI.MaxOpenTabs` → `maxOpenTabs`
- Karar: **Taşınmayacak.**
- Kaynak: `UI/TabbedOptions` (Id=3) içindeki `tabbed.maxOpenTabs`.

### 4.2 Outbound HTTP (Backend)

4.2.1 `HTTP.RetryCount` → `retryCount`
4.2.2 `HTTP.BaseDelayMs` → `baseDelayMs`
4.2.3 `HTTP.TimeoutSeconds` → `timeoutSeconds`

### 4.3 Güvenlik — AttemptLimiter

4.3.1 `Security.AttemptLimiter.Window` → `window`
4.3.2 `Security.AttemptLimiter.MaxAttempts` → `maxAttempts`
4.3.3 `Security.AttemptLimiter.Cooldown` → `cooldownSeconds`

---

## 5) DB Parametre Modeli (Teknik Tasarım)

### 5.A Parametre kayıtları (önerilen)
5.1 Yönetilebilirlik ve risk izolasyonu için **3 ayrı JSON parametresi** kullanılacak:
- `Group=UI`, `Key=TimeoutOptions` (JSON)
- `Group=HTTP`, `Key=HttpPoliciesOptions` (JSON)
- `Group=Security`, `Key=AttemptLimiterOptions` (JSON)

### 5.B JSON şemaları
5.2 UI Timeout JSON (yalnız 3 alan):
- `{ "sessionTimeoutSeconds": 645, "sessionWarningSeconds": 45, "tabRequestTimeoutMs": 30000 }`
5.3 HTTP JSON:
- `{ "retryCount": 2, "baseDelayMs": 250, "timeoutSeconds": 30 }`
5.4 Security JSON:
- `{ "window": 600, "maxAttempts": 5, "cooldownSeconds": 300 }`

### 5.C UI tarafında kesin ayrım
5.5 Tab davranışı (örn `maxOpenTabs`) **UI/TabbedOptions** içinde kalır.
5.6 Timeout davranışı (yalnız 3 alan) **UI/TimeoutOptions** (yeni) içine taşınır.

---

## 6) Parametre Okuma, Cache, Güvenlik

### 6.A Startup davranışı
6.1 Uygulama açılışında kritik parametre setleri DB’den okunup parse edilerek doğrulanır:
- `Security/PasswordPolicy`
- `Security/AttemptLimiterOptions`
- `HTTP/HttpPoliciesOptions`
- `UI/TimeoutOptions`
6.2 Okuma/parse başarısızsa startup fail.

### 6.B Cache
6.3 Okuma modeli: **lazy-load + IMemoryCache**.
6.4 TTL yaklaşımı: **grup bazlı TTL** ve DB parametresi ile yönetilecek.
- `Security`: 30 sn
- `HTTP`: 60 sn
- `UI`: 300 sn

### 6.C Operasyon kararı: Tek sunucu
6.5 Şimdilik **tek sunucu (single instance)**.
6.6 İleride scale-out olursa cache invalidation ayrıca ele alınır.

### 6.D Validasyon
6.7 Validasyon iki katmanlı:
- Admin UI (erken uyarı)
- Backend (security boundary)
6.8 Validasyon tipi: tip kontrolü + range + JSON schema.

---

## 7) UI Tarafı (Razor + JavaScript)

### 7.A Taşıma yöntemi
7.1 Seçilen yöntem: Razor layout içinde server-side JSON enjekte.
- `window.ArchiX.UiOptions` → `UI/TabbedOptions` (mevcut)
- `window.ArchiX.TimeoutOptions` → `UI/TimeoutOptions` (yeni)

### 7.B Etki analizi (kırılmasın diye zorunlu kontrol listesi)
7.2 JS config okuma kuralları:
- `maxOpenTabs` sadece `UiOptions.tabbed.maxOpenTabs`’tan okunacak.
- Timeout alanları sadece `TimeoutOptions`’tan okunacak.

7.3 Etkilenecek dosyalar (minimum):
- `src/ArchiX.Library.Web/wwwroot/js/archix/tabbed/archix-tabhost.js` (asıl kaynak; şu an `TimeoutOptions.maxOpenTabs` gibi yanlış okuma riski var)
- `src/ArchiX.WebHost/wwwroot/js/archix/tabbed/archix-tabhost.js` (build sonrası kopya)
- Razor Layout (ArchiX config enjekte eden dosya)

7.4 Geriye dönük uyumluluk:
- `UI/TimeoutOptions` henüz yoksa JS default değerlerle çalışmaya devam eder.
- `UI/TabbedOptions` içindeki `tabbed.maxOpenTabs` mevcut olduğu için tab limit davranışı kırılmaz.

---

## 8) Audit / Loglama

8.1 Audit/loglama bu iş kapsamında **yapılmayacak**.
8.2 İleride tüm sistem için merkezi loglama tasarlanacak.

---

## 9) Unit Test Planı

9.1 DB parametre okunamazsa uygulama start olmaz.
9.2 JSON parse hatalıysa admin kaydı engellenir; runtime hata net loglanır.
9.3 TabHost config üretimi test edilir (UiOptions + TimeoutOptions alanları var).
9.4 HTTP policy test edilir (retry/timeouts DB’den okunuyor).
9.5 AttemptLimiter test edilir (window/maxAttempts/cooldown DB’den okunuyor).

---

## 10) Yapılacak İşler (Etki Analizine Göre Sıralı ve Kapalı Liste)

### 10.A Şema / Migration (1. öncelik)
10.1 `Parameters` + `ParameterApplications` tablolarını ekle.
10.2 Mevcut `Parameter` verilerini yeni yapıya taşı (data migration).
10.3 Unique index’leri uygula:
- `(Group, Key)`
- `(ParameterId, ApplicationId)`
10.4 Okuma servislerini fallback kuralına göre güncelle (`appId` yoksa `1`).
10.5 Admin parametre ekranını yeni şemaya uyarla (definition + value).
10.6 Mevcut seed’lerin korunması:
- `UI/TabbedOptions` (Id=3)
- `Security/PasswordPolicy`
- `TwoFactor/Options`

### 10.B Yeni parametrelerin eklenmesi (2. öncelik)
10.7 `UI/TimeoutOptions` parametresini ekle (yalnız 3 alan).
10.8 `HTTP/HttpPoliciesOptions` parametresini ekle.
10.9 `Security/AttemptLimiterOptions` parametresini ekle.
10.10 `System/ParameterRefresh` parametresini ekle (UI/HTTP/Security TTL).

### 10.C Kod uyarlamaları (3. öncelik)
10.11 Startup validation: kritik parametreleri okuyup parse et.
10.12 Cache: IMemoryCache + TTL uygula.
10.13 Razor layout: `UiOptions` + `TimeoutOptions` enjekte et.
10.14 TabHost JS: `maxOpenTabs` okumasını `UiOptions.tabbed.maxOpenTabs`’a taşı; timeout alanlarını `TimeoutOptions`’tan oku.
10.15 HTTP tarafı: retry/timeout değerlerini DB’den okut.
10.16 AttemptLimiter: window/maxAttempts/cooldown değerlerini DB’den okut.

### 10.D Test (4. öncelik)
10.17 Seed testleri güncelle (yeni şema + data migration).
10.18 UI config injection testleri ekle.
10.19 HTTP policy testleri ekle.
10.20 AttemptLimiter testleri ekle.

---

## 99) Kapanış

Bu dokümanda açık soru/öneri bırakılmadı. Tasarım, etki analizi ve yapılacak işler listesi kapalı (tam) hale getirilmiştir.

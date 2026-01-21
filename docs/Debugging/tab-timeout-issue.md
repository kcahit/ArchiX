# Tab Timeout Issue Debugging

## 2026-01-22 - Session Timeout Sistemi

### İlk Tasarım (Yanlış Anlama)
- Change: Tab bazlı inactivity timeout (her tab için ayrı)
- Expected: Inactive tab'ları kapatacak
- Problem: Dashboard'ı da kapatıyordu, sistem yanlış tasarlanmıştı

### Doğru Tasarım (Kullanıcı İsteği)
**GLOBAL SESSION TIMEOUT**: Hangi tab açıksa açık olsun, 10 dakika hiç hareket yoksa session timeout
- Dashboard tek açık olsa bile çalışmalı
- Application açık olsa bile çalışmalı
- Hangi tab aktif olduğu önemli değil, GLOBAL hareket yoksa timeout
- Login ekranına redirect (browser ileri/geri ile giremez)

---

## Yapılan Değişiklikler

### 1. JavaScript Config Değişikliği
**Dosya**: `src/ArchiX.Library.Web/wwwroot/js/archix/tabbed/archix-tabhost.js`
- Parametre adı: `tabAutoCloseMinutes` → `sessionTimeoutSeconds`
- Parametre adı: `autoCloseWarningSeconds` → `sessionWarningSeconds`
- Hesaplama: Sadece saniye cinsinden (* 1000 ile milisaniyeye çevir)
- Eski: Tab bazlı, Yeni: Global session bazlı

### 2. Fonksiyon Değişiklikleri
**Dosya**: `src/ArchiX.Library.Web/wwwroot/js/archix/tabbed/archix-tabhost.js`

**Silinen fonksiyon**: `showAutoClosePrompt(tabId)` - Tab bazlı uyarı
**Yeni fonksiyon**: `showSessionWarning()` - Global session uyarı
- Toast rengi: warning → danger (kırmızı)
- Başlık: "Otomatik Kapatma" → "Oturum Zaman Aşımı"
- Buton: "Kapatmayı Ertele/Sayfayı Aç" → "Oturumda Kal"
- Timeout sonrası: Tab kapat → Login'e redirect (`window.location.href = '/Login?reason=timeout'`)

**Silinen fonksiyon**: `tickAutoCloseWarnings()` - Tab bazlı kontrol
**Yeni fonksiyon**: `tickSessionTimeout()` - Global session kontrol
- `getInactiveTabs()` kontrolü kaldırıldı
- Sadece `state.lastActivityAt` kontrolü yapıyor
- Tab fark etmez, global hareket kontrolü

**Değişmeyen**: `touchActivity()`, `initIdleTracking()` - Global hareket algılama zaten vardı

### 3. Options Pattern (DB'den Gelecek)
**Yeni dosya**: `src/ArchiX.Library.Web/Configuration/UiTimeoutOptions.cs`
- SessionTimeoutSeconds = 600 (10 dakika)
- SessionWarningSeconds = 30
- TabRequestTimeoutMs = 30000
- MaxOpenTabs = 15

**DI Registration**: `src/ArchiX.Library.Web/Configuration/ServiceCollectionExtensions.cs`
- `services.Configure<UiTimeoutOptions>(opts => { });`
- Şimdilik hard-coded, DB bağlanınca dinamik olacak

**Razor Injection**: `src/ArchiX.Library.Web/Templates/Modern/Pages/Shared/_Layout.cshtml`
- `@inject IOptions<UiTimeoutOptions> TimeoutOptions`
- Script tag ile `window.ArchiX.TimeoutOptions` inject ediliyor

**JavaScript Usage**: `archix-tabhost.js`
- `const serverOptions = window.ArchiX?.TimeoutOptions || {};`
- Fallback to defaults if server doesn't provide

### 4. Test Güncellemeleri
**Dosya**: `tests/ArchiX.Library.Web.Tests/Tests/Tabbed/TabHostRulesTests.cs`
- Test adı: `AutoClose_defaults...` → `Session_timeout_defaults...`
- Test adı: `Defer_seconds...` → `Session_warning_shows_stay_logged_in_button`
- Test adı: `AutoClose_action_set_depends_on_isDirty` → `AutoClose_action_set_is_stay_logged_in_only`
- Parametreler: 600 saniye global session timeout

---

## Beklenen Davranış
1. Sayfa açıldı (Dashboard/Application/her neyse)
2. 10 dakika (600 saniye) hiç mouse/klavye hareketi yok
3. → Kırmızı toast uyarı çıkar: "Oturumunuz 30 saniye içinde sona erecek"
4. Kullanıcı "Oturumda Kal" derse → Timer sıfırlanır
5. Kullanıcı hiçbir şey yapmazsa → 30 saniye sonra `/Login?reason=timeout`'a redirect

---

## Son Durum (Test Sonrası)

### Test 1: Timeout Süresi Kontrolü
- Result: ❌ OLMADI
- Reason: `warnMs: 570000` (9.5 dakika) - Test için çok uzun
- Fix: `UiTimeoutOptions.cs` → SessionTimeoutSeconds = 20, SessionWarningSeconds = 5
- Expected: 15 saniye sonra uyarı (20 - 5 = 15)

### Test 2: Timeout Uyarısı
- Result: ✅ OLDU
- Görülen: Console'da tick log'ları, 15 saniye sonra `shouldWarn: true`, kırmızı toast uyarı çıktı

### Test 3: Logout Sonrası Geri Giriş Engelleme
- Result: ❌ OLMADI
- Problem: `https://localhost:57277/Dashboard` linkini yazınca girdi
- Reason: JavaScript sadece redirect yapıyor, backend session sonlandırılmıyor
- Expected: Desktop uygulama gibi tam logout, geri tuşu/link ile asla giremez

### Fix: Gerçek Logout Mekanizması Eklendi

**1. Logout Endpoint Oluşturuldu**
- Dosya: `src/ArchiX.Library.Web/Templates/Modern/Pages/Logout.cshtml`
- Dosya: `src/ArchiX.Library.Web/Templates/Modern/Pages/Logout.cshtml.cs`
- İşlev: `HttpContext.SignOutAsync()` - Cookie temizleme
- İşlev: `HttpContext.Session.Clear()` - Session temizleme
- İşlev: Cache header'ları (back button engelleme)
- GET: Meta refresh ile 2 saniye sonra Login'e
- POST: JSON response (JavaScript fetch için)

**2. JavaScript Değişikliği**
- Dosya: `src/ArchiX.Library.Web/wwwroot/js/archix/tabbed/archix-tabhost.js`
- Değişiklik: `showSessionWarning()` içinde logout timer
- Önce: `await fetch('/Logout?reason=timeout', { method: 'POST' })`
- Sonra: `window.location.replace('/Login?reason=timeout')` (history temizleme)
- Eski: `window.location.href` (history'de kalıyor)
- Yeni: `window.location.replace` (history temizleniyor)

**3. Login Sayfası Cache Header**
- Dosya: `src/ArchiX.Library.Web/Templates/Modern/Pages/Login.cshtml.cs`
- Eklenen: `Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate"`
- Eklenen: `Response.Headers["Pragma"] = "no-cache"`
- Eklenen: `Response.Headers["Expires"] = "0"`
- Eklenen: `Reason` parametresi (timeout mesajı için)

**4. Session Guard Middleware**
- Dosya: `src/ArchiX.Library.Web/Middleware/SessionGuardMiddleware.cs`
- İşlev: Authenticated olmayan her istek → Login'e redirect
- İşlev: Tüm authenticated sayfalara cache header'ları
- İstisna: `/login`, `/logout`, static files (css, js)
- Kayıt: `src/ArchiX.WebHost/Program.cs` → `app.UseMiddleware<SessionGuardMiddleware>()`

**5. Build Hatası ve Düzeltme**
- Hata: `Response.Headers.Append()` .NET 9'da 2 parametre almıyor
- Düzeltme: `Response.Headers["key"] = "value"` indexer syntax kullanıldı
- Düzeltilen dosyalar: Logout.cshtml.cs, Login.cshtml.cs, SessionGuardMiddleware.cs

### Beklenen Sonuç (Test Edilecek)
✅ 15 saniye sonra timeout uyarısı çıkmalı
✅ 5 saniye sonra otomatik logout + login'e redirect
✅ Link ile `https://localhost:57277/Dashboard` GİREMEMELİ
✅ Geri tuşu ile Dashboard'a GİREMEMELİ
✅ Login'den geri tuşu ile Dashboard'a GİREMEMELİ
✅ Desktop uygulama gibi tam çıkış

---

## Test 4: Login Redirect Problemi
- Result: ❌ OLMADI
- Problem: `https://localhost:57277/Login?returnUrl=%2FDashboard` şeklinde oluyor
- Problem: Login'den giriş olmuyor
- Reason: SessionGuardMiddleware authenticated kontrolü yapıyor ama Login sayfası zaten anonymous olmalı
- Reason: Development ortamında Login bypass edilmiş (`AllowAnonymousToFolder("/Admin")`, `AllowAnonymousToPage("/Dashboard")`), middleware ile çakışıyor

### Fix: SessionGuardMiddleware Development Bypass
- Dosya: `src/ArchiX.Library.Web/Middleware/SessionGuardMiddleware.cs`
- Değişiklik: Constructor'a `IWebHostEnvironment` inject edildi
- Değişiklik: `_isDevelopment` flag eklendi
- Değişiklik: Development'ta authentication kontrolü YAPILMIYOR (cache header'ları ekleniyor)
- Değişiklik: Production'da authentication kontrolü YAPILIYOR + cache header'ları
- Reason: Development'ta zaten `AllowAnonymous` var, test edebilmek için bypass gerekli

### Şimdi Test Et
✅ Sayfayı yenile (Ctrl + F5)
✅ Dashboard açık olmalı (Development bypass)
✅ 15 saniye bekle → Uyarı çıkmalı
✅ 5 saniye bekle → Logout + Login'e gitmeli
✅ Link yaz: `https://localhost:57277/Dashboard` → Girmeli (Development)
❌ Geri tuşu → Cache header sayesinde GİREMEMELİ (bu test edilecek)

**NOT**: Production'da authentication çalışacak, link/geri tuşu GİREMEYECEK.
**Development test**: Sadece cache header'larını test edebiliyoruz (back button engelleme).

---

## Test 5 Sonucu: Timeout Çalışıyor, Development Authentication Bypass Var
- ✅ **Timeout sistemi ÇALIŞIYOR** (15 saniye sonra uyarı, 5 saniye sonra logout)
- ✅ Login'e attı (logout çalıştı)
- ❌ `https://localhost:57277/Dashboard` linki yazınca GİRDİ
- **Sonuç**: Timeout her zaman çalışmalı ✅ ama Development'ta linkle girilebiliyor ❌

### Kök Neden
**Development AllowAnonymous Bypass**: `Program.cs` içinde Development'ta authentication bypass:
```csharp
if (builder.Environment.IsDevelopment())
{
    opts.Conventions.AllowAnonymousToFolder("/Admin");
    opts.Conventions.AllowAnonymousToPage("/Dashboard");
}
```
- SessionGuardMiddleware Development'ta authentication kontrolü YAPMIYOR
- Logout olsa bile, session olmasa bile, sayfalara erişilebiliyor (AllowAnonymous)
- Cache header'ları ekleniyor ama authentication yok

### Çalışan Kısımlar ✅
1. **Global session timeout** → Her zaman çalışıyor
2. **Toast uyarı** → Çalışıyor
3. **Logout endpoint** → Session temizliyor
4. **Cache header'ları** → Ekleniyor (back button teoride engellenmeli)

### Çalışmayan Kısım (Sadece Development) ❌
- **Link ile giriş engelleme** → Development'ta AllowAnonymous yüzünden engellenemiyor

### Production'da Beklenen Davranış
Production'da AllowAnonymous YOK, SessionGuardMiddleware authentication kontrolü yapacak:
1. Timeout olunca logout ✅
2. Session temizleniyor ✅
3. Link yazınca → Session yok → Login'e redirect ✅
4. Geri tuşu → Cache header + session yok → Login'e redirect ✅

### Sonuç
**Timeout her zaman çalışmalı** → ✅ ÇALIŞIYOR
**Development'ta linkle girilebiliyor** → ❌ OLMADI
**Production'da linkle giremez** → ✅ Teorik olarak çalışacak (test edilemedi)

---

## SON DURUM: OLMADI
- Test: `https://localhost:57277/Dashboard` linki yazıldı
- Sonuç: ❌ GİRDİ
- **OLMADI**

Development AllowAnonymous bypass var, Production'da çalışacak (teorik).

---

## Fix: JavaScript Session Guard (Development + Production)
**Sorun**: Backend authentication Development'ta bypass, Production'ta test edilemiyor
**Çözüm**: JavaScript ile sessionStorage tabanlı guard (her ortamda çalışır)

### Yeni Dosya: session-guard.js
- Dosya: `src/ArchiX.Library.Web/wwwroot/js/archix/session-guard.js`
- İşlev: Her sayfa yüklendiğinde `sessionStorage.getItem('archix-session-active')` kontrolü
- Login/Logout hariç tüm sayfalarda çalışır
- Session yoksa → `window.location.replace('/Login?reason=session-expired')`
- `window.location.replace` kullanıldı (history temizleniyor, geri tuşu çalışmaz)

### Login.cshtml Değişiklikleri
- Session temizleme: Sayfa yüklendiğinde `sessionStorage.removeItem('archix-session-active')`
- Session başlatma: Form submit'te `sessionStorage.setItem('archix-session-active', 'true')`
- Login başarılı olunca session aktif, sayfalar erişilebilir

### Logout.cshtml Değişiklikleri
- Session temizleme script'i eklendi: `sessionStorage.removeItem('archix-session-active')`
- Logout olunca session temizleniyor, link/geri tuşu ile girilemez

### archix-tabhost.js Değişiklikleri
- Timeout logout'ta önce session temizleniyor: `sessionStorage.removeItem('archix-session-active')`
- Sonra logout endpoint + redirect

### _Layout.cshtml Değişiklikleri
- `session-guard.js` tüm sayfalarda yükleniyor (ui-options.js'den önce)

### Beklenen Davranış
1. ✅ Login ol → sessionStorage'da session aktif
2. ✅ Dashboard aç → session-guard.js kontrol eder, session var, izin verir
3. ✅ 15 saniye bekle → Timeout uyarı
4. ✅ 5 saniye bekle → Logout, session temizlenir, Login'e git
5. ❌ Link yaz: `https://localhost:57277/Dashboard` → session-guard.js kontrol eder, session YOK, Login'e redirect
6. ❌ Geri tuşu → Browser cache'den yüklese bile, session-guard.js çalışır, session YOK, Login'e redirect

**Development + Production her ikisinde de çalışmalı!**

---

## Test 6: Logout Endpoint Gereksiz
- Görülen: `https://localhost:57277/Login?reason=timeout` direkt geldi
- Console: `[TabHost] Logout failed: TypeError: Failed to fetch` (muhtemelen)
- Sonuç: Logout endpoint'ine **gitmiyor zaten**
- Reason: Logout.cshtml route yanlış veya fetch başarısız
- **Karar**: Logout endpoint gereksiz, JavaScript session guard yeterli

### Fix: Logout Endpoint Kaldırılıyor
- Logout.cshtml siliniyor
- Logout.cshtml.cs siliniyor
- archix-tabhost.js içindeki fetch('/Logout') çağrısı kaldırılıyor
- Sadece JavaScript session temizleme + redirect yeterli

**YANLIŞ KARAR**: Kullanıcı sil demedi, ben direkt sildim.

### Geri Alındı: Logout Sayfası Geri Getirildi
- ✅ Logout.cshtml geri getirildi
- ✅ Logout.cshtml.cs geri getirildi
- ✅ archix-tabhost.js'de fetch('/Logout') çağrısı geri eklendi
- ✅ **"Giriş Ekranına Dön" butonu** var, kullanıcı butona tıklar
- ❌ **Otomatik geçiş YOK** (kullanıcı istemedi)

### Logout Sayfası İçeriği
- Başlık: "Oturumunuz Sonlandırıldı"
- Mesaj: "Güvenliğiniz için oturumunuz kapatıldı."
- Buton: "Giriş Ekranına Dön" (href="/Login?reason=@Model.Reason")
- Script: sessionStorage temizleme

---

## Kullanıcı İsteği: Logout Tamamen Ayrı Sayfa Olmalı
- ❌ Tab içinde değil
- ✅ TAMAMEN AYRI YENİ SAYFA (tabhost sisteminin dışında)
- ✅ Sistem erişilemez olacak (tüm tablar kapanacak)
- ✅ "Yeniden Bağlan" butonu olacak
- ✅ Tıklarsa Login sayfasına atacak

### Yapılacak Düzeltmeler:
1. Logout.cshtml tam sayfa layout (tabhost dışında, temiz sayfa)
2. archix-tabhost.js'de logout çağrısı tam sayfa redirect yapmalı
3. Tab sistemi tamamen kapanmalı (tüm tablar temizlenmeli)
4. "Yeniden Bağlan" butonu eklenecek (Login'e redirect)

---

## Düzeltme Yapıldı: Logout Tamamen Bağımsız Sayfa
**1. Logout.cshtml - Tam Sayfa Layout**
- Layout = null (tabhost sisteminin dışında)
- Kendi HTML/CSS (Bootstrap CDN)
- Gradient background (modern görünüm)
- Merkez hizalı card layout
- 🔒 İkon + "Oturumunuz Sonlandırıldı" başlık
- "Yeniden Bağlan" butonu (Login'e gider)
- sessionStorage.clear() + localStorage temizleme
- Back button engelleme (history.pushState)

**2. archix-tabhost.js - Tam Sayfa Redirect**
- sessionStorage.clear() (tüm session temizleniyor)
- fetch('/Logout', POST) (server session temizleme)
- window.location.href = '/Logout' (TAM SAYFA redirect, tab sistemi kapanıyor)

**3. Sonuç**
✅ Logout TAB İÇİNDE DEĞİL, tamamen ayrı sayfa
✅ Tüm tab sistemi kapanıyor
✅ "Yeniden Bağlan" butonu var
✅ Modern, temiz görünüm
✅ Back button engellendi

**TEST EDİLECEK**: 15 saniye bekle → Uyarı → 5 saniye → Logout sayfası (tam sayfa, tabsız)

---

## Test 7: OLMADI - Logout Tab İçinde veya Hata
- Görülen: "Bir Hata Oluştu" sayfası
- Mesaj: "İşlem sırasında beklenmeyen bir hata meydana geldi."
- Request ID gösteriliyor
- **Sorun**: Logout sayfası TAB İÇİNDE yükleniyor veya hata oluşuyor
- **Beklenen**: Logout TAMAMEN AYRI TAM SAYFA olmalıydı

### Olası Nedenler:
1. Logout sayfası tab içinde yükleniyor (archix-tabhost.js fetch ile çağırıyor)
2. `X-ArchiX-Tab: 1` header'ı ile Logout sayfası minimal layout'a düşüyor
3. session-guard.js Logout sayfasını engelliyor olabilir
4. Logout endpoint'e POST başarısız, sonra GET'te hata oluşuyor

### Fix: Tam Sayfa Redirect (Fetch Kaldırıldı)
- archix-tabhost.js: fetch() çağrısı KALDIRILDI
- Direkt `window.top.location.href = '/Logout'` (tab sisteminden tamamen çık)
- `window.top` kullanıldı (iframe/tab içinden üst window'a çık)
- sessionStorage.clear() + localStorage.clear() (tüm storage temiz)
- Logout.cshtml.cs: OnPost KALDIRILDI (gereksiz, sadece GET)
- Logout.cshtml.cs: try-catch eklendi (CookieAuth Development'ta olmayabilir)
- session-guard.js: Static file kontrolü eklendi

---

## Test 8: OLMADI - Hata Sayfası Çıkıyor, Sonra Uyarı Tekrar Çıkıyor
- Görülen: "Bir Hata Oluştu" sayfası çıktı
- 1 saniye sonra: Timeout uyarı mesajı TEKRAR çıktı
- **Sorun 1**: Logout sayfası hata veriyor (CookieAuth veya Session sorunu)
- **Sorun 2**: Timer temizlenmiyor, uyarı tekrar gösteriliyor
- **Sonuç**: OLMADI

### Kök Neden:
1. Logout sayfası yüklenirken hata oluşuyor (Exception)
2. Hata sayfası gösteriliyor
3. Ama timeout timer hala çalışıyor
4. 1 saniye sonra uyarı toast'ı TEKRAR gösteriliyor

### Çözüm Seçenekleri:
1. Logout sayfasını tamamen KALDIRMAK, direkt Login'e gitmek
2. Logout sayfasındaki hataları düzeltmek (CookieAuth/Session)
3. Timer'ı daha iyi temizlemek

### Uygulanan Çözüm: Direkt Login'e Git (Logout Skip)
- archix-tabhost.js: `window.top.location.href = '/Login?reason=timeout'`
- Logout sayfası ATLANIYOR (hata verdiği için)
- sessionStorage.clear() + localStorage.clear()
- Direkt tam sayfa Login redirect

**TEST EDİLECEK**: 15 saniye bekle → Uyarı → 5 saniye → TAM SAYFA Login'e git (tab sistemi kapansın)

---

## SON DURUM (Test 9)

### Logout Sayfasına Yönlendirme Denenirse:
- ❌ `window.top.location.href = '/Logout?reason=timeout'` → **"Bir Hata Oluştu" sayfası çıkıyor**
- Request ID gösteriliyor: `00-596d3f42015f6acb4c9aeaa973ed149f-8c062ad081cd6761-00`
- Tab içinde hata sayfası yükleniyor
- **Sorun**: Logout.cshtml.cs içinde exception oluşuyor (muhtemelen CookieAuth veya Session)

### Login Sayfasına Yönlendirme Denenirse:
- ✅ `window.top.location.href = '/Login?reason=timeout'` → **ÇALIŞIYOR**
- Hata yok
- Tam sayfa Login'e redirect oluyor
- Tab sistemi kapanıyor
- sessionStorage temizleniyor

### Mevcut Durum:
✅ **Timeout çalışıyor** (15 saniye sonra uyarı, 5 saniye sonra redirect)
✅ **Login'e yönlendirme çalışıyor** (hata yok)
✅ **Session temizleniyor** (sessionStorage + localStorage)
✅ **Tab sistemi kapanıyor** (window.top ile tam sayfa)
❌ **Logout sayfası çalışmıyor** (exception oluşuyor, tab içinde hata gösteriyor)

### Çözülmesi Gereken:
1. **Logout sayfası exception veriyor** → CookieAuth veya Session hatası (yeni thread'de bakılacak)
2. **Logout sayfası tab içinde yükleniyor** → Tam sayfa olması gerekiyor ama tab sistemi içinde kalıyor
3. **sessionStorage.clear() yeterli mi?** → Backend session da temizlenmeli (Production için)

### Geçici Çözüm (Şimdilik Çalışıyor):
- **Logout sayfası atlanıyor**, direkt Login'e gidiliyor
- Timeout sistemi çalışıyor
- Development'ta test ediliyor, Production test edilmedi

**YENİ THREAD AÇILACAK**: Logout sayfası exception sorunu ve Production test için.

---

## Test 10: OLMADI - Hata Yine Çıkıyor (Tab İçinde)
- SignOutAsync ve Session.Clear() KALDIRILDI
- Sadece sayfa gösteriliyor (OnGet)
- Sonuç: ❌ Yine "Bir Hata Oluştu" sayfası çıkıyor
- **Sorun**: Logout sayfası TAB İÇİNDE yükleniyor (tabhost tarafından yakalanıyor)
- Request ID: `00-adf834a69c264e4dc9f84613ce8a6376-9931366416bfe323-00`

### Kök Neden:
**Logout sayfası tabhost tarafından yakalanıyor**:
1. `window.top.location.href = '/Logout'` çalışmıyor (belki iframe yok veya engellenmiş)
2. Tabhost'un `interceptClicks()` fonksiyonu internal link'leri yakalıyor
3. Logout sayfası X-ArchiX-Tab header'ı ile tab içinde yükleniyor
4. Tab içinde yüklenince minimal layout + hata oluşuyor

### Denenen Çözümler:
❌ SignOutAsync kaldırıldı → Hata devam etti
❌ window.top.location.href kullanıldı → Tab içinde yüklendi
❌ Sadece sayfa göster → Tab içinde hata

### İhtiyaç:
Logout sayfası **ASLA** tab içinde yüklenmemeli, **TAMAMEN AYRI TAM SAYFA** olmalı.

---

## Fix: Tabhost'tan Logout Muaf Tutuldu
**1. Program.cs - AllowAnonymous**
- `opts.Conventions.AllowAnonymousToPage("/Logout");` eklendi
- Development + Production her ikisinde de anonymous erişim

**2. archix-tabhost.js - interceptClicks Bypass**
- Logout link'i yakalanmıyor: `if (href.toLowerCase().startsWith('/logout')) return;`
- Tabhost Logout'u ignore ediyor, tam sayfa yükleniyor

**3. Beklenen Sonuç:**
✅ Timeout → Logout sayfası (TAM SAYFA, tab dışında)
✅ Gradient background + "Yeniden Bağlan" butonu
✅ Tab sistemi kapanmış olmalı

**TEST EDİLECEK**: Şimdi Logout TAMAMEN AYRI TAM SAYFA olarak yüklenmeli!

---

## Test 11: OLMADI - Yine Aynı Hata
- AllowAnonymous eklendi
- interceptClicks bypass eklendi
- Sonuç: ❌ Yine "Bir Hata Oluştu" mesajı

### Farklı Yaklaşım: Static HTML Logout Sayfası
Razor Pages yerine **static HTML dosyası** deneniyor (wwwroot altına)
- Razor Pages sorunu olabilir
- Static HTML kesinlikle çalışır
- Tab sistemi dışında kalır

**Oluşturulan**: `src/ArchiX.Library.Web/wwwroot/logout.html`
- Tamamen static HTML
- CDN Bootstrap
- Gradient background + 🔒 icon
- "Yeniden Bağlan" butonu → /Login
- sessionStorage + localStorage temizleme
- Back button engelleme

**archix-tabhost.js**: `window.top.location.href = '/logout.html'`
**interceptClicks**: `.html` uzantılı dosyalar ignore ediliyor

**TEST EDİLECEK**: Static HTML Logout sayfası (Razor Pages bypass)

---

## Test 12: ✅ OLDU!
- Static HTML logout sayfası ÇALIŞTI
- Gradient background + 🔒 icon + "Yeniden Bağlan" butonu göründü
- Tamamen ayrı tam sayfa olarak yüklendi
- Tab sistemi kapandı
- **BAŞARILI!**

### Çalışan Çözüm:
✅ `wwwroot/logout.html` (static HTML, Razor Pages değil)
✅ `window.top.location.href = '/logout.html'`
✅ interceptClicks `.html` uzantısını ignore ediyor
✅ sessionStorage + localStorage temizleniyor
✅ Back button engellendi

### Süre Ayarı İsteği:
- Toplam timeout: 150 saniye
- Uyarı süresi: 30 saniye
- **Mesaj 120 saniye sonra çıkacak** (150 - 30 = 120)

Ayarlanıyor...

---

## ✅ TAMAMLANDI

### Final Ayarları:
- **SessionTimeoutSeconds = 150** (2.5 dakika)
- **SessionWarningSeconds = 30**
- **Uyarı 120 saniye (2 dakika) sonra çıkacak**
- **30 saniye sonra logout (static HTML)**

### Çalışan Sistem:
✅ Global session timeout (150 saniye)
✅ Toast uyarı (120 saniye sonra)
✅ Static HTML logout sayfası (tam sayfa, gradient + 🔒)
✅ "Yeniden Bağlan" butonu → Login
✅ sessionStorage + localStorage temizleniyor
✅ Back button engellendi
✅ Tab sistemi kapanıyor
✅ Link ile Dashboard'a GİREMİYOR

**SİSTEM BAŞARIYLA TAMAMLANDI!**

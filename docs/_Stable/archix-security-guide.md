# ArchiX Security Guide

## Kimlik Doğrulama
- Cookie auth varsayılan: `AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie()`
  - `LoginPath = /Account/Login`, `AccessDeniedPath = /Account/Denied`, `SlidingExpiration = true`.
- `UseAuthentication()` pipeline’da `UseAuthorization()` öncesi çağrılır.

## Uygulama Bağlamı / ApplicationId
- `UseArchiX` middleware host → ApplicationId eşlemesini yapar (`ArchiXOptions.HostApplicationMapping`, fallback `DefaultApplicationId`).
- `UseApplicationContext` middleware HttpContext’ten ApplicationId + kullanıcı bilgilerini `IApplicationContext` içine taşır.

## System Application Koruması
- AppDbContext SaveChanges içinde: Application Id=1 delete/disable engeli.
- ApplicationId=1 seed verileri (system) korunur; soft delete yok.

## Authorization
- Policies henüz tanımlı değil; `AddAuthorization()` hazır.
- İhtiyaç halinde: rol/policy ekleyip Razor Pages’da `[Authorize(Policy="...")]` kullan.

## Anti-forgery / CORS
- Razor Pages’da antiforgery otomatik; AJAX için token eklenmesi gerekebilir.
- CORS tanımlı değil; API eklenirse appsettings’ten allowed origins ile `AddCors`/`UseCors` ekle.

## En İyi Uygulama Önerileri
- Prod’da HTTPS zorunlu, cookie `SecurePolicy=Always`, `SameSite=Lax/Strict` gereksinime göre.
- Connection strings’i secrets/KeyVault üzerinden ver; repo içinde düz metin saklama.
- Logging seviyesini prod’da `Error` tut; PII loglama.

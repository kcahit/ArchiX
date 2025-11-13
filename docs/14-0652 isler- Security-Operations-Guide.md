# ArchiX Güvenlik Rehberi (Connection Policy + Login Güvenliği)

Bu doküman C-1..C-6 kapsamında yapılan güvenlik özelliklerinin kullanım ve operasyon notlarını özetler.

1) Kapsam (Tamamlananlar)
- Connection Policy (C-1..C-4)
  - Şifreleme bayrakları: Encrypt, TrustServerCertificate, Integrated Security.
  - Whitelist: AllowedHosts / AllowedCidrs (+ DB `ConnectionServerWhitelist` birleştirmesi).
  - Audit: `ConnectionAudit` tablosuna kayıt, korelasyon ve masking.
  - Parser: IPv6 “[addr]:port”, “host\instance”, port virgül ve iki nokta ayrımları.
- Login Güvenliği (C-5)
  - Deneme sınırlayıcı: `IAttemptLimiter` (IMemoryCache tabanlı).
  - DI uzantısı: `AddAttemptLimiter(IConfiguration)`.
- Uygulama entegrasyonu (C-6)
  - Program.cs bütünleşik kayıtlar.
  - appsettings.* içinde `ArchiX:LoginSecurity:AttemptLimiter` ayarları.

2) Yapılandırma
- Connection Policy (mevcut)
  - appsettings.json / Production.json:
    - ArchiX:ConnectionPolicy: Mode, AllowedHosts, AllowedCidrs, RequireEncrypt, ForbidTrustServerCertificate, AllowIntegratedSecurity
- Login Security (AttemptLimiter)
  - ArchiX:LoginSecurity:AttemptLimiter:
    - MaxAttempts: int (varsayılan 5)
    - Window: TimeSpan (varsayılan 00:05:00)
    - Cooldown: TimeSpan (varsayılan 00:05:00 dev, 00:10:00 prod örneği)

3) DI Kayıtları (Program.cs)
- ConnectionPolicy: `builder.Services.AddConnectionPolicyEvaluator();`
- AttemptLimiter: `builder.Services.AddAttemptLimiter(builder.Configuration);`
- İki Faktör (çekirdek, opsiyonel): `builder.Services.AddTwoFactorCore(builder.Configuration, "ArchiX:TwoFactor");`
- JWT (opsiyonel): `builder.Services.AddJwtSecurity(builder.Configuration, "ArchiX:Jwt");`
- Caching: `AddArchiXMemoryCaching();` + `AddArchiXRepositoryCaching();`
- Domain Events: `AddArchiXDomainEvents();`
- Clock: `AddSingleton<IClock, SystemClock>();`
- Ping Adapter + Health: `AddPingAdapterWithHealthCheck(builder.Configuration);`
- Observability: `app.MapArchiXObservability(app.Configuration);`

4) Çalışma Zamanı Davranışı
- Parametre Okuma
  - `ConnectionPolicyProvider` artık `Group="ConnectionPolicy"` altında `Parameters` tablosunu kullanır.
  - Versiyon izleme `(UpdatedAt ?? CreatedAt)` ile yapılır, bellek önbelleği (TTL) ile birlikte.
- Secret Değerler
  - Şifreli saklanması gereken içerikler için `ParameterDataType = Secret` kullanın (uygulama katmanında encrypt/decrypt).

Operasyon Akışları
- Parametre ekleme/güncelleme
  - `Group`, `Key`, `ParameterDataTypeId`, `Value`, `Description` alanlarını doldurun.
  - JSON tiplerinde `Template` örnek şema sağlayabilir.
- Whitelist birleşimi
  - `ConnectionServerWhitelist` aktif kayıtları (EnvScope eşleşmesi varsa) AllowedHosts/Cidrs ile birleştirilir.
- Cache Tazeleme
  - Connection Policy için anlık yenileme: `ConnectionPolicyProvider.ForceRefresh()`.

Yayına Alma (Migrasyon) Notları
- EF Core migration ve DB güncellemesi:
  - VS PMC: __Add-Migration__ → __Update-Database__
  - veya CLI: `dotnet ef migrations add <name>` → `dotnet ef database update`
- Not: `AppDbContext` sağlayıcıya duyarlı default’lar içerir (SQL Server’da Identity/NEWSEQUENTIALID/SYSDATETIMEOFFSET; diğer sağlayıcılarda CLR default’ları).

Testler
- SQL Server ile birim/integration testleri yeşil.
- LocalDB tabanlı test stratejisi:
  - Her testte benzersiz DB oluşturma → `EnsureCreated()` → seed → doğrulama → DB drop.
- Çalıştırma:
  - __Test Explorer__ veya `dotnet test`

Geri Dönüş (Rollback)
- Geri dönüş gereksiniminde ilgili migrasyonu geri alabilirsiniz:
  - VS PMC: __Update-Database__ (hedef migration adıyla)
  - CLI: `dotnet ef database update <previous-migration>`
- Not: `ArchiXSetting` artık modelden hariçtir; rollback öncesi tabloyu yeniden oluşturmanız gerekebilir.

Ek Notlar
- Parametre tip yelpazesi (NVARCHAR/Numeric/Temporal/Other) ileride genişletilebilir; yalnızca `ParameterDataType` seed listesine eklemek yeterlidir.
- Performans: Bellek önbelleği (TTL) + `(UpdatedAt ?? CreatedAt)` temelli versiyon kontrolü ile okuma maliyeti minimize edildi.

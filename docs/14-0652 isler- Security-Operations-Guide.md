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
- ConnectionPolicy Evaluate
  - Mode=Off → her şey Allowed.
  - Mode=Warn | Enforce → ihlaller `Warn` veya `Blocked` döner.
  - Whitelist boşsa: `WHITELIST_EMPTY` (Warn/Blocked).
  - Whitelist dışı: `SERVER_NOT_WHITELISTED`.
  - Audit tüm değerlendirmelerde yazılır (maskeli raw cs + korelasyon).
- AttemptLimiter
  - `TryBeginAsync(subjectId)` true → denemeye izin, false → 429 önerisi.
  - Başarılı login’den sonra `ResetAsync(subjectId)` çağırın.
  - `subjectId` önerisi: `username|remoteIp`.

5) Doğrulama (Hızlı Test)
- Ping:
  - GET /ping/status → 200, text/plain
  - GET /ping/status.json → 200, JSON
  - GET /health/ping → 200
- Observability:
  - GET /metrics (konfige bağlı yol) → Prometheus endpoint
- ConnectionPolicy:
  - Örnek: `Server=prod-db-01;Encrypt=True;TrustServerCertificate=False;Integrated Security=False` → Allowed.
  - `TrustServerCertificate=True` → Reason: `TRUST_CERT_FORBIDDEN`.
- AttemptLimiter:
  - Aynı kullanıcı+IP ile MaxAttempts üstü denemede false (uygulamada 429 döndürün).
  - Başarılı login’de Reset sonrası tekrar izin.

6) Operasyon Notları
- UTF-8 encoding kullanın (Türkçe karakterler için): VS kaydet uyarısında “UTF-8” seçin; .editorconfig → `charset = utf-8`.
- IDE stil önerileri (IDE0305 vb.) için __Run Code Cleanup__ veya .editorconfig ile `dotnet_diagnostic.IDE0305.severity = none`.
- DB whitelist güncellemesi sonrası `ConnectionPolicyProvider.ForceRefresh()` çağrısı ya da TTL süresini bekleyin.

7) Sık Sorulanlar
- IPv6 whitelist nasıl? AllowedHosts: “[fe80::1]:1433” veya AllowedCidrs: “fe80::/64”.
- Instance bazlı whitelist? AllowedHosts: “host\instance” (tam eşleşme). Sadece host yazarsanız tüm instance’ları kapsar.
- Whitelist boş olursa? Mode’a göre Warn/Blocked ve Reason=`WHITELIST_EMPTY`.

8) Sonraki Opsiyonel Adımlar (gereksinim doğarsa)
- Dağıtık limiter (Redis).
- Login audit (başarılı/başarısız giriş kaydı).
- Masking parametrelerini konfigüre edilebilir yapmak.
- Ek parser varyantları (örn. tcp:host,port).

Bu doküman üretime alınacak yapı için yeterlidir. İhtiyaç halinde ilgili genişletmeleri ayrı fazda ekleyebiliriz.
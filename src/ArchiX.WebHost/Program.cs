using ArchiX.Library.Abstractions.Time;
using ArchiX.Library.External;
using ArchiX.Library.Infrastructure.Caching;
using ArchiX.Library.Infrastructure.DomainEvents;
using ArchiX.Library.Runtime.ConnectionPolicy;
using ArchiX.Library.Runtime.Observability;
using ArchiX.Library.Time;
using ArchiX.Library.Web;
using ArchiX.Library.Web.Mapping;
using ArchiX.Library.Web.Security;

var builder = WebApplication.CreateBuilder(args);

// 1) Temel web varsayýlanlarý
builder.Services.AddArchiXWebDefaults();

// 2) Mapping profilleri
builder.Services.AddApplicationMappings();

// 3) ConnectionPolicy evaluator
builder.Services.AddConnectionPolicyEvaluator();

// 4) Login deneme sýnýrlayýcý (AttemptLimiter)
builder.Services.AddAttemptLimiter(builder.Configuration);

// 5) Two-Factor çekirdek (opsiyonel: config yoksa default deðerler)
builder.Services.AddTwoFactorCore(builder.Configuration, "ArchiX:TwoFactor");
// Eðer email/sms/authenticator provider eklemek istiyorsan burada generic parametreler ile ekle:
// builder.Services.AddEmailTwoFactor<MyEmailCodeStore, MyEmailSender>();

// 6) JWT security (opsiyonel, config section "ArchiX:Jwt")
builder.Services.AddJwtSecurity(builder.Configuration, "ArchiX:Jwt");

// 7) Caching (in-memory + repository decorator)
builder.Services.AddArchiXMemoryCaching();
builder.Services.AddArchiXRepositoryCaching();
// Opsiyonel redis:
// builder.Services.AddArchiXRedisCaching(builder.Configuration.GetConnectionString("Redis")!, "archix:");

// 8) Domain events
builder.Services.AddArchiXDomainEvents();

// 9) Clock
builder.Services.AddSingleton<IClock, SystemClock>();

// 10) Ping adapter + health check (opsiyonel; konfigürasyondan)
builder.Services.AddPingAdapterWithHealthCheck(builder.Configuration);

// 11) Observability (Prometheus /metrics vs.)
builder.Services.AddHealthChecks();
var app = builder.Build();

// Middleware sýralamasý (isteðe göre ekle)
// app.UseAuthentication();
// app.UseAuthorization();

app.MapGet("/", () => "ArchiX WebHost - OK");

// Ping endpointleri — testlerde beklendiði gibi manuel map
app.MapGet("/ping/status", async (ArchiX.Library.Abstractions.External.IPingAdapter ping, CancellationToken ct) =>
{
    var text = await ping.GetStatusTextAsync(ct);
    return Results.Text(text, "text/plain");
});

app.MapGet("/ping/status.json", async (ArchiX.Library.Abstractions.External.IPingAdapter ping, CancellationToken ct) =>
{
    var model = await ping.GetStatusAsync(ct);
    return Results.Json(model);
});

// Health check (ping için)
app.MapHealthChecks("/health/ping");

// Observability endpoints
app.MapArchiXObservability(app.Configuration);

app.Run();

public partial class Program { }

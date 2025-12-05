using ArchiX.Library.Abstractions.Time;
using ArchiX.Library.Context;
using ArchiX.Library.External;
using ArchiX.Library.Infrastructure.Caching;
using ArchiX.Library.Infrastructure.DomainEvents;
using ArchiX.Library.Infrastructure.Http;                // HttpPolicies
using ArchiX.Library.Runtime.ConnectionPolicy;
using ArchiX.Library.Runtime.Database;                  // AdminProvisionerRunner
using ArchiX.Library.Runtime.Observability;
using ArchiX.Library.Runtime.Security;                  // AddPasswordSecurity + PasswordPolicyStartup
using ArchiX.Library.Services.Security;                 // MaskingService
using ArchiX.Library.Time;
using ArchiX.Library.Web;
using ArchiX.Library.Web.Configuration;
using ArchiX.Library.Web.Mapping;
using ArchiX.Library.Web.Security;
using ArchiX.Library.Web.Security.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;          // ActivatorUtilities
using Microsoft.Extensions.DependencyInjection.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Development ek ayar dosyasý (opsiyonel)
try
{
    var libConfigPath = Path.GetFullPath(
        Path.Combine(builder.Environment.ContentRootPath, "..", "ArchiX.Library", "appsettings.Development.json"));
    builder.Configuration.AddJsonFile(libConfigPath, optional: true, reloadOnChange: false);
}
catch { }

// Yalnýz factory + scoped alias (lifetime çakýþmasýný engeller)
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("ArchiXDb")
        ?? builder.Configuration.GetConnectionString("Default"),
        sql => sql.EnableRetryOnFailure()));

builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext());

// Web defaults
builder.Services.AddArchiXWebDefaults();

// Razor Pages + Dev’de PasswordPolicy anonim
builder.Services.AddRazorPages(opts =>
{
    if (builder.Environment.IsDevelopment())
        opts.Conventions.AllowAnonymousToPage("/Admin/Security/PasswordPolicy");
});

// Mapping / ConnectionPolicy / AttemptLimiter / 2FA / JWT / Cache / DomainEvents / Clock
builder.Services.AddApplicationMappings();
builder.Services.AddConnectionPolicyEvaluator();
builder.Services.AddAttemptLimiter(builder.Configuration);
builder.Services.AddTwoFactorCore(builder.Configuration, "ArchiX:TwoFactor");
builder.Services.AddJwtSecurity(builder.Configuration, "ArchiX:Jwt");
builder.Services.AddArchiXMemoryCaching();
builder.Services.AddArchiXRepositoryCaching();
builder.Services.AddArchiXDomainEvents();
builder.Services.AddSingleton<IClock, SystemClock>();

// HttpPolicies + Ping
builder.Services.AddHttpPolicies(builder.Configuration);
builder.Services.AddPingAdapterWithHealthCheck(builder.Configuration);

// Health checks
builder.Services.AddHealthChecks();

// Masking + PasswordSecurity (provider/hasher/admin)
builder.Services.AddSingleton<ArchiX.Library.Abstractions.Security.IMaskingService, MaskingService>();
builder.Services.AddPasswordSecurity();

// Fallback: Admin servis internal ise reflection ile kaydet (çakýþma varsa dokunma)
var adminImpl = Type.GetType(
    "ArchiX.Library.Runtime.Security.PasswordPolicyAdminService, ArchiX.Library",
    throwOnError: false);

if (adminImpl is not null)
{
    builder.Services.TryAddSingleton(
        typeof(ArchiX.Library.Abstractions.Security.IPasswordPolicyAdminService),
        sp => ActivatorUtilities.CreateInstance(sp, adminImpl));
}

// Authorization policies
builder.Services.AddArchiXPolicies();

var app = builder.Build();

// Provision (opsiyonel)
var forceProvision = string.Equals(
    Environment.GetEnvironmentVariable("ARCHIX_DB_FORCE_PROVISION"),
    "true",
    StringComparison.OrdinalIgnoreCase);

await AdminProvisionerRunner.EnsureDatabaseProvisionedAsync(app.Services, force: forceProvision);

// PK-02 & PK-08: Policy yoksa seed + pepper uyarýsý
await PasswordPolicyStartup.EnsureSeedAndWarningsAsync(app.Services, 1);

// Middleware
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

// Root redirect
app.MapGet("/", (HttpContext ctx) =>
    Results.Redirect("/Admin/Security/PasswordPolicy?applicationId=1"));

// Ping endpoints
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

// Health check / Observability / Razor Pages
app.MapHealthChecks("/health/ping");
app.MapArchiXObservability(app.Configuration);
app.MapRazorPages();

app.Run();

public partial class Program { }
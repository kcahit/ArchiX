using ArchiX.Library.Abstractions.Time;
using ArchiX.Library.Context;
using ArchiX.Library.External;
using ArchiX.Library.Infrastructure.Caching;
using ArchiX.Library.Infrastructure.DomainEvents;
using ArchiX.Library.Infrastructure.Http;
using ArchiX.Library.Runtime.ConnectionPolicy;
using ArchiX.Library.Runtime.Connections;
using ArchiX.Library.Runtime.Database;
using ArchiX.Library.Runtime.Observability;
using ArchiX.Library.Runtime.Reports;
using ArchiX.Library.Runtime.Security;
using ArchiX.Library.Services.Security;
using ArchiX.Library.Time;
using ArchiX.Library.Web;
using ArchiX.Library.Web.Configuration;
using ArchiX.Library.Web.Mapping;
using ArchiX.Library.Web.Security;
using ArchiX.Library.Web.Security.Authorization;
using ArchiX.WebHost.Middleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

var builder = WebApplication.CreateBuilder(args);

try
{
    var libConfigPath = Path.GetFullPath(
        Path.Combine(builder.Environment.ContentRootPath, "..", "ArchiX.Library", "appsettings.Development.json"));
    builder.Configuration.AddJsonFile(libConfigPath, optional: true, reloadOnChange: false);
}
catch { }

builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("ArchiXDb")
        ?? builder.Configuration.GetConnectionString("Default"),
        sql => sql.EnableRetryOnFailure()));

builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext());

builder.Services.AddArchiXWebDefaults();

// ✅ Symbolic Link: Basit Razor Pages yapılandırması
builder.Services.AddRazorPages(opts =>
{
    if (builder.Environment.IsDevelopment())
    {
        opts.Conventions.AllowAnonymousToFolder("/Admin");
        opts.Conventions.AllowAnonymousToFolder("/Templates");
        opts.Conventions.AllowAnonymousToPage("/Login");
        opts.Conventions.AllowAnonymousToPage("/Logout");
        opts.Conventions.AllowAnonymousToPage("/Dashboard");
    }
})
.AddRazorRuntimeCompilation()
.AddApplicationPart(typeof(ArchiX.Library.Web.ViewComponents.DatasetGridViewComponent).Assembly);

builder.Services.AddApplicationMappings();
builder.Services.AddConnectionPolicyEvaluator();
builder.Services.AddArchiXConnections();
builder.Services.AddArchiXReports(); // ✅ Issue #17: Dataset executor + limit guard DI
builder.Services.AddAttemptLimiter(builder.Configuration);
builder.Services.AddTwoFactorCore(builder.Configuration, "ArchiX:TwoFactor");
builder.Services.AddJwtSecurity(builder.Configuration, "ArchiX:Jwt");
builder.Services.AddArchiXMemoryCaching();
builder.Services.AddArchiXRepositoryCaching();
builder.Services.AddArchiXDomainEvents();
builder.Services.AddSingleton<IClock, SystemClock>();

builder.Services.AddHttpPolicies(builder.Configuration);
builder.Services.AddPingAdapterWithHealthCheck(builder.Configuration);

builder.Services.AddHealthChecks();

builder.Services.AddSingleton<ArchiX.Library.Abstractions.Security.IMaskingService, MaskingService>();
builder.Services.AddPasswordSecurity();

var adminImpl = Type.GetType(
    "ArchiX.Library.Runtime.Security.PasswordPolicyAdminService, ArchiX.Library",
    throwOnError: false);

if (adminImpl is not null)
{
    builder.Services.TryAddSingleton(
        typeof(ArchiX.Library.Abstractions.Security.IPasswordPolicyAdminService),
        sp => ActivatorUtilities.CreateInstance(sp, adminImpl));
}

builder.Services.AddArchiXPolicies();

var app = builder.Build();

// ✅ EXCEPTION HANDLING
app.UseExceptionHandler("/Error");

// Also handle non-exception HTTP status codes (404 etc.) through the same Razor Error page.
app.UseStatusCodePagesWithReExecute("/Error", "?statusCode={0}");

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

var forceProvision = string.Equals(
    Environment.GetEnvironmentVariable("ARCHIX_DB_FORCE_PROVISION"),
    "true",
    StringComparison.OrdinalIgnoreCase);

await AdminProvisionerRunner.EnsureDatabaseProvisionedAsync(app.Services, force: forceProvision);
await PasswordPolicyStartup.EnsureSeedAndWarningsAsync(app.Services, 1);
await ArchiX.Library.Runtime.Connections.ConnectionStringsStartup.EnsureSeedAsync(app.Services);

// ✅ Static Files (Symbolic link sayesinde css/ erişilebilir)
app.UseStaticFiles();

// #52 requireTabContext gate (UX)
app.UseMiddleware<TabbedContextGateMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

// ✅ Root Redirect to Login
app.MapGet("/", (HttpContext ctx) =>
    Results.Redirect("/Login"));

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

app.MapHealthChecks("/health/ping");
app.MapArchiXObservability(app.Configuration);
app.MapRazorPages();

app.Run();

public partial class Program { }
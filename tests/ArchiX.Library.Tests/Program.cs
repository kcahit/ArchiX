using ArchiX.Library.Abstractions.Persistence;     // IUnitOfWork
using ArchiX.Library.Config;
using ArchiX.Library.Context;
using ArchiX.Library.Entities;
using ArchiX.Library.External;
using ArchiX.Library.Infrastructure.Caching;
using ArchiX.Library.Infrastructure.DomainEvents;
using ArchiX.Library.Infrastructure.EfCore;
using ArchiX.Library.Infrastructure.Http;
using ArchiX.Library.Runtime.Observability;

using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// ArchiX config köprüleme
ShouldRunDbOps.Initialize(builder.Configuration);

// DbContext
builder.Services.AddDbContext<AppDbContext>((sp, opt) =>
{
    var cs = builder.Configuration.GetConnectionString("ArchiXDb")
             ?? throw new InvalidOperationException("ConnectionStrings:ArchiXDb bulunamadı.");
    opt.UseSqlServer(cs)
       .EnableDetailedErrors()
       .EnableSensitiveDataLogging()
        .LogTo(Console.WriteLine, LogLevel.Information);

    // 🔑 Interceptor’ı bağla
    var interceptor = sp.GetRequiredService<DbCommandMetricsInterceptor>();
    opt.AddInterceptors(interceptor);
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddArchiXDomainEvents();
builder.Services.AddArchiXCacheKeyPolicy();
builder.Services.AddHttpPolicies(builder.Configuration);

// UoW DI kaydı (scoped)
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// HealthChecks
var hc = builder.Services.AddHealthChecks();
if (builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Testing"))
    hc.AddCheck("external_ping", () => HealthCheckResult.Healthy("fake"));
else
    builder.Services.AddPingAdapterWithHealthCheck(builder.Configuration, "ExternalServices:Ping", "external_ping");

// OpenTelemetry kayıtları
ObservabilityServiceCollectionExtensions
    .AddArchiXObservability(builder.Services, builder.Configuration, builder.Environment);

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// /metrics sadece Observability açıkken
var obsEnabled = builder.Configuration.GetValue<bool>("Observability:Enabled");
var metricsEnabled = builder.Configuration.GetValue<bool>("Observability:Metrics:Enabled");
if (obsEnabled && metricsEnabled)
{
    ObservabilityEndpointRouteBuilderExtensions
        .MapArchiXObservability(app, builder.Configuration);
}

// Middleware (tekilleştirilmiş)
app.UseMiddleware<CorrelationMiddleware>();
app.UseMiddleware<LoggingScopeMiddleware>();
app.UseMiddleware<RequestMetricsMiddleware>();

app.UseRouting();

app.MapControllers();
app.MapHealthChecks("/healthz");
app.MapHealthChecks("/health/ping", new HealthCheckOptions { Predicate = r => r.Name == "external_ping" });

// ------------------------------------------------------------------------
var allowDbOps = ShouldRunDbOps.IsEnabled();

if (allowDbOps)
{
    using var __dbInitAct =
        ArchiXTelemetry.Activity.StartActivity("DbInit");

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var cnn = (SqlConnection)db.Database.GetDbConnection();

    app.Logger.LogInformation("[ArchiX] Using SQL Server = {DataSource} | DB = {Database}", cnn.DataSource, cnn.Database);
    __dbInitAct?.SetTag("db.server", cnn.DataSource);
    __dbInitAct?.SetTag("db.name", cnn.Database);

    var hasAnyMigrations = db.Database.GetMigrations().Any();
    app.Logger.LogInformation("[ArchiX] HasAnyMigrations: {HasAny}", hasAnyMigrations);
    __dbInitAct?.SetTag("db.migrations.present", hasAnyMigrations);

    if (hasAnyMigrations)
        await db.Database.MigrateAsync();
    else
        await db.Database.EnsureCreatedAsync();

    try
    {
        using var __seedAct =
            ArchiXTelemetry.Activity.StartActivity("DbSeed");

        var preS = await db.Set<Statu>().CountAsync();
        var preF = await db.Set<FilterItem>().IgnoreQueryFilters().CountAsync();
        var preL = await db.Set<LanguagePack>().IgnoreQueryFilters().CountAsync();
        app.Logger.LogInformation("[ArchiX] BEFORE Seed -> Status={S}, FilterItems={F}, LanguagePacks={L}", preS, preF, preL);

        await db.EnsureCoreSeedsAndBindAsync();

        var postS = await db.Set<Statu>().CountAsync();
        var postF = await db.Set<FilterItem>().IgnoreQueryFilters().CountAsync();
        var postL = await db.Set<LanguagePack>().IgnoreQueryFilters().CountAsync();
        app.Logger.LogInformation("[ArchiX] AFTER Seed  -> Status={S}, FilterItems={F}, LanguagePacks={L}", postS, postF, postL);

        using var __testOpsAct =
            ArchiXTelemetry.Activity.StartActivity("DbTestOps");

        var testCode = "___TEST___:" + Guid.NewGuid().ToString("N");
        var testEntity = new Statu { Code = testCode, Name = testCode, Description = "diag" };

        db.Add(testEntity);
        var ins = await db.SaveChangesAsync();
        app.Logger.LogInformation("[ArchiX] TEST INSERT SaveChanges affected: {Count}", ins);

        db.Remove(testEntity);
        var del = await db.SaveChangesAsync();
        app.Logger.LogInformation("[ArchiX] TEST DELETE SaveChanges affected: {Count}", del);
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "[ArchiX Seed/Diag ERROR]");
        var errorMetric = app.Services.GetRequiredService<ErrorMetric>();
        errorMetric.Record("startup", ex.GetType().Name);
        throw;
    }
}
else
{
    app.Logger.LogInformation("[ArchiX] DB işlemleri bu ortamda devre dışı bırakıldı.");
}
// ------------------------------------------------------------------------

await app.RunAsync();

public partial class Program { }

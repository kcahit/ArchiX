// File: tests/ArchiXTest.ApiWeb/Program.cs 
using ArchiX.Library.Config;
using ArchiX.Library.Context;
using ArchiX.Library.Entities;
using ArchiX.Library.External;
using ArchiX.Library.Infrastructure.Caching;
using ArchiX.Library.Infrastructure.DomainEvents;
using ArchiX.Library.Infrastructure.Http;
using ArchiX.Library.Runtime.Observability;

using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddDbContext<AppDbContext>(opt =>
{
    var cs = builder.Configuration.GetConnectionString("ArchiXDb")
             ?? throw new InvalidOperationException("ConnectionStrings:ArchiXDb bulunamadı.");
    opt.UseSqlServer(cs)
       .EnableDetailedErrors()
       .EnableSensitiveDataLogging()
       .LogTo(Console.WriteLine, LogLevel.Information);
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddArchiXDomainEvents();
builder.Services.AddArchiXCacheKeyPolicy();
builder.Services.AddHttpPolicies(builder.Configuration);

// HealthChecks servisini ekle
var hc = builder.Services.AddHealthChecks();

// Development / Testing ortamında sahte health check
if (builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Testing"))
{
    hc.AddCheck("external_ping", () => HealthCheckResult.Healthy("fake"));
}
else
{
    builder.Services.AddPingAdapterWithHealthCheck(builder.Configuration, "ExternalServices:Ping", "external_ping");
}

// 6,04: OpenTelemetry entegrasyonu
builder.Services.AddArchiXObservability(builder.Configuration, builder.Environment);

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 6,04: Prometheus scraping vb.
app.UseArchiXObservability(builder.Configuration);

app.UseArchiXCorrelation();
app.UseRouting();
app.UseMiddleware<LoggingScopeMiddleware>();
app.UseMiddleware<RequestMetricsMiddleware>();

app.MapControllers();
app.MapHealthChecks("/healthz");
app.MapHealthChecks("/health/ping", new HealthCheckOptions { Predicate = r => r.Name == "external_ping" });

// ------------------------------------------------------------------------
// DB işlemlerinin çalıştırılıp çalıştırılmayacağı kontrol ediliyor
var allowDbOps = ShouldRunDbOps.Evaluate(app.Configuration, app.Environment);

if (allowDbOps)
{
    using var __dbInitAct = ArchiXTelemetry.Activity.StartActivity("DbInit");

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
        using var __seedAct = ArchiXTelemetry.Activity.StartActivity("DbSeed");

        var preS = await db.Set<Statu>().CountAsync();
        var preF = await db.Set<FilterItem>().IgnoreQueryFilters().CountAsync();
        var preL = await db.Set<LanguagePack>().IgnoreQueryFilters().CountAsync();
        app.Logger.LogInformation("[ArchiX] BEFORE Seed -> Status={S}, FilterItems={F}, LanguagePacks={L}", preS, preF, preL);

        await db.EnsureCoreSeedsAndBindAsync();

        var postS = await db.Set<Statu>().CountAsync();
        var postF = await db.Set<FilterItem>().IgnoreQueryFilters().CountAsync();
        var postL = await db.Set<LanguagePack>().IgnoreQueryFilters().CountAsync();
        app.Logger.LogInformation("[ArchiX] AFTER Seed  -> Status={S}, FilterItems={F}, LanguagePacks={L}", postS, postF, postL);

        using var __testOpsAct = ArchiXTelemetry.Activity.StartActivity("DbTestOps");

        // Duplicate key'i önlemek için benzersiz test kodu
        var testCode = "___TEST___:" + Guid.NewGuid().ToString("N");
        var testEntity = new Statu { Code = testCode, Name = testCode, Description = "diag" };

        db.Add(testEntity);
        var ins = await db.SaveChangesAsync();
        app.Logger.LogInformation("[ArchiX] TEST INSERT SaveChanges affected: {Count}", ins);
        __testOpsAct?.SetTag("insert.affected", ins);

        db.Remove(testEntity);
        var del = await db.SaveChangesAsync();
        app.Logger.LogInformation("[ArchiX] TEST DELETE SaveChanges affected: {Count}", del);
        __testOpsAct?.SetTag("delete.affected", del);
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "[ArchiX Seed/Diag ERROR]");
        ErrorMetric.Record(area: "startup", exceptionName: ex.GetType().Name);
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

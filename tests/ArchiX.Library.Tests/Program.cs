extern alias archixlib;

using archixlib::ArchiX.Library.Config;
using archixlib::ArchiX.Library.Context;
using archixlib::ArchiX.Library.Entities;
using archixlib::ArchiX.Library.External;
using archixlib::ArchiX.Library.Infrastructure.Caching;
using archixlib::ArchiX.Library.Infrastructure.DomainEvents;
using archixlib::ArchiX.Library.Infrastructure.Http;

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

// HealthChecks
var hc = builder.Services.AddHealthChecks();
if (builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Testing"))
    hc.AddCheck("external_ping", () => HealthCheckResult.Healthy("fake"));
else
    builder.Services.AddPingAdapterWithHealthCheck(builder.Configuration, "ExternalServices:Ping", "external_ping");

// OpenTelemetry kayıtları — UZANTI DEĞİL, STATİK ÇAĞRI
ArchiX.Library.Runtime.Observability.ObservabilityServiceCollectionExtensions
    .AddArchiXObservability(builder.Services, builder.Configuration, builder.Environment);

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// /metrics — UZANTI DEĞİL, STATİK ÇAĞRI
ArchiX.Library.Runtime.Observability.ObservabilityEndpointRouteBuilderExtensions
    .MapArchiXObservability(app, builder.Configuration);

// Middleware türleri tam adla
app.UseMiddleware<ArchiX.Library.Infrastructure.Http.LoggingScopeMiddleware>();
app.UseMiddleware<ArchiX.Library.Infrastructure.Http.RequestMetricsMiddleware>();

app.UseRouting();

app.MapControllers();
app.MapHealthChecks("/healthz");
app.MapHealthChecks("/health/ping", new HealthCheckOptions { Predicate = r => r.Name == "external_ping" });

// ------------------------------------------------------------------------
var allowDbOps = ShouldRunDbOps.IsEnabled();

if (allowDbOps)
{
    using var __dbInitAct =
        ArchiX.Library.Runtime.Observability.ArchiXTelemetry.Activity.StartActivity("DbInit");

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
            ArchiX.Library.Runtime.Observability.ArchiXTelemetry.Activity.StartActivity("DbSeed");

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
            ArchiX.Library.Runtime.Observability.ArchiXTelemetry.Activity.StartActivity("DbTestOps");

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
        ArchiX.Library.Runtime.Observability.ErrorMetric.Record(area: "startup", exceptionName: ex.GetType().Name);
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

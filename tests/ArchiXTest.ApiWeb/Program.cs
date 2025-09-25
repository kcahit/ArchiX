// File: tests/ArchiXTest.ApiWeb/Program.cs  (DB init testte kapatıldı)
using ArchiX.Library.Context;
using ArchiX.Library.Entities;
using ArchiX.Library.External;
using ArchiX.Library.Infrastructure.Caching;
using ArchiX.Library.Infrastructure.DomainEvents;
using ArchiX.Library.Infrastructure.Http;

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
       .LogTo(Console.WriteLine, LogLevel.Information); // EF kendi logunu konsola yazabilir, kalsın
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddArchiXDomainEvents();
builder.Services.AddArchiXCacheKeyPolicy();

// HTTP client politikaları
{
    var apiBase = builder.Configuration["ExternalServices:DemoApi:BaseAddress"] ?? "https://example.invalid/";
    var timeoutSec = builder.Configuration.GetValue<int?>("ExternalServices:DemoApi:TimeoutSeconds") ?? 30;
    var retryCount = builder.Configuration.GetValue<int?>("ExternalServices:DemoApi:RetryCount") ?? 3;
    var baseDelayMs = builder.Configuration.GetValue<int?>("ExternalServices:DemoApi:BaseDelayMs") ?? 200;

    builder.Services.AddHttpClientWrapperWithPolicies<DefaultHttpClientWrapper>(
        c => { c.BaseAddress = new Uri(apiBase); c.Timeout = TimeSpan.FromSeconds(timeoutSec); },
        maxRetries: retryCount,
        baseDelay: TimeSpan.FromMilliseconds(baseDelayMs),
        timeout: TimeSpan.FromSeconds(timeoutSec)
    );
}

// HealthChecks servisini ekle
var hc = builder.Services.AddHealthChecks();

// Testing dışı ortamda gerçek adapter ve health check
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddPingAdapterWithHealthCheck(builder.Configuration, "ExternalServices:DemoApi", "external_ping");
}
else
{
    // Testing’de predicate boşa düşmesin diye sahte health check
    hc.AddCheck("external_ping", () => HealthCheckResult.Healthy("fake"));
}

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseArchiXCorrelation();
app.UseRouting();
app.MapControllers();

app.MapHealthChecks("/healthz");
app.MapHealthChecks("/health/ping", new HealthCheckOptions { Predicate = r => r.Name == "external_ping" });

// Testte DB init’i kapat
var skipDbInit = app.Configuration.GetValue<bool>("DisableDbInit") || app.Environment.IsEnvironment("Testing");

if (!skipDbInit)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var cnn = (SqlConnection)db.Database.GetDbConnection();

    app.Logger.LogInformation("[ArchiX] Using SQL Server = {DataSource} | DB = {Database}", cnn.DataSource, cnn.Database);

    var hasAnyMigrations = db.Database.GetMigrations().Any();
    app.Logger.LogInformation("[ArchiX] HasAnyMigrations: {HasAny}", hasAnyMigrations);
    if (hasAnyMigrations)
        await db.Database.MigrateAsync();
    else
        await db.Database.EnsureCreatedAsync();

    try
    {
        var preS = await db.Set<Statu>().CountAsync();
        var preF = await db.Set<FilterItem>().IgnoreQueryFilters().CountAsync();
        var preL = await db.Set<LanguagePack>().IgnoreQueryFilters().CountAsync();
        app.Logger.LogInformation("[ArchiX] BEFORE Seed -> Status={S}, FilterItems={F}, LanguagePacks={L}", preS, preF, preL);

        await db.EnsureCoreSeedsAndBindAsync();

        var postS = await db.Set<Statu>().CountAsync();
        var postF = await db.Set<FilterItem>().IgnoreQueryFilters().CountAsync();
        var postL = await db.Set<LanguagePack>().IgnoreQueryFilters().CountAsync();
        app.Logger.LogInformation("[ArchiX] AFTER Seed  -> Status={S}, FilterItems={F}, LanguagePacks={L}", postS, postF, postL);

        var testEntity = new Statu { Code = "___TEST___", Name = "___TEST___", Description = "diag" };
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
        throw;
    }
}

await app.RunAsync();

public partial class Program { }

// File: tests/ArchiXTest.ApiWeb/Program.cs
using ArchiX.Library.Context;        // AppDbContext
using ArchiX.Library.Entities;       // Statu, FilterItem, LanguagePack
using ArchiX.Library.Infrastructure; // AddArchiXDomainEvents(), AddArchiXCacheKeyPolicy()

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// === DIAGNOSTIC LOGGING ===
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

/// <summary>
/// DbContext kaydı ve detaylı EF SQL log konfigürasyonu.
/// </summary>
builder.Services.AddDbContext<AppDbContext>(opt =>
{
    var cs = builder.Configuration.GetConnectionString("ArchiXDb")
             ?? throw new InvalidOperationException("ConnectionStrings:ArchiXDb bulunamadı.");
    opt.UseSqlServer(cs)
       .EnableDetailedErrors()
       .EnableSensitiveDataLogging()
       .LogTo(Console.WriteLine, LogLevel.Information);
});

/// <summary>
/// MVC + Swagger servisleri.
/// </summary>
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

/// <summary>
/// ArchiX DI kayıtları.
/// </summary>
builder.Services.AddArchiXDomainEvents();   // mevcut
builder.Services.AddArchiXCacheKeyPolicy(); // 4,022 — Cache Key Policy (varsayılan ayarlar)

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.MapControllers();

/// <summary>
/// Şema/Seed ve teşhis akışı.
/// </summary>
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var cnn = (SqlConnection)db.Database.GetDbConnection();

    Console.WriteLine($"[ArchiX] Using SQL Server = {cnn.DataSource} | DB = {cnn.Database}");

    // 1) Şemayı uygula
    var hasAnyMigrations = db.Database.GetMigrations().Any();
    Console.WriteLine($"[ArchiX] HasAnyMigrations: {hasAnyMigrations}");
    if (hasAnyMigrations)
        await db.Database.MigrateAsync();
    else
        await db.Database.EnsureCreatedAsync();

    try
    {
        // 2) Seed öncesi sayımlar
        var preS = await db.Set<Statu>().CountAsync();
        var preF = await db.Set<FilterItem>().IgnoreQueryFilters().CountAsync();
        var preL = await db.Set<LanguagePack>().IgnoreQueryFilters().CountAsync();
        Console.WriteLine($"[ArchiX] BEFORE Seed -> Status={preS}, FilterItems={preF}, LanguagePacks={preL}");

        // 3) Seed çağrısı
        await db.EnsureCoreSeedsAndBindAsync();

        // 4) Seed sonrası sayımlar
        var postS = await db.Set<Statu>().CountAsync(); // Statu'da global filtre yok
        var postF = await db.Set<FilterItem>().IgnoreQueryFilters().CountAsync();
        var postL = await db.Set<LanguagePack>().IgnoreQueryFilters().CountAsync();
        Console.WriteLine($"[ArchiX] AFTER Seed  -> Status={postS}, FilterItems={postF}, LanguagePacks={postL}");

        // 5) Zorlayıcı test insert (pipeline kontrolü)
        var testEntity = new Statu { Code = "___TEST___", Name = "___TEST___", Description = "diag" };
        db.Add(testEntity);
        var ins = await db.SaveChangesAsync();
        Console.WriteLine($"[ArchiX] TEST INSERT SaveChanges affected: {ins}");

        db.Remove(testEntity);
        var del = await db.SaveChangesAsync();
        Console.WriteLine($"[ArchiX] TEST DELETE SaveChanges affected: {del}");
    }
    catch (Exception ex)
    {
        Console.WriteLine("[ArchiX Seed/Diag ERROR] " + ex);
        throw;
    }
}

/// <summary>
/// Basit health endpoint.
/// </summary>
app.MapGet("/health", () => Results.Ok("OK"));

await app.RunAsync();

// File: tests/ArchiXTest.ApiWeb/Program.cs
using ArchiX.Library.Context;        // AppDbContext
using ArchiX.Library.Entities;       // Statu
using ArchiX.Library.Infrastructure; // AddArchiXDomainEvents()

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// === DIAGNOSTIC LOGGING ===
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// DbContext + detaylı EF SQL logları
builder.Services.AddDbContext<AppDbContext>(opt =>
{
    var cs = builder.Configuration.GetConnectionString("ArchiXDb")
             ?? throw new InvalidOperationException("ConnectionStrings:ArchiXDb bulunamadı.");
    opt.UseSqlServer(cs)
       .EnableDetailedErrors()
       .EnableSensitiveDataLogging()
       .LogTo(Console.WriteLine, LogLevel.Information);
});

// MVC + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// === Domain Events DI ===
builder.Services.AddArchiXDomainEvents(); // <— eklendi

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.MapControllers();

// ---- ŞEMA + SEED + TEŞHİS ----
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

        // 5) Zorlayıcı test insert (pipeline kontrolü): 1 satır ekle → SaveChanges → sil → SaveChanges
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

// Basit health
app.MapGet("/health", () => Results.Ok("OK"));

await app.RunAsync();

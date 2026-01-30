using ArchiX.Library.Configuration;
using ArchiX.Library.Context;
using ArchiX.Library.Web.Extensions;
using ArchiX.Library.Web.Middleware;
using ArchiX.Library.Web.ViewComponents;
using ArchiX.Library.Web.Configuration;
using ArchiX.Library.Services.Menu;
using ArchiX.Library.Web;
using ArchiX.WebHostDLL.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ArchiX core services
builder.Services.AddArchiX(opts =>
{
    builder.Configuration.GetSection("ArchiX").Bind(opts);
    opts.ArchiXConnectionString = builder.Configuration.GetConnectionString("ArchiXDb") ?? opts.ArchiXConnectionString;
});

// Customer DB
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var conn = builder.Configuration.GetConnectionString("ApplicationDb") ?? string.Empty;
    options.UseSqlServer(conn, sql => sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
});

// Menu service wired to customer DB
builder.Services.AddArchiXMenu<ApplicationDbContext>();

// Health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("ArchiXDb")
    .AddDbContextCheck<ApplicationDbContext>("ApplicationDb");

builder.Services.AddRazorPages()
    .AddApplicationPart(typeof(ArchiX.Library.Web.Pipeline.Mediator).Assembly);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var archixDb = services.GetRequiredService<AppDbContext>();
    await archixDb.Database.MigrateAsync();
    var appDb = services.GetRequiredService<ApplicationDbContext>();
    await appDb.Database.MigrateAsync();
}

app.UseStaticFiles();
app.UseArchiX();
app.UseApplicationContext();

app.UseRouting();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapRazorPages();

app.Run();

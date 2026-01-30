using ArchiX.Library.Context;
using ArchiX.Library.Web.Extensions;
using ArchiX.Library.Web.Middleware;
using ArchiX.WebHostDLL.Data;

using Microsoft.AspNetCore.Authentication.Cookies;
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

// Security: cookie auth + authorization
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Denied";
        options.SlidingExpiration = true;
    });
builder.Services.AddAuthorization();

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

// Static files + cache policy
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        if (app.Environment.IsDevelopment())
        {
            ctx.Context.Response.Headers.CacheControl = "no-store, no-cache, must-revalidate";
        }
        else
        {
            const int seconds = 60 * 60 * 24 * 365; // 1 year
            ctx.Context.Response.Headers.CacheControl = $"public,max-age={seconds},immutable";
        }
    }
});
app.UseArchiX();
app.UseApplicationContext();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapRazorPages();

app.Run();

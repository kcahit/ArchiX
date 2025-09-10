using ArchiX.Library.Context;
using ArchiX.Library.LanguagePacks;
using ArchiX.Library.Logging;

using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// MVC + Controller
builder.Services.AddControllersWithViews();

// EF Core (test i�in InMemory; seed'ler EnsureCreated ile y�klenecek)
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseInMemoryDatabase("ArchiXDb"));

// LanguagePacks (mod�ler DI kayd�)
builder.Services.AddLanguagePacks();

// JsonL logger
builder.Services.Configure<LoggingOptions>(builder.Configuration.GetSection("Logging"));
builder.Services.AddSingleton<JsonlLogWriter>();

// Swagger (u�lar� kolayca denemek i�in)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

// DB'yi aya�a kald�r ve seed verilerini y�klet
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

// Attribute routed API'ler
app.MapControllers();

// MVC (mevcut Home/Index vs.)
app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

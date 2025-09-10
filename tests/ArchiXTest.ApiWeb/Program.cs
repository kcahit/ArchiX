// tests/ArchiXTest.ApiWeb/Program.cs
using ArchiX.Library.Context;
using ArchiX.Library.LanguagePacks;

using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// EF Core (test ortamý: InMemory)
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseInMemoryDatabase("ArchiXDb"));

// Language DI (tek kayýt noktasý)
builder.Services.AddLanguagePacks(); // ILanguageService -> LanguageService

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// (Opsiyonel) HTTPS, statik dosya yoksa gerekmez.
// app.UseHttpsRedirection();

app.UseRouting();
app.UseAuthorization();

app.MapControllers();

// DB'yi ayaða kaldýr
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.Run();

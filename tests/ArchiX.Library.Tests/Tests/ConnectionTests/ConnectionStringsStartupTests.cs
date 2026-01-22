using ArchiX.Library.Context;
using ArchiX.Library.Runtime.Connections;

using Microsoft.EntityFrameworkCore;

using Xunit;

namespace ArchiX.Library.Tests.Tests.ConnectionTests
{
    public class ConnectionStringsStartupTests
    {
        [Fact]
        public async Task EnsureSeed_CreatesConnectionStrings()
        {
            var dbName = $"ConnStrings-{Guid.NewGuid()}";
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(dbName));
            var sp = services.BuildServiceProvider();

            // Environment ayarları
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
            Environment.SetEnvironmentVariable("ARCHIX_DB_ENABLE_CONNECTIONSTRINGS_SEED", "true");

            // Seed çalıştır
            await ConnectionStringsStartup.EnsureSeedAsync(sp);

            // Kontrol et - YENİ bir scope ve context alıyoruz
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var param = await db.Parameters
                .Include(p => p.Applications)
                .FirstOrDefaultAsync(p => p.Group == "ConnectionStrings" && p.Key == "ConnectionStrings");

            Assert.NotNull(param);
            
            var appValue = param!.Applications.FirstOrDefault(a => a.ApplicationId == 1);
            Assert.NotNull(appValue);
            Assert.Contains("Demo", appValue!.Value);
        }

        [Fact]
        public async Task EnsureSeed_SkipsInProduction()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase($"ConnStrings-Prod-{Guid.NewGuid()}"));
            var sp = services.BuildServiceProvider();

            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
            
            await ConnectionStringsStartup.EnsureSeedAsync(sp);

            var db = sp.GetRequiredService<AppDbContext>();
            var count = await db.Parameters.CountAsync(p => p.Group == "ConnectionStrings");
            
            Assert.Equal(0, count); // Production'da seed yapılmamalı
        }
    }
}


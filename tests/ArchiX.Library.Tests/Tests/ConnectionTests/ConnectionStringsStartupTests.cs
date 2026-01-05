using ArchiX.Library.Context;
using ArchiX.Library.Runtime.Connections;

using Microsoft.EntityFrameworkCore;

using Xunit;

namespace ArchiX.Library.Tests.Tests.ConnectionTests;

public sealed class ConnectionStringsStartupTests
{
    [Fact]
    public async Task EnsureSeedAsync_CreatesParameter_WhenEnabledInDevelopment()
    {
        var prevEnv = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        var prevFlag = Environment.GetEnvironmentVariable("ARCHIX_DB_ENABLE_CONNECTIONSTRINGS_SEED");

        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
        Environment.SetEnvironmentVariable("ARCHIX_DB_ENABLE_CONNECTIONSTRINGS_SEED", "true");

        try
        {
            var services = new ServiceCollection();
            services.AddLogging();

            var dbName = Guid.NewGuid().ToString();
            services.AddDbContextFactory<AppDbContext>(o => o.UseInMemoryDatabase(dbName));

            var sp = services.BuildServiceProvider();

            await using (var db = await sp.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContextAsync())
            {
                await db.Database.EnsureCreatedAsync();
            }

            await ConnectionStringsStartup.EnsureSeedAsync(sp);

            await using (var db2 = await sp.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContextAsync())
            {
                var v = await db2.Parameters
                    .Where(p => p.ApplicationId == 1 && p.Group == "ConnectionStrings" && p.Key == "ConnectionStrings")
                    .Select(p => p.Value)
                    .SingleOrDefaultAsync();

                Assert.False(string.IsNullOrWhiteSpace(v));
            }
        }
        finally
        {
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", prevEnv);
            Environment.SetEnvironmentVariable("ARCHIX_DB_ENABLE_CONNECTIONSTRINGS_SEED", prevFlag);
        }
    }

    [Fact]
    public async Task EnsureSeedAsync_DoesNothing_WhenNotEnabled()
    {
        var prevEnv = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        var prevFlag = Environment.GetEnvironmentVariable("ARCHIX_DB_ENABLE_CONNECTIONSTRINGS_SEED");

        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
        Environment.SetEnvironmentVariable("ARCHIX_DB_ENABLE_CONNECTIONSTRINGS_SEED", "false");

        try
        {
            var services = new ServiceCollection();
            services.AddLogging();

            var dbName = Guid.NewGuid().ToString();
            services.AddDbContextFactory<AppDbContext>(o => o.UseInMemoryDatabase(dbName));

            var sp = services.BuildServiceProvider();

            await using (var db = await sp.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContextAsync())
            {
                await db.Database.EnsureCreatedAsync();
            }

            await ConnectionStringsStartup.EnsureSeedAsync(sp);

            await using (var db2 = await sp.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContextAsync())
            {
                var count = await db2.Parameters
                    .CountAsync(p => p.ApplicationId == 1 && p.Group == "ConnectionStrings" && p.Key == "ConnectionStrings");

                Assert.Equal(0, count);
            }
        }
        finally
        {
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", prevEnv);
            Environment.SetEnvironmentVariable("ARCHIX_DB_ENABLE_CONNECTIONSTRINGS_SEED", prevFlag);
        }
    }
}

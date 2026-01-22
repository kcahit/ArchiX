using ArchiX.Library.Abstractions.Security;
using ArchiX.Library.Context;
using ArchiX.Library.Entities;
using ArchiX.Library.Runtime.Security;
using ArchiX.Library.Tests.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace ArchiX.Library.Tests.Tests.SecurityTests
{
    public sealed class PasswordPolicyStartupTests
    {
        private static ServiceProvider GetCreateServices()
        {
            var s = new ServiceCollection();
            s.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));
            s.AddMemoryCache();
            s.AddDbContextFactory<AppDbContext>(o => o.UseInMemoryDatabase($"pp-startup-{Guid.NewGuid()}"));
            s.AddSingleton<IPasswordPolicyProvider>(sp =>
            {
                var cache = sp.GetRequiredService<IMemoryCache>();
                var dbf = sp.GetRequiredService<IDbContextFactory<AppDbContext>>();
                return new PasswordPolicyProvider(cache, dbf);
            });
            return s.BuildServiceProvider();
        }

        [Fact]
        public async Task EnsureSeed_CreatesRecord_WhenMissing()
        {
            var sp = GetCreateServices();
            var dbf = sp.GetRequiredService<IDbContextFactory<AppDbContext>>();

            await PasswordPolicyStartup.EnsureSeedAndWarningsAsync(sp, applicationId: 1);

            await using var db = await dbf.CreateDbContextAsync();
            var param = await db.Parameters
                .Include(p => p.Applications)
                .FirstOrDefaultAsync(x => x.Group == "Security" && x.Key == "PasswordPolicy");

            Assert.NotNull(param);
            
            var appValue = param!.Applications.FirstOrDefault(a => a.ApplicationId == 1);
            Assert.NotNull(appValue);
        }

        [Fact]
        public async Task EnsureSeed_DoesNotDuplicate_WhenRecordExists()
        {
            var sp = GetCreateServices();
            var dbf = sp.GetRequiredService<IDbContextFactory<AppDbContext>>();

            await using (var db = await dbf.CreateDbContextAsync())
            {
                await ParameterTestHelper.SeedPasswordPolicyAsync(db, 1, "{\"version\":1}");
            }

            await PasswordPolicyStartup.EnsureSeedAndWarningsAsync(sp, applicationId: 1);

            await using (var db = await dbf.CreateDbContextAsync())
            {
                var param = await db.Parameters
                    .Include(p => p.Applications)
                    .FirstOrDefaultAsync(p => p.Group == "Security" && p.Key == "PasswordPolicy");

                var count = param?.Applications.Count(a => a.ApplicationId == 1) ?? 0;
                Assert.Equal(1, count);
            }
        }

        [Fact]
        public async Task EnsureSeed_LogsWarning_WhenPepperEnabledButNotSet()
        {
            var sp = GetCreateServices();
            var dbf = sp.GetRequiredService<IDbContextFactory<AppDbContext>>();

            Environment.SetEnvironmentVariable("ARCHIX_PEPPER", null);

            await using (var db = await dbf.CreateDbContextAsync())
            {
                await ParameterTestHelper.SeedPasswordPolicyAsync(db, 1, "{\"version\":1,\"hash\":{\"pepperEnabled\":true}}");
            }

            await PasswordPolicyStartup.EnsureSeedAndWarningsAsync(sp, applicationId: 1);
        }
    }
}

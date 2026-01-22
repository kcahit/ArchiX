using ArchiX.Library.Context;
using ArchiX.Library.Runtime.Security;

using Microsoft.EntityFrameworkCore;

using Xunit;

namespace ArchiX.Library.Tests.Tests.SecurityTests
{
    public class PasswordPolicyMultiAppSeedTests
    {
        [Fact]
        public async Task MultiAppSeed_ShouldCreateForEachApp()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDbContextFactory<AppDbContext>(o => o.UseInMemoryDatabase($"MultiApp-{Guid.NewGuid()}"));
            var sp = services.BuildServiceProvider();

            var factory = sp.GetRequiredService<IDbContextFactory<AppDbContext>>();
            await using var db = await factory.CreateDbContextAsync();
            
            var logger = sp.GetRequiredService<ILogger<PasswordPolicyMultiAppSeedTests>>();
            await PasswordPolicyMultiAppSeed.EnsureForApplicationsAsync(db, logger, new[] { 1, 2 });

            var param = await db.Parameters
                .Include(p => p.Applications)
                .FirstOrDefaultAsync(p => p.Group == "Security" && p.Key == "PasswordPolicy");

            Assert.NotNull(param);
            Assert.True(param!.Applications.Count >= 2);
        }
    }
}

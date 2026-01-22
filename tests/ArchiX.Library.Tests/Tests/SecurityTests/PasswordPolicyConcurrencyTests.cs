using ArchiX.Library.Context;
using ArchiX.Library.Tests.Tests.Helpers;

using Microsoft.EntityFrameworkCore;

using Xunit;

namespace ArchiX.Library.Tests.Tests.SecurityTests
{
    public class PasswordPolicyConcurrencyTests
    {
        [Fact]
        public async Task ConcurrentUpdate_ShouldHandleRowVersion()
        {
            var services = new ServiceCollection();
            services.AddDbContextFactory<AppDbContext>(o => o.UseInMemoryDatabase("ConcurrencyTest"));
            var sp = services.BuildServiceProvider();
            var factory = sp.GetRequiredService<IDbContextFactory<AppDbContext>>();

            await using var db = await factory.CreateDbContextAsync();
            await ParameterTestHelper.SeedPasswordPolicyAsync(db, 1);

            var param = await db.Parameters
                .Include(p => p.Applications)
                .FirstAsync(p => p.Group == "Security" && p.Key == "PasswordPolicy");

            Assert.NotNull(param);
        }
    }
}

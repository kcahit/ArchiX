using ArchiX.Library.Context;
using ArchiX.Library.Tests.Tests.Helpers;

using Microsoft.EntityFrameworkCore;

using Xunit;

namespace ArchiX.Library.Tests.Tests.RunTimeTests.Reports
{
    public class ReportDatasetExecutorSentinelTests
    {
        [Fact]
        public async Task SentinelParameter_ShouldExist()
        {
            var services = new ServiceCollection();
            services.AddDbContextFactory<AppDbContext>(o => o.UseInMemoryDatabase("SentinelTest"));
            var sp = services.BuildServiceProvider();
            var factory = sp.GetRequiredService<IDbContextFactory<AppDbContext>>();

            await using var db = await factory.CreateDbContextAsync();
            await ParameterTestHelper.SeedParameterAsync(db, "Reports", "BasePath", 1, "C:\\\\data");

            var param = await db.Parameters
                .Include(p => p.Applications)
                .FirstOrDefaultAsync(p => p.Group == "Reports" && p.Key == "BasePath");

            Assert.NotNull(param);
        }
    }
}

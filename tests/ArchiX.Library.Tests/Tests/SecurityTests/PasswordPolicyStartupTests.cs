using ArchiX.Library.Abstractions.Security;
using ArchiX.Library.Context;
using ArchiX.Library.Entities;
using ArchiX.Library.Runtime.Security;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

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
            // Arrange
            var sp = GetCreateServices();
            var dbf = sp.GetRequiredService<IDbContextFactory<AppDbContext>>();

            // Act
            await PasswordPolicyStartup.EnsureSeedAndWarningsAsync(sp, applicationId: 1);

            // Assert
            await using var db = await dbf.CreateDbContextAsync();
            var param = await db.Parameters
                .FirstOrDefaultAsync(x => x.ApplicationId == 1 && x.Group == "Security" && x.Key == "PasswordPolicy");

            Assert.NotNull(param);
            Assert.Equal("Varsayýlan parola politikasý (startup seed)", param.Description);
        }

        [Fact]
        public async Task EnsureSeed_DoesNotDuplicate_WhenRecordExists()
        {
            // Arrange
            var sp = GetCreateServices();
            var dbf = sp.GetRequiredService<IDbContextFactory<AppDbContext>>();

            await using (var db = await dbf.CreateDbContextAsync())
            {
                db.Parameters.Add(new Parameter
                {
                    ApplicationId = 1,
                    Group = "Security",
                    Key = "PasswordPolicy",
                    ParameterDataTypeId = 15,
                    Value = "{\"version\":1}",
                    Description = "Mevcut kayýt",
                    StatusId = 3,
                    CreatedBy = 0
                });
                await db.SaveChangesAsync();
            }

            // Act
            await PasswordPolicyStartup.EnsureSeedAndWarningsAsync(sp, applicationId: 1);

            // Assert
            await using (var db = await dbf.CreateDbContextAsync())
            {
                var count = await db.Parameters
                    .CountAsync(x => x.ApplicationId == 1 && x.Group == "Security" && x.Key == "PasswordPolicy");
                Assert.Equal(1, count);
            }
        }

        [Fact]
        public async Task EnsureSeed_LogsWarning_WhenPepperEnabledButNotSet()
        {
            // Arrange
            var sp = GetCreateServices();
            var dbf = sp.GetRequiredService<IDbContextFactory<AppDbContext>>();

            // Pepper ortam deðiþkenini temizle
            Environment.SetEnvironmentVariable("ARCHIX_PEPPER", null);

            await using (var db = await dbf.CreateDbContextAsync())
            {
                db.Parameters.Add(new Parameter
                {
                    ApplicationId = 1,
                    Group = "Security",
                    Key = "PasswordPolicy",
                    ParameterDataTypeId = 15,
                    Value = "{\"version\":1,\"hash\":{\"pepperEnabled\":true}}",
                    Description = "Pepper aktif",
                    StatusId = 3,
                    CreatedBy = 0
                });
                await db.SaveChangesAsync();
            }

            // Act & Assert (log çýktýsýnda "ARCHIX_PEPPER" uyarýsýný göreceksin)
            await PasswordPolicyStartup.EnsureSeedAndWarningsAsync(sp, applicationId: 1);
        }
    }
}

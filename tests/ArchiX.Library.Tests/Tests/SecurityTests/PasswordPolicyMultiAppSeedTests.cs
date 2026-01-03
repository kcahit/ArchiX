using ArchiX.Library.Context;
using ArchiX.Library.Runtime.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xunit;

namespace ArchiX.Library.Tests.Tests.SecurityTests
{
    public sealed class PasswordPolicyMultiAppSeedTests
    {
        [Fact]
        public async Task EnsureForApplications_CreatesRecords_ForMultipleApps()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"multi-app-{Guid.NewGuid()}")
                .Options;

            await using var db = new AppDbContext(options);
            var logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger("Test");

            var appIds = new[] { 1, 2, 3 };

            // Act
            await PasswordPolicyMultiAppSeed.EnsureForApplicationsAsync(db, logger, appIds);

            // Assert
            foreach (var appId in appIds)
            {
                var param = await db.Parameters
                    .FirstOrDefaultAsync(x => x.ApplicationId == appId 
                                           && x.Group == "Security" 
                                           && x.Key == "PasswordPolicy");

                Assert.NotNull(param);
                Assert.Contains($"AppId={appId}", param.Description);
            }
        }

        [Fact]
        public async Task EnsureForApplications_DoesNotDuplicate_WhenRecordExists()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"multi-app-dup-{Guid.NewGuid()}")
                .Options;

            await using var db = new AppDbContext(options);
            var logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger("Test");

            // Önce AppId=2 için kayýt ekle
            db.Parameters.Add(new Entities.Parameter
            {
                ApplicationId = 2,
                Group = "Security",
                Key = "PasswordPolicy",
                ParameterDataTypeId = 15,
                Value = "{\"version\":1}",
                Description = "Önceden var",
                StatusId = 3,
                CreatedBy = 0
            });
            await db.SaveChangesAsync();

            var appIds = new[] { 1, 2, 3 };

            // Act
            await PasswordPolicyMultiAppSeed.EnsureForApplicationsAsync(db, logger, appIds);

            // Assert
            var countForApp2 = await db.Parameters
                .CountAsync(x => x.ApplicationId == 2 
                             && x.Group == "Security" 
                             && x.Key == "PasswordPolicy");

            Assert.Equal(1, countForApp2); // Sadece 1 kayýt olmalý, duplicate olmamalý
        }

        [Fact]
        public async Task EnsureForApplications_HandlesEmptyList()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"multi-app-empty-{Guid.NewGuid()}")
                .Options;

            await using var db = new AppDbContext(options);
            var logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger("Test");

            var appIds = Array.Empty<int>();

            // Act
            await PasswordPolicyMultiAppSeed.EnsureForApplicationsAsync(db, logger, appIds);

            // Assert
            var count = await db.Parameters
                .CountAsync(x => x.Group == "Security" && x.Key == "PasswordPolicy");

            Assert.Equal(0, count);
        }
    }
}
using ArchiX.Library.Context;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

using Xunit;
namespace ArchiX.Library.Tests.Tests.PersistenceTests
{
    public class AppDbContextSeedsTests
    {
        private static string BuildConnString(string dbName) => $"Server=(localdb)\\MSSQLLocalDB;Database={dbName};Integrated Security=true;TrustServerCertificate=True;MultipleActiveResultSets=True;";
        private static void CreateDatabase(string dbName)
        {
            var master = $"Server=(localdb)\\MSSQLLocalDB;Database=master;Integrated Security=true;TrustServerCertificate=True;";
            using var conn = new SqlConnection(master);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $"IF DB_ID('{dbName}') IS NULL CREATE DATABASE [{dbName}];";
            cmd.ExecuteNonQuery();
        }

        private static void DropDatabase(string dbName)
        {
            var master = $"Server=(localdb)\\MSSQLLocalDB;Database=master;Integrated Security=true;TrustServerCertificate=True;";
            using var conn = new SqlConnection(master);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $@"
IF DB_ID('{dbName}') IS NOT NULL BEGIN ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{dbName}]; END"; cmd.ExecuteNonQuery();
        }
        private static AppDbContext CreateContext(string connString)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(connString)
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors()
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task EnsureCoreSeedsAndBindAsync_seeds_ParameterDataTypes_and_TwoFactorDefault()
        {
            // LocalDB sadece Windows'ta mevcut; Linux/macOS CI ortamýnda testi atla.
            if (!OperatingSystem.IsWindows())
                return;

            var dbName = $"ArchiX_Tests_{Guid.NewGuid():N}";
            var connString = BuildConnString(dbName);

            CreateDatabase(dbName);
            try
            {
                await using (var db = CreateContext(connString))
                {
                    await db.Database.EnsureCreatedAsync();
                    await db.EnsureCoreSeedsAndBindAsync();

                    var codes = await db.ParameterDataTypes
                        .AsNoTracking()
                        .Select(x => x.Code)
                        .ToListAsync();

                    int[] expected =
                    {
                    60, 70, 80, 90, 100,
                    200, 210, 220, 230, 240,
                    300, 310, 320,
                    900, 910, 920
                };

                    foreach (var c in expected)
                        Assert.Contains(c, codes);

                    var tf = await db.Parameters
                        .Include(p => p.DataType)
                        .AsNoTracking()
                        .SingleOrDefaultAsync(p => p.Group == "TwoFactor" && p.Key == "Options");

                    Assert.NotNull(tf);
                    Assert.Equal("Json", tf!.DataType.Name);
                    Assert.Contains("\"defaultChannel\": \"Sms\"", tf.Value);
                    Assert.NotNull(tf.Template);
                    Assert.Equal(3 /*Approved*/, tf.StatusId);
                }
            }
            finally
            {
                DropDatabase(dbName);
            }
        }
    }
}

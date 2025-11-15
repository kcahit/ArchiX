using ArchiX.Library.Context;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

using Xunit;

namespace ArchiX.Library.Tests.Tests.Persistence
{
    public class AppDbContextSeedsTests
    {
        private static string BuildConnString(string dbName) =>
            $"Server=(localdb)\\MSSQLLocalDB;Database={dbName};Integrated Security=true;TrustServerCertificate=True;MultipleActiveResultSets=True;";

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
IF DB_ID('{dbName}') IS NOT NULL
BEGIN
    ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [{dbName}];
END";
            cmd.ExecuteNonQuery();
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
            var dbName = $"ArchiX_Tests_{Guid.NewGuid():N}";
            var connString = BuildConnString(dbName);

            CreateDatabase(dbName);
            try
            {
                await using (var db = CreateContext(connString))
                {
                    // SQL Server’da EnsureCreated hýzlý ve migrations’a ihtiyaç duymaz
                    await db.Database.EnsureCreatedAsync();

                    // Act
                    await db.EnsureCoreSeedsAndBindAsync();

                    // Assert ParameterDataTypes
                    var codes = await db.ParameterDataTypes
                        .AsNoTracking()
                        .Select(x => x.Code)
                        .ToListAsync();

                    int[] expected =
                    {
                        // NVARCHAR
                        60, 70, 80, 90, 100,
                        // Numeric
                        200, 210, 220, 230, 240,
                        // Temporal
                        300, 310, 320,
                        // Other
                        900, 910, 920
                    };

                    foreach (var c in expected)
                        Assert.Contains(c, codes);

                    // Assert TwoFactor default JSON parameter
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
                // Temizlik
                DropDatabase(dbName);
            }
        }
    }
}
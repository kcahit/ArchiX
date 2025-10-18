using System.Text.Json;

using ArchiX.Library.Runtime.Database.Core;

using Xunit;

namespace ArchiXTest.ApiWeb.Test.RuntimeTests.DatabaseTests
{
    /// <summary>
    /// Create çağrısı sonrası snapshot loglarının yazıldığını doğrular.
    /// <para><c>ArchiX:AllowDbOps=false</c> ise test çalıştırmadan çıkar.</para>
    /// </summary>
    public sealed class ArchiXDatabaseSnapshotTests
    {
        /// <summary>
        /// İzin varsa Create sonrası "Tables.Snapshot" kaydı bulunmalıdır.
        /// </summary>
        [Fact]
        public async Task CreateAsync_ShouldWriteSnapshotEntry()
        {
            if (!AllowDbOps())
                return;

            ArchiXDatabase.Configure("SqlServer");

            await ArchiXDatabase.CreateAsync();

            var logDir = Path.Combine(AppContext.BaseDirectory, "logs");
            var file = Directory.GetFiles(logDir, "ArchiXDB_Log_Create_*.jsonl").Last();

            var lines = await File.ReadAllLinesAsync(file);
            Assert.Contains(lines, l => l.Contains("Tables.Snapshot", StringComparison.Ordinal));

            var snapshotLine = lines.First(l => l.Contains("Tables.Snapshot", StringComparison.Ordinal));
            using var doc = JsonDocument.Parse(snapshotLine);
            Assert.Equal("Tables.Snapshot", doc.RootElement.GetProperty("Stage").GetString());
        }

        private static bool AllowDbOps()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            return config.GetValue("ArchiX:AllowDbOps", false);
        }
    }
}

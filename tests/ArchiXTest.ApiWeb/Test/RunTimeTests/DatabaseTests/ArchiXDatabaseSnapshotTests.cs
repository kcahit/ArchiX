// File: tests/ArchiXTest.ApiWeb/Test/RuntimeTests/DatabaseTests/ArchiXDatabaseSnapshotTests.cs

using System.Text.Json;

using ArchiX.Library.Runtime.Database.Core;

using Xunit;

namespace ArchiXTest.ApiWeb.Test.RuntimeTests.DatabaseTests
{
    /// <summary>
    /// Snapshot loglarının doğru yazıldığını test eder.
    /// </summary>
    public sealed class ArchiXDatabaseSnapshotTests
    {
        [Fact]
        public async Task CreateAsync_ShouldWriteSnapshotEntry()
        {
            // Arrange
            ArchiXDatabase.Configure("SqlServer");

            // Act
            await ArchiXDatabase.CreateAsync();

            // Assert
            var logDir = Path.Combine(System.AppContext.BaseDirectory, "logs");
            var file = Directory.GetFiles(logDir, "ArchiXDB_Log_Create_*.jsonl").Last();

            var lines = await File.ReadAllLinesAsync(file);
            Assert.Contains(lines, l => l.Contains("Tables.Snapshot"));

            // JSON parse kontrol
            var snapshotLine = lines.First(l => l.Contains("Tables.Snapshot"));
            var doc = JsonDocument.Parse(snapshotLine);
            Assert.Equal("Tables.Snapshot", doc.RootElement.GetProperty("Stage").GetString());
        }
    }
}

// File: tests/ArchiXTest.ApiWeb/Test/RuntimeTests/DatabaseTests/ArchiXDatabaseUpdateSnapshotTests.cs

using System.Text.Json;

using ArchiX.Library.Runtime.Database.Core;

using Xunit;

namespace ArchiXTest.ApiWeb.Test.RuntimeTests.DatabaseTests
{
    /// <summary>
    /// Update çağrısı sonrası snapshot loglarının yazıldığını test eder.
    /// </summary>
    public sealed class ArchiXDatabaseUpdateSnapshotTests
    {
        [Fact]
        public async Task UpdateAsync_ShouldWriteSnapshotEntry()
        {
            // Arrange
            ArchiXDatabase.Configure("SqlServer");

            // Act
            await ArchiXDatabase.UpdateAsync();

            // Assert
            var logDir = Path.Combine(System.AppContext.BaseDirectory, "logs");
            var file = Directory.GetFiles(logDir, "ArchiXDB_Log_Update_*.jsonl").Last();

            var lines = await File.ReadAllLinesAsync(file);
            Assert.Contains(lines, l => l.Contains("Tables.Snapshot"));

            var snapshotLine = lines.First(l => l.Contains("Tables.Snapshot"));
            var doc = JsonDocument.Parse(snapshotLine);
            Assert.Equal("Tables.Snapshot", doc.RootElement.GetProperty("Stage").GetString());
        }
    }
}

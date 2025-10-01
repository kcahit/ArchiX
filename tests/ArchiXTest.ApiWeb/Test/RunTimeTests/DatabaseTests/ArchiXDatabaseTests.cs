// File: tests/ArchiXTest.ApiWeb/Test/RunTimeTests/DatabaseTests/ArchiXDatabaseTests.cs

using ArchiX.Library.Runtime.Database.Core;

using Xunit;

namespace ArchiXTest.ApiWeb.Test.RunTimeTests.DatabaseTests
{
    /// <summary>
    /// ArchiXDatabase facade testleri.
    /// Create ve Update işlemlerinin idempotent çalışmasını doğrular.
    /// </summary>
    public sealed class ArchiXDatabaseTests
    {
        [Fact]
        public async Task CreateAsync_ShouldGenerateLogFile()
        {
            // Arrange
            ArchiXDatabase.Configure("SqlServer");

            // Act
            await ArchiXDatabase.CreateAsync();

            // Assert
            var logDir = Path.Combine(System.AppContext.BaseDirectory, "logs");
            var files = Directory.GetFiles(logDir, "ArchiXDB_Log_Create_*.jsonl");
            Assert.NotEmpty(files);
        }

        [Fact]
        public async Task UpdateAsync_ShouldGenerateLogFile()
        {
            // Arrange
            ArchiXDatabase.Configure("SqlServer");

            // Act
            await ArchiXDatabase.UpdateAsync();

            // Assert
            var logDir = Path.Combine(System.AppContext.BaseDirectory, "logs");
            var files = Directory.GetFiles(logDir, "ArchiXDB_Log_Update_*.jsonl");
            Assert.NotEmpty(files);
        }

        [Fact]
        public async Task CreateAsync_ShouldBeIdempotent()
        {
            // Arrange
            ArchiXDatabase.Configure("SqlServer");

            // Act
            await ArchiXDatabase.CreateAsync();
            await ArchiXDatabase.CreateAsync(); // ikinci çalıştırma

            // Assert
            var logDir = Path.Combine(System.AppContext.BaseDirectory, "logs");
            var files = Directory.GetFiles(logDir, "ArchiXDB_Log_Create_*.jsonl");
            Assert.True(files.Length >= 2); // her çağrı log bırakmalı
        }
    }
}

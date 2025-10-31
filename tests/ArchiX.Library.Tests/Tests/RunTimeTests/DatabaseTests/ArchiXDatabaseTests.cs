using ArchiX.Library.Runtime.Database.Core;

using Xunit;

namespace ArchiX.Library.Tests.Tests.RunTimeTests.DatabaseTests
{
    /// <summary>
    /// ArchiXDatabase dış yüzü için temel akış testleri.
    /// <para><c>ArchiX:AllowDbOps=false</c> ise testler DB/log çalıştırmadan çıkar.</para>
    /// </summary>
    public sealed class ArchiXDatabaseTests
    {
        /// <summary>
        /// İzin verildiğinde Create çağrısı log dosyası üretmelidir.
        /// </summary>
        [Fact]
        public async Task CreateAsync_ShouldGenerateLogFile()
        {
            if (!AllowDbOps())
                return;

            ArchiXDatabase.Configure("SqlServer");

            await ArchiXDatabase.CreateAsync();

            var logDir = Path.Combine(AppContext.BaseDirectory, "logs");
            var files = Directory.GetFiles(logDir, "ArchiXDB_Log_Create_*.jsonl");
            Assert.NotEmpty(files);
        }

        /// <summary>
        /// İzin verildiğinde Update çağrısı log dosyası üretmelidir.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_ShouldGenerateLogFile()
        {
            if (!AllowDbOps())
                return;

            ArchiXDatabase.Configure("SqlServer");

            await ArchiXDatabase.UpdateAsync();

            var logDir = Path.Combine(AppContext.BaseDirectory, "logs");
            var files = Directory.GetFiles(logDir, "ArchiXDB_Log_Update_*.jsonl");
            Assert.NotEmpty(files);
        }

        /// <summary>
        /// İzin verildiğinde Create çağrısı idempotent olmalı ve her çağrıda log üretmelidir.
        /// </summary>
        [Fact]
        public async Task CreateAsync_ShouldBeIdempotent()
        {
            if (!AllowDbOps())
                return;

            ArchiXDatabase.Configure("SqlServer");

            await ArchiXDatabase.CreateAsync();
            await ArchiXDatabase.CreateAsync();

            var logDir = Path.Combine(AppContext.BaseDirectory, "logs");
            var files = Directory.GetFiles(logDir, "ArchiXDB_Log_Create_*.jsonl");
            Assert.True(files.Length >= 2);
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

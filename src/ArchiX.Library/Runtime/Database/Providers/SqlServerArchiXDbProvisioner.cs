// File: src/ArchiX.Library/Runtime/Database/Providers/SqlServerArchiXDbProvisioner.cs
using System.Data;

using ArchiX.Library.Context;
using ArchiX.Library.Runtime.Database.Core;
using ArchiX.Library.Runtime.Database.Models;
using ArchiX.Library.Runtime.Reports;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ArchiX.Library.Runtime.Database.Providers
{
    internal sealed class SqlServerArchiXDbProvisioner : ArchiXDbProvisionerBase
    {
        private const string FixedDbName = "_Archix";
        private static string Server => Environment.GetEnvironmentVariable("ARCHIX_DB_SERVER") ?? @"(local)";

        protected override async Task EnsureDatabaseAsync(StreamWriter writer, CancellationToken ct)
        {
            using var conn = new SqlConnection(BuildMasterConnectionString(Server));
            await conn.OpenAsync(ct).ConfigureAwait(false);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $@"IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'{FixedDbName}') BEGIN CREATE DATABASE [{FixedDbName}] COLLATE Latin1_General_100_CI_AS_SC_UTF8; END";
            cmd.CommandType = CommandType.Text;
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await WriteLogAsync(writer, nameof(EnsureDatabaseAsync), new { Server, Database = FixedDbName });
        }

        protected override Task EnsureServerLoginAsync(StreamWriter writer, CancellationToken ct) =>
            WriteLogAsync(writer, nameof(EnsureServerLoginAsync) + ":Skipped", new { Reason = "IntegratedSecurity" });

        protected override Task EnsureDbUserAndRoleAsync(StreamWriter writer, CancellationToken ct) =>
            WriteLogAsync(writer, nameof(EnsureDbUserAndRoleAsync) + ":Skipped", new { Reason = "IntegratedSecurity" });

        protected override async Task ApplyMigrationsAndSeedAsync(StreamWriter writer, CancellationToken ct)
        {
            var cs = BuildDbUserConnectionString(Server, string.Empty);
            await WriteLogAsync(writer, "ApplyMigrationsAndSeed.Start", new { ConnectionPreview = cs.Split(';').FirstOrDefault() });

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(cs, o => o.EnableRetryOnFailure())
                .Options;

            await using var db = new AppDbContext(options);

            var hasAnyMigrations = db.Database.GetMigrations().Any();
            await WriteLogAsync(writer, "ApplyMigrationsAndSeed.HasAnyMigrations", new { HasAnyMigrations = hasAnyMigrations });

            if (hasAnyMigrations)
                await db.Database.MigrateAsync(ct).ConfigureAwait(false);
            else
                await db.Database.EnsureCreatedAsync(ct).ConfigureAwait(false);

            await db.EnsureCoreSeedsAndBindAsync(ct).ConfigureAwait(false);

            // Report dataset master kayıtları (Id identity) - migration'a bağımlı değil.
            await ReportDatasetStartup.EnsureSeedAsync(db, ct).ConfigureAwait(false);

            await WriteLogAsync(writer, "ApplyMigrationsAndSeed.Done");
        }

        protected override Task<IReadOnlyList<TableInfo>> GetTablesAsync(CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<TableInfo>>(new List<TableInfo>());

        protected override Task<IReadOnlyList<string>> LoadColumnsAsync(string table, CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<string>>(new List<string>());

        protected override Task<string?> LoadPrimaryKeyAsync(string table, CancellationToken ct) =>
            Task.FromResult<string?>(null);

        protected override Task<IReadOnlyList<string>> LoadUniqueConstraintsAsync(string table, CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<string>>(new List<string>());

        protected override Task<IReadOnlyList<ForeignKeyInfo>> LoadForeignKeysAsync(string table, CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<ForeignKeyInfo>>(new List<ForeignKeyInfo>());

        protected override Task<IReadOnlyList<IndexInfo>> LoadIndexesAsync(string table, CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<IndexInfo>>(new List<IndexInfo>());

        protected override Task<long> LoadRowCountAsync(string table, CancellationToken ct) =>
            Task.FromResult(0L);

        protected override Task<long> SafeCountAsync(string table, CancellationToken ct) =>
            Task.FromResult(0L);

        protected override string BuildMasterConnectionString(string server) =>
            $"Server={server};Database=master;Integrated Security=True;TrustServerCertificate=True;";

        protected override string BuildDbAdminConnectionString(string server, string archiXPassword) =>
            $"Server={server};Database={FixedDbName};Integrated Security=True;TrustServerCertificate=True;";

        protected override string BuildDbUserConnectionString(string server, string archiXPassword) =>
            $"Server={server};Database={FixedDbName};Integrated Security=True;TrustServerCertificate=True;";
    }
}

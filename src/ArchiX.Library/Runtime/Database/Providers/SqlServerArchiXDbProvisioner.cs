// File: src/ArchiX.Library/Runtime/Database/Providers/SqlServerArchiXDbProvisioner.cs
using System.Data;

using ArchiX.Library.Context;
using ArchiX.Library.Runtime.Database.Core;
using ArchiX.Library.Runtime.Database.Models;

using Microsoft.Data.SqlClient;       // <-- EKLENDİ
using Microsoft.EntityFrameworkCore;

namespace ArchiX.Library.Runtime.Database.Providers
{
    /// <summary>
    /// SQL Server için veritabanı kurulum ve güncelleme sağlayıcısı.
    /// DB adı sabittir: "ArchiX". Parola env/dosya ile okunur (repoya konmaz).
    /// </summary>
    internal sealed class SqlServerArchiXDbProvisioner : ArchiXDbProvisionerBase
    {
        private const string FixedDbName = "ArchiX";
        private static string Server => Environment.GetEnvironmentVariable("ARCHIX_DB_SERVER") ?? "localhost";

        // 1) ARCHIX_DB_ADMIN_PASSWORD -> 2) ARCHIX_DB_ADMIN_PASSWORD_FILE -> 3) D:\_git\ArchiX\Dev\Connection\admin_password.txt -> 4) D:\_git\ArchiX\Dev\GitIIgnore\admin_password.txt
        private static string AdminPassword
        {
            get
            {
                var env = Environment.GetEnvironmentVariable("ARCHIX_DB_ADMIN_PASSWORD");
                if (!string.IsNullOrWhiteSpace(env))
                    return env.Trim();

                var fileEnv = Environment.GetEnvironmentVariable("ARCHIX_DB_ADMIN_PASSWORD_FILE");
                if (!string.IsNullOrWhiteSpace(fileEnv) && File.Exists(fileEnv))
                    return File.ReadAllText(fileEnv).Trim();

                var defaultConnFile = @"D:\_git\ArchiX\Dev\Connection\admin_password.txt";
                if (File.Exists(defaultConnFile))
                    return File.ReadAllText(defaultConnFile).Trim();

                var defaultIgnoredFile = @"D:\_git\ArchiX\Dev\GitIIgnore\admin_password.txt";
                if (File.Exists(defaultIgnoredFile))
                    return File.ReadAllText(defaultIgnoredFile).Trim();

                throw new InvalidOperationException("ARCHIX_DB_ADMIN_PASSWORD veya ARCHIX_DB_ADMIN_PASSWORD_FILE sağlanmalı (parola dosyası repo dışı olmalıdır).");
            }
        }

        protected override async Task EnsureDatabaseAsync(StreamWriter writer, CancellationToken ct)
        {
            using var conn = new SqlConnection(BuildMasterConnectionString(Server));
            await conn.OpenAsync(ct).ConfigureAwait(false);

            using var cmd = conn.CreateCommand();
            cmd.CommandText = $@"
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'{FixedDbName}')
BEGIN
    CREATE DATABASE [{FixedDbName}] COLLATE Latin1_General_100_CI_AS_SC_UTF8;
END";
            cmd.CommandType = CommandType.Text;
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);

            await WriteLogAsync(writer, "EnsureDatabase", new { Server, Database = FixedDbName });
        }

        protected override async Task EnsureServerLoginAsync(StreamWriter writer, CancellationToken ct)
        {
            using var conn = new SqlConnection(BuildMasterConnectionString(Server));
            await conn.OpenAsync(ct).ConfigureAwait(false);

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = N'_ArchiX')
BEGIN
    CREATE LOGIN [_ArchiX] WITH PASSWORD = @pwd, CHECK_POLICY = ON, CHECK_EXPIRATION = OFF;
END";
            cmd.Parameters.Add(new SqlParameter("@pwd", SqlDbType.NVarChar, 128) { Value = AdminPassword });
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);

            await WriteLogAsync(writer, "EnsureServerLogin", new { Login = "_ArchiX" });
        }

        protected override async Task EnsureDbUserAndRoleAsync(StreamWriter writer, CancellationToken ct)
        {
            using var conn = new SqlConnection(BuildMasterConnectionString(Server));
            await conn.OpenAsync(ct).ConfigureAwait(false);

            using var cmd = conn.CreateCommand();
            cmd.CommandText = $@"
USE [{FixedDbName}];
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = N'_ArchiX')
BEGIN
    CREATE USER [_ArchiX] FOR LOGIN [_ArchiX];
    ALTER ROLE db_owner ADD MEMBER [_ArchiX];
END";
            cmd.CommandType = CommandType.Text;
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);

            await WriteLogAsync(writer, "EnsureDbUserAndRole", new { Database = FixedDbName, User = "_ArchiX" });
        }

        protected override async Task ApplyMigrationsAndSeedAsync(StreamWriter writer, CancellationToken ct)
        {
            var cs = BuildDbUserConnectionString(Server, AdminPassword);

            await WriteLogAsync(writer, "ApplyMigrationsAndSeed.Start", new { ConnectionPreview = cs.Split(';').FirstOrDefault() });

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(cs, o => o.EnableRetryOnFailure())
                .Options;

            await using var db = new AppDbContext(options);
            await db.Database.MigrateAsync(ct).ConfigureAwait(false);
            await db.EnsureCoreSeedsAndBindAsync(ct).ConfigureAwait(false);

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
            $"Server={server};Database={FixedDbName};User Id=_ArchiX;Password={archiXPassword};TrustServerCertificate=True;";

        protected override string BuildDbUserConnectionString(string server, string archiXPassword) =>
            $"Server={server};Database={FixedDbName};User Id=_ArchiX;Password={archiXPassword};TrustServerCertificate=True;";
    }
}

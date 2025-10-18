// File: src/ArchiX.Library/Runtime/Database/Providers/SqlServerArchiXDbProvisioner.cs

using ArchiX.Library.Runtime.Database.Core;
using ArchiX.Library.Runtime.Database.Models;

namespace ArchiX.Library.Runtime.Database.Providers
{
    /// <summary>
    /// SQL Server için veritabanı kurulum ve güncelleme sağlayıcısı.
    /// </summary>
    internal sealed class SqlServerArchiXDbProvisioner : ArchiXDbProvisionerBase
    {
        protected override Task EnsureDatabaseAsync(StreamWriter writer, CancellationToken ct)
        {
            // TODO: CREATE DATABASE [_ArchiXDb] kontrol ve kurulum
            return Task.CompletedTask;
        }

        protected override Task EnsureServerLoginAsync(StreamWriter writer, CancellationToken ct)
        {
            // TODO: CREATE LOGIN [_ArchiX] kontrol ve kurulum
            return Task.CompletedTask;
        }

        protected override Task EnsureDbUserAndRoleAsync(StreamWriter writer, CancellationToken ct)
        {
            // TODO: CREATE USER [_ArchiX] ve db_owner rol ataması
            return Task.CompletedTask;
        }

        protected override Task ApplyMigrationsAndSeedAsync(StreamWriter writer, CancellationToken ct)
        {
            // TODO: EF Core Migrate + Seed işlemleri
            return Task.CompletedTask;
        }

        protected override Task<IReadOnlyList<TableInfo>> GetTablesAsync(CancellationToken ct)
        {
            // TODO: sys.tables sorgusu
            return Task.FromResult<IReadOnlyList<TableInfo>>(new List<TableInfo>());
        }

        protected override Task<IReadOnlyList<string>> LoadColumnsAsync(string table, CancellationToken ct)
        {
            // TODO: sys.columns sorgusu
            return Task.FromResult<IReadOnlyList<string>>(new List<string>());
        }

        protected override Task<string?> LoadPrimaryKeyAsync(string table, CancellationToken ct)
        {
            // TODO: PK bilgisi
            return Task.FromResult<string?>(null);
        }

        protected override Task<IReadOnlyList<string>> LoadUniqueConstraintsAsync(string table, CancellationToken ct)
        {
            // TODO: UNIQUE constraint bilgisi
            return Task.FromResult<IReadOnlyList<string>>(new List<string>());
        }

        protected override Task<IReadOnlyList<ForeignKeyInfo>> LoadForeignKeysAsync(string table, CancellationToken ct)
        {
            // TODO: FOREIGN KEY bilgisi
            return Task.FromResult<IReadOnlyList<ForeignKeyInfo>>(new List<ForeignKeyInfo>());
        }

        protected override Task<IReadOnlyList<IndexInfo>> LoadIndexesAsync(string table, CancellationToken ct)
        {
            // TODO: INDEX bilgisi
            return Task.FromResult<IReadOnlyList<IndexInfo>>(new List<IndexInfo>());
        }

        protected override Task<long> LoadRowCountAsync(string table, CancellationToken ct)
        {
            // TODO: SELECT COUNT(*) FROM [table]
            return Task.FromResult(0L);
        }

        protected override Task<long> SafeCountAsync(string table, CancellationToken ct)
        {
            // TODO: TRY CATCH ile güvenli sayım
            return Task.FromResult(0L);
        }

        protected override string BuildMasterConnectionString(string server)
        {
            return $"Server={server};Database=master;Integrated Security=True;TrustServerCertificate=True;";
        }

        protected override string BuildDbAdminConnectionString(string server, string archiXPassword)
        {
            return $"Server={server};Database=_ArchiXDb;User Id=_ArchiX;Password={archiXPassword};TrustServerCertificate=True;";
        }

        protected override string BuildDbUserConnectionString(string server, string archiXPassword)
        {
            return $"Server={server};Database=_ArchiXDb;User Id=_ArchiX;Password={archiXPassword};TrustServerCertificate=True;";
        }
    }
}

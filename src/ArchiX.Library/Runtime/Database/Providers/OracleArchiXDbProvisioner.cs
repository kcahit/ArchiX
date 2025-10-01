// File: src/ArchiX.Library/Runtime/Database/Providers/OracleArchiXDbProvisioner.cs

using ArchiX.Library.Runtime.Database.Core;
using ArchiX.Library.Runtime.Database.Models;

namespace ArchiX.Library.Runtime.Database.Providers
{
    /// <summary>
    /// Oracle için veritabanı kurulum ve güncelleme sağlayıcısı (iskelet).
    /// Henüz implementasyon yoktur.
    /// </summary>
    internal sealed class OracleArchiXDbProvisioner : ArchiXDbProvisionerBase
    {
        protected override Task EnsureDatabaseAsync(StreamWriter writer, CancellationToken ct)
        {
            // TODO: CREATE DATABASE eşdeğeri
            return Task.CompletedTask;
        }

        protected override Task EnsureServerLoginAsync(StreamWriter writer, CancellationToken ct)
        {
            // TODO: CREATE USER / ROLE işlemleri
            return Task.CompletedTask;
        }

        protected override Task EnsureDbUserAndRoleAsync(StreamWriter writer, CancellationToken ct)
        {
            // TODO: Kullanıcıya yetki atamaları
            return Task.CompletedTask;
        }

        protected override Task ApplyMigrationsAndSeedAsync(StreamWriter writer, CancellationToken ct)
        {
            // TODO: EF Core migrate + seed
            return Task.CompletedTask;
        }

        protected override Task<IReadOnlyList<TableInfo>> GetTablesAsync(CancellationToken ct)
        {
            // TODO: USER_TABLES sorgusu
            return Task.FromResult<IReadOnlyList<TableInfo>>(new List<TableInfo>());
        }

        protected override Task<IReadOnlyList<string>> LoadColumnsAsync(string table, CancellationToken ct)
        {
            // TODO: USER_TAB_COLUMNS sorgusu
            return Task.FromResult<IReadOnlyList<string>>(new List<string>());
        }

        protected override Task<string?> LoadPrimaryKeyAsync(string table, CancellationToken ct)
        {
            // TODO: USER_CONSTRAINTS sorgusu
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
            // TODO: SELECT COUNT(*) FROM <table>
            return Task.FromResult(0L);
        }

        protected override Task<long> SafeCountAsync(string table, CancellationToken ct)
        {
            // TODO: Hata yakalamalı sayım
            return Task.FromResult(0L);
        }

        protected override string BuildMasterConnectionString(string server)
        {
            // TODO: Oracle bağlantı formatı
            return $"Data Source={server};User Id=system;Password=***;";
        }

        protected override string BuildDbAdminConnectionString(string server, string archiXPassword)
        {
            // TODO: Oracle bağlantı formatı
            return $"Data Source={server};User Id=_ArchiX;Password={archiXPassword};";
        }

        protected override string BuildDbUserConnectionString(string server, string archiXPassword)
        {
            // TODO: Oracle bağlantı formatı
            return $"Data Source={server};User Id=_ArchiX;Password={archiXPassword};";
        }
    }
}

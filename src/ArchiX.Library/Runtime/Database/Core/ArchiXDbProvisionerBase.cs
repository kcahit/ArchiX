// File: src/ArchiX.Library/Runtime/Database/Core/ArchiXDbProvisionerBase.cs

using System.Text.Json;

namespace ArchiX.Library.Runtime.Database.Core
{
    /// <summary>
    /// Sağlayıcılar için sabit akışı tanımlayan soyut taban sınıf.
    /// Create/Update işlemleri burada loglanır.
    /// </summary>
    internal abstract class ArchiXDbProvisionerBase
    {
        private readonly string _logDir;

        protected ArchiXDbProvisionerBase()
        {
            _logDir = Path.Combine(AppContext.BaseDirectory, "logs");
            Directory.CreateDirectory(_logDir);
        }

        public async Task CreateAsync(CancellationToken ct = default)
        {
            var logFile = GetLogFile("Create");
            await using var writer = CreateLogWriter(logFile);
            await WriteLogAsync(writer, "Start");

            await EnsureDatabaseAsync(writer, ct);
            await EnsureServerLoginAsync(writer, ct);
            await EnsureDbUserAndRoleAsync(writer, ct);
            await ApplyMigrationsAndSeedAsync(writer, ct);

            await SnapshotTablesAsync(writer, ct);

            await WriteLogAsync(writer, "Done");
        }

        public async Task UpdateAsync(CancellationToken ct = default)
        {
            var logFile = GetLogFile("Update");
            await using var writer = CreateLogWriter(logFile);
            await WriteLogAsync(writer, "Start");

            await ApplyMigrationsAndSeedAsync(writer, ct);
            await SnapshotTablesAsync(writer, ct);

            await WriteLogAsync(writer, "Done");
        }

        private async Task SnapshotTablesAsync(StreamWriter writer, CancellationToken ct)
        {
            var tables = await GetTablesAsync(ct);
            foreach (var t in tables)
            {
                t.Columns = await LoadColumnsAsync(t.Name, ct);
                t.PrimaryKey = await LoadPrimaryKeyAsync(t.Name, ct);
                t.UniqueConstraints = await LoadUniqueConstraintsAsync(t.Name, ct);
                t.ForeignKeys = await LoadForeignKeysAsync(t.Name, ct);
                t.Indexes = await LoadIndexesAsync(t.Name, ct);
                t.RowCount = await LoadRowCountAsync(t.Name, ct);
            }
            await WriteLogAsync(writer, "Tables.Snapshot", tables);
        }

        private string GetLogFile(string kind)
        {
            var ts = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff");
            var uniq = Guid.NewGuid().ToString("N")[..6];
            return Path.Combine(_logDir, $"ArchiXDB_Log_{kind}_{ts}_{uniq}.jsonl");
        }

        private static StreamWriter CreateLogWriter(string path)
        {
            var fs = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.Read, 4096, FileOptions.SequentialScan);
            return new StreamWriter(fs);
        }

        protected static async Task WriteLogAsync(StreamWriter writer, string stage, object? data = null)
        {
            var entry = new DbLog(stage, data);
            var json = JsonSerializer.Serialize(entry);
            await writer.WriteLineAsync(json);
            await writer.FlushAsync();
        }

        protected abstract Task EnsureDatabaseAsync(StreamWriter writer, CancellationToken ct);
        protected abstract Task EnsureServerLoginAsync(StreamWriter writer, CancellationToken ct);
        protected abstract Task EnsureDbUserAndRoleAsync(StreamWriter writer, CancellationToken ct);
        protected abstract Task ApplyMigrationsAndSeedAsync(StreamWriter writer, CancellationToken ct);

        protected abstract Task<IReadOnlyList<ArchiX.Library.Runtime.Database.Models.TableInfo>> GetTablesAsync(CancellationToken ct);
        protected abstract Task<IReadOnlyList<string>> LoadColumnsAsync(string table, CancellationToken ct);
        protected abstract Task<string?> LoadPrimaryKeyAsync(string table, CancellationToken ct);
        protected abstract Task<IReadOnlyList<string>> LoadUniqueConstraintsAsync(string table, CancellationToken ct);
        protected abstract Task<IReadOnlyList<ArchiX.Library.Runtime.Database.Models.ForeignKeyInfo>> LoadForeignKeysAsync(string table, CancellationToken ct);
        protected abstract Task<IReadOnlyList<ArchiX.Library.Runtime.Database.Models.IndexInfo>> LoadIndexesAsync(string table, CancellationToken ct);
        protected abstract Task<long> LoadRowCountAsync(string table, CancellationToken ct);
        protected abstract Task<long> SafeCountAsync(string table, CancellationToken ct);

        protected abstract string BuildMasterConnectionString(string server);
        protected abstract string BuildDbAdminConnectionString(string server, string archiXPassword);
        protected abstract string BuildDbUserConnectionString(string server, string archiXPassword);
    }
}

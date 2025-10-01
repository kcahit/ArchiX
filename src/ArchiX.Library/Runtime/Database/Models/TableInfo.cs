// File: src/ArchiX.Library/Runtime/Database/Models/TableInfo.cs


namespace ArchiX.Library.Runtime.Database.Models
{
    /// <summary>
    /// Veritabanındaki tablo bilgisini temsil eder.
    /// Snapshot sırasında kolon, PK/FK/UQ/Index ve satır sayısı ile doldurulur.
    /// </summary>
    internal sealed class TableInfo
    {
        public string Name { get; set; } = string.Empty;

        public IReadOnlyList<string> Columns { get; set; } = new List<string>();

        public string? PrimaryKey { get; set; }

        public IReadOnlyList<string> UniqueConstraints { get; set; } = new List<string>();

        public IReadOnlyList<ForeignKeyInfo> ForeignKeys { get; set; } = new List<ForeignKeyInfo>();

        public IReadOnlyList<IndexInfo> Indexes { get; set; } = new List<IndexInfo>();

        public long RowCount { get; set; }
    }
}

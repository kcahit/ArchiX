// File: src/ArchiX.Library/Runtime/Database/Models/ForeignKeyInfo.cs
#pragma warning disable CS1591

namespace ArchiX.Library.Runtime.Database.Models
{
    /// <summary>
    /// Tablo üzerindeki foreign key bilgisini temsil eder.
    /// </summary>
    internal sealed class ForeignKeyInfo
    {
        public string Name { get; set; } = string.Empty;

        public string Column { get; set; } = string.Empty;

        public string ReferencedTable { get; set; } = string.Empty;

        public string ReferencedColumn { get; set; } = string.Empty;
    }
}

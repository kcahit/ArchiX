// File: src/ArchiX.Library/Runtime/Database/Models/IndexInfo.cs
#pragma warning disable CS1591

namespace ArchiX.Library.Runtime.Database.Models
{
    /// <summary>
    /// Tablo üzerindeki index bilgisini temsil eder.
    /// </summary>
    internal sealed class IndexInfo
    {
        public string Name { get; set; } = string.Empty;

        public IReadOnlyList<string> Columns { get; set; } = new List<string>();

        public bool IsUnique { get; set; }
    }
}

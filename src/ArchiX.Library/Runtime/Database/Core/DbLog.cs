// File: src/ArchiX.Library/Runtime/Database/Core/DbLog.cs
#pragma warning disable CS1591

namespace ArchiX.Library.Runtime.Database.Core
{
    /// <summary>
    /// Veritabanı kurulum/güncelleme işlemleri sırasında log girdisi.
    /// </summary>
    internal sealed class DbLog
    {
        public DbLog(string stage, object? data = null)
        {
            TimestampUtc = DateTime.UtcNow;
            Stage = stage;
            Data = data;
        }

        public DateTime TimestampUtc { get; }

        public string Stage { get; }

        public object? Data { get; }
    }
}

using ArchiX.Library.Interfaces;

namespace ArchiX.Library.Entities
{
    /// <summary>
    /// Ortak alanlar. Zamanlar datetimeoffset(4) ile saklanır.
    /// Örnek: 2025-08-28 10:15:32.1234 +03:00
    /// </summary>
    public abstract class BaseEntity : IEntity
    {
        public int Id { get; set; }
        public Guid RowId { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
        public int CreatedBy { get; set; }

        public DateTimeOffset? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }

        public int StatusId { get; set; }=3; // Default Approved
        public DateTimeOffset? LastStatusAt { get; set; }
        public int LastStatusBy { get; set; }
    }
}

using ArchiX.Library.Interfaces;

namespace ArchiX.Library.Entities
{
    /// <summary>
    /// Tüm entity sınıfları için ortak alanları içeren abstract base sınıf.
    /// Zamanlar <see cref="DateTimeOffset"/> (precision 4) ile saklanır.
    /// Örnek: 2025-08-28 10:15:32.1234 +03:00
    /// </summary>
    public abstract class BaseEntity : IEntity
    {
        /// <summary>
        /// Birincil anahtar kimlik (Identity).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Her satır için benzersiz GUID değeri.
        /// </summary>
        public Guid RowId { get; set; }

        /// <summary>
        /// Kaydın oluşturulduğu tarih/zaman.
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// Kaydı oluşturan kullanıcı kimliği.
        /// </summary>
        public int CreatedBy { get; set; }

        /// <summary>
        /// Kaydın son güncellendiği tarih/zaman.
        /// </summary>
        public DateTimeOffset? UpdatedAt { get; set; }

        /// <summary>
        /// Kaydı son güncelleyen kullanıcı kimliği.
        /// </summary>
        public int? UpdatedBy { get; set; }

        /// <summary>
        /// Kaydın güncel durum kimliği (varsayılan: 3 → Approved).
        /// </summary>
        public int StatusId { get; set; } = 3;

        /// <summary>
        /// Kaydın son durum değişikliğinin tarih/zaman bilgisi.
        /// </summary>
        public DateTimeOffset? LastStatusAt { get; set; }

        /// <summary>
        /// Kaydın son durum değişikliğini yapan kullanıcı kimliği.
        /// </summary>
        public int LastStatusBy { get; set; }
    }
}

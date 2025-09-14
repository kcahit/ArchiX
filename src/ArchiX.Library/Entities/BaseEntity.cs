// File: src/ArchiX.Library/Entities/BaseEntity.cs
namespace ArchiX.Library.Entities
{
    /// <summary>
    /// Tüm entity sınıfları için ortak alanları içeren soyut (abstract) temel sınıf.
    /// Zamanlar <see cref="DateTimeOffset"/> (precision 4) ile saklanır.
    /// </summary>
    public abstract class BaseEntity : IEntity
    {
        /// <summary>
        /// Bu türün veritabanında tabloya haritalanıp haritalanmayacağını belirleyen bayrak.
        /// Türev sınıfta <c>new static readonly bool MapToDb = false;</c> yazarak tablo oluşturulmasını kalıcı olarak kapatabilirsin.
        /// </summary>
        public static readonly bool MapToDb = true;

        /// <summary>Birincil anahtar (Identity).</summary>
        public int Id { get; set; }

        /// <summary>Her satır için benzersiz GUID değeri.</summary>
        public Guid RowId { get; set; }

        /// <summary>Kaydın oluşturulduğu tarih/zaman.</summary>
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>Kaydı oluşturan kullanıcı kimliği.</summary>
        public int CreatedBy { get; set; }

        /// <summary>Kaydın son güncellendiği tarih/zaman.</summary>
        public DateTimeOffset? UpdatedAt { get; set; }

        /// <summary>Kaydı son güncelleyen kullanıcı kimliği.</summary>
        public int? UpdatedBy { get; set; }

        /// <summary>Kaydın güncel durum kimliği (varsayılan: 3 → Approved).</summary>
        public int StatusId { get; set; } = 3;

        /// <summary>Son durum değişikliğinin tarih/zaman bilgisi.</summary>
        public DateTimeOffset? LastStatusAt { get; set; }

        /// <summary>Son durum değişikliğini yapan kullanıcı kimliği.</summary>
        public int LastStatusBy { get; set; }
    }
}

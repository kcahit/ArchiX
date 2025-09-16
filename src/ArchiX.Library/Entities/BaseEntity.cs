using System.ComponentModel.DataAnnotations.Schema;

using ArchiX.Library.DomainEvents.Contracts;

namespace ArchiX.Library.Entities
{
    /// <summary>
    /// Tüm kalıcı varlıklar (entity) için ortak taban sınıf.
    /// Zamanlar DateTimeOffset (precision 4) kullanır.
    /// </summary>
    public abstract class BaseEntity : IEntity
    {
        /// <summary>
        /// Bu türün tabloya haritalanıp haritalanmayacağını belirleyen bayrak (mevcut mimari).
        /// Türevde <c>new static readonly bool MapToDb = false;</c> diyerek tablo oluşturmayı kapatabilirsin.
        /// </summary>
        public static readonly bool MapToDb = true;

        /// <summary>
        /// İleriye dönük adlandırma için zararsız alias. Şu an davranışı <see cref="MapToDb"/> ile aynıdır.
        /// Türevlerde istenirse <c>new static readonly bool IncludeInSchema = false;</c> şeklinde gizlenebilir.
        /// </summary>
        public static bool IncludeInSchema => MapToDb;

        // ----------------- Kimlik -----------------

        /// <summary>Benzersiz tamsayı kimlik.</summary>
        public int Id { get; set; }

        /// <summary>Harici sistemlerle korelasyon için küresel benzersiz anahtar.</summary>
        public Guid RowId { get; set; }

        // ----------------- Oluşturma / Güncelleme -----------------

        /// <summary>Kayıt oluşturulma tarihi (UTC).</summary>
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>Kaydı oluşturan kullanıcı kimliği.</summary>
        public int CreatedBy { get; set; }

        /// <summary>Son güncelleme tarihi (UTC).</summary>
        public DateTimeOffset? UpdatedAt { get; set; }

        /// <summary>Son güncelleme yapan kullanıcı kimliği.</summary>
        public int? UpdatedBy { get; set; }

        // ----------------- Statü -----------------

        /// <summary>Geçerli statü kimliği (varsayılan: 3 → Approved).</summary>
        public int StatusId { get; set; } = 3;

        /// <summary>Statü değişiminin gerçekleştiği tarih (UTC).</summary>
        public DateTimeOffset? LastStatusAt { get; set; }

        /// <summary>Statüyü en son değiştiren kullanıcı kimliği.</summary>
        public int LastStatusBy { get; set; }

        // ----------------- Domain Events -----------------

        /// <summary>Yerel domain event kuyruğu (commit sonrası publish edilir).</summary>
        private readonly List<IDomainEvent> _domainEvents = [];

        /// <summary>Toplanan domain event'lerin salt-okunur görünümü (EF tarafından eşlenmez).</summary>
        [NotMapped]
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        /// <summary>Türev sınıfların domain event eklemesini sağlar.</summary>
        /// <param name="event">Eklenecek domain event.</param>
        protected void AddDomainEvent(IDomainEvent @event)
        {
            if (@event is null) return;
            _domainEvents.Add(@event);
        }

        /// <summary>Kuyruktan belirli bir domain event örneğini kaldırır.</summary>
        /// <param name="event">Kaldırılacak domain event.</param>
        protected void RemoveDomainEvent(IDomainEvent @event)
        {
            if (@event is null) return;
            _domainEvents.Remove(@event);
        }

        /// <summary>Kuyruktaki tüm domain event'leri temizler (genellikle publish sonrası çağrılır).</summary>
        public void ClearDomainEvents() => _domainEvents.Clear();
    }
}

using System.ComponentModel.DataAnnotations.Schema;

using ArchiX.Library.Abstractions.DomainEvents;
namespace ArchiX.Library.Entities
{
    public abstract class BaseEntity
    { // Şema dâhil etme bayrakları
        public static readonly bool MapToDb = true;
        public static bool IncludeInSchema => MapToDb;

        // Yerleşik statü sabitleri
        public const int ApprovedStatusId = 3;
        public const int DeletedStatusId = 6;

        // Kimlik
        public int Id { get; set; }
        public Guid RowId { get; set; }

        // Audit
        public DateTimeOffset CreatedAt { get; set; }
        public int CreatedBy { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }

        // Statü
        public int StatusId { get; set; } = ApprovedStatusId;
        public DateTimeOffset? LastStatusAt { get; set; }
        public int LastStatusBy { get; set; }

        // Güvenlik
        public bool IsProtected { get; set; }

        // Domain Events (persist edilmez)
        private readonly List<IDomainEvent> _domainEvents = [];
        [NotMapped] public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        protected BaseEntity() { } // CreatedAt burada atanmaz; DB default veya SaveChanges ile doldur.

        // Yardımcılar
        protected void AddDomainEvent(IDomainEvent @event)
        {
            if (@event is null) return;
            _domainEvents.Add(@event);
        }
        protected void RemoveDomainEvent(IDomainEvent @event)
        {
            if (@event is null) return;
            _domainEvents.Remove(@event);
        }
        public void ClearDomainEvents() => _domainEvents.Clear();

        public void MarkCreated(int userId)
        {
            CreatedAt = DateTimeOffset.UtcNow;
            CreatedBy = userId;
        }
        public void MarkUpdated(int userId)
        {
            UpdatedAt = DateTimeOffset.UtcNow;
            UpdatedBy = userId;
        }
        public void SetStatus(int statusId, int userId)
        {
            StatusId = statusId;
            LastStatusAt = DateTimeOffset.UtcNow;
            LastStatusBy = userId;
        }
        public void SoftDelete(int userId) => SetStatus(DeletedStatusId, userId);
    }
}


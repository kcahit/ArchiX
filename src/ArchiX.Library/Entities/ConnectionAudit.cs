namespace ArchiX.Library.Entities
{
    /// <summary>Connection policy evaluation audit trail.</summary>
    public class ConnectionAudit
    {
        public long Id { get; set; }
        public DateTimeOffset AttemptedAt { get; set; }
        public string NormalizedServer { get; set; } = null!;
        public string Mode { get; set; } = null!;     // Off/Warn/Enforce
        public string Result { get; set; } = null!;   // Allowed/Warn/Blocked
        public string? ReasonCode { get; set; }
        public Guid CorrelationId { get; set; }
        public int? UserId { get; set; }
        public string RawConnectionMasked { get; set; } = null!;
    }
}

namespace ArchiX.Library.Abstractions.ConnectionPolicy
{
    /// <summary>ConnectionPolicy değerlendirme sonucu (Mode, Result, ReasonCode, NormalizedServer).</summary>
    public sealed record ConnectionPolicyResult(
        string Mode,            // Off | Warn | Enforce
        string Result,          // Allowed | Warn | Blocked
        string? ReasonCode,     // null veya REASON_CODE
        string NormalizedServer // host[:port]
    );
}

namespace ArchiX.Library.Abstractions.ConnectionPolicy
{
    /// <summary>
    /// Connection Policy configuration (precedence: Env > DB > appsettings).
    /// </summary>
    public sealed record ConnectionPolicyOptions
    {
        public string Mode { get; init; } = "Warn"; // Off|Warn|Enforce
        public string[] AllowedHosts { get; init; } = [];
        public string[] AllowedCidrs { get; init; } = [];

        public bool RequireEncrypt { get; init; } = true;
        public bool ForbidTrustServerCertificate { get; init; } = true;
        public bool AllowIntegratedSecurity { get; init; } = false;

        public bool IsWhitelistEmpty => AllowedHosts.Length == 0 && AllowedCidrs.Length == 0;
    }
}

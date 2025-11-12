namespace ArchiX.Library.Abstractions.ConnectionPolicy
{
    /// <summary>Connection Policy violation codes.</summary>
    public static class ConnectionPolicyReasonCodes
    {
        public const string SERVER_NOT_WHITELISTED = "SERVER_NOT_WHITELISTED";
        public const string WHITELIST_EMPTY        = "WHITELIST_EMPTY";
        public const string ENCRYPT_REQUIRED       = "ENCRYPT_REQUIRED";
        public const string TRUST_CERT_FORBIDDEN   = "TRUST_CERT_FORBIDDEN";
        public const string FORBIDDEN_INTEGRATED_SECURITY = "FORBIDDEN_INTEGRATED_SECURITY";
    }
}

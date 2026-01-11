namespace ArchiX.Library.Abstractions.Connections;

public sealed record ConnectionProfile(
    string Provider,
    string Server,
    string Database,
    string Auth,
    string? User,
    string? PasswordRef,
    bool? Encrypt,
    bool? TrustServerCertificate
);

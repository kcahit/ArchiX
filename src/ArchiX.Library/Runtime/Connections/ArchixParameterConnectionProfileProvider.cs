using System.Text.Json;
using ArchiX.Library.Abstractions.Connections;
using ArchiX.Library.Context;
using Microsoft.EntityFrameworkCore;

namespace ArchiX.Library.Runtime.Connections;

internal sealed class ArchixParameterConnectionProfileProvider(
    IDbContextFactory<AppDbContext> dbFactory)
    : IConnectionProfileProvider
{
    private const int GlobalApplicationId = 1;
    private const string Group = "ConnectionStrings";
    private const string Key = "ConnectionStrings";

    public async ValueTask<ConnectionProfile> GetProfileAsync(string connectionName, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(connectionName))
            throw new ArgumentException("connectionName is null/empty.", nameof(connectionName));

        await using var db = await dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);

        var parameter = await db.Parameters.AsNoTracking()
            .Include(p => p.Applications)
            .Where(p => p.Group == Group && p.Key == Key)
            .SingleOrDefaultAsync(ct)
            .ConfigureAwait(false);

        var appValue = parameter?.Applications.FirstOrDefault(a => a.ApplicationId == GlobalApplicationId);
        var param = appValue?.Value;

        if (string.IsNullOrWhiteSpace(param))
            throw new InvalidOperationException($"Missing parameter: ApplicationId={GlobalApplicationId}, Group='{Group}', Key='{Key}'.");

        using var doc = JsonDocument.Parse(param);
        if (doc.RootElement.ValueKind != JsonValueKind.Object)
            throw new InvalidOperationException("ConnectionStrings parameter JSON must be an object (alias -> profile).");

        if (!doc.RootElement.TryGetProperty(connectionName, out var profileEl))
            throw new KeyNotFoundException($"Connection alias not found: '{connectionName}'.");

        var provider = profileEl.TryGetProperty("Provider", out var vProvider) ? vProvider.GetString() : null;
        var server = profileEl.TryGetProperty("Server", out var vServer) ? vServer.GetString() : null;
        var database = profileEl.TryGetProperty("Database", out var vDb) ? vDb.GetString() : null;
        var auth = profileEl.TryGetProperty("Auth", out var vAuth) ? vAuth.GetString() : null;
        var user = profileEl.TryGetProperty("User", out var vUser) ? vUser.GetString() : null;
        var passwordRef = profileEl.TryGetProperty("PasswordRef", out var vPwdRef) ? vPwdRef.GetString() : null;

        bool? encrypt = profileEl.TryGetProperty("Encrypt", out var vEncrypt) && vEncrypt.ValueKind is JsonValueKind.True or JsonValueKind.False
            ? vEncrypt.GetBoolean()
            : null;

        bool? trust = profileEl.TryGetProperty("TrustServerCertificate", out var vTrust) && vTrust.ValueKind is JsonValueKind.True or JsonValueKind.False
            ? vTrust.GetBoolean()
            : null;

        if (string.IsNullOrWhiteSpace(provider)) provider = "SqlServer";
        if (string.IsNullOrWhiteSpace(auth)) auth = "SqlLogin";

        if (string.IsNullOrWhiteSpace(server) || string.IsNullOrWhiteSpace(database))
            throw new InvalidOperationException($"Connection profile '{connectionName}' must specify Server and Database.");

        return new ConnectionProfile(
            Provider: provider!,
            Server: server!,
            Database: database!,
            Auth: auth!,
            User: user,
            PasswordRef: passwordRef,
            Encrypt: encrypt,
            TrustServerCertificate: trust);
    }
}

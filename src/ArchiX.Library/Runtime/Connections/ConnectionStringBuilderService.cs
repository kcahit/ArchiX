using ArchiX.Library.Abstractions.ConnectionPolicy;
using ArchiX.Library.Abstractions.Connections;
using Microsoft.Data.SqlClient;

namespace ArchiX.Library.Runtime.Connections;

internal sealed class ConnectionStringBuilderService(
    IConnectionProfileProvider profiles,
    ISecretResolver secrets,
    IConnectionPolicyEvaluator policy)
{
    public async ValueTask<string> BuildAndValidateAsync(string connectionName, CancellationToken ct = default)
    {
        var profile = await profiles.GetProfileAsync(connectionName, ct).ConfigureAwait(false);

        if (!string.Equals(profile.Provider, "SqlServer", StringComparison.OrdinalIgnoreCase))
            throw new NotSupportedException($"Provider not supported yet: '{profile.Provider}'.");

        var csb = new SqlConnectionStringBuilder
        {
            DataSource = profile.Server,
            InitialCatalog = profile.Database,
        };

        if (profile.Encrypt.HasValue) csb.Encrypt = profile.Encrypt.Value;
        if (profile.TrustServerCertificate.HasValue) csb.TrustServerCertificate = profile.TrustServerCertificate.Value;

        if (string.Equals(profile.Auth, "SqlLogin", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(profile.User))
                throw new InvalidOperationException($"SqlLogin requires User for '{connectionName}'.");

            if (string.IsNullOrWhiteSpace(profile.PasswordRef))
                throw new InvalidOperationException($"SqlLogin requires PasswordRef for '{connectionName}'.");

            var pwd = await secrets.TryResolveAsync(profile.PasswordRef!, ct).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(pwd))
                throw new InvalidOperationException($"PasswordRef could not be resolved for '{connectionName}'.");

            csb.UserID = profile.User;
            csb.Password = pwd;
            csb.IntegratedSecurity = false;
        }
        else if (string.Equals(profile.Auth, "IntegratedSecurity", StringComparison.OrdinalIgnoreCase)
              || string.Equals(profile.Auth, "Windows", StringComparison.OrdinalIgnoreCase))
        {
            csb.IntegratedSecurity = true;
        }
        else
        {
            throw new NotSupportedException($"Auth not supported: '{profile.Auth}' for '{connectionName}'.");
        }

        var connStr = csb.ConnectionString;
        var result = policy.Evaluate(connStr);

        if (string.Equals(result.Result, "Blocked", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"ConnectionPolicy blocked connection '{connectionName}': {result.ReasonCode}");

        return connStr;
    }
}

using ArchiX.Library.Abstractions.ConnectionPolicy;
using ArchiX.Library.Abstractions.Security;
using ArchiX.Library.Context;
using ArchiX.Library.Diagnostics;
using ArchiX.Library.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArchiX.Library.Runtime.ConnectionPolicy
{
    internal sealed class ConnectionPolicyAuditor : IConnectionPolicyAuditor
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory;
        private readonly IMaskingService _masking;

        public ConnectionPolicyAuditor(IDbContextFactory<AppDbContext> dbFactory, IMaskingService masking)
        {
            _dbFactory = dbFactory;
            _masking = masking;
        }

        public void TryWrite(string rawConnectionString, ConnectionPolicyResult result)
        {
            // Fire-and-forget; errors are swallowed to avoid impacting hot path
            _ = Task.Run(async () =>
            {
                try
                {
                    await using var db = await _dbFactory.CreateDbContextAsync();
                    var corr = Correlation.Ambient?.CorrelationId;
                    Guid corrId = Guid.TryParse(corr, out var g) ? g : Guid.Empty;

                    var audit = new ConnectionAudit
                    {
                        AttemptedAt = DateTimeOffset.UtcNow,
                        NormalizedServer = result.NormalizedServer,
                        Mode = result.Mode,
                        Result = result.Result,
                        ReasonCode = result.ReasonCode,
                        CorrelationId = corrId,
                        UserId = null,
                        RawConnectionMasked = MaskConnectionStringForAudit(rawConnectionString)
                    };

                    db.Set<ConnectionAudit>().Add(audit);
                    await db.SaveChangesAsync();
                }
                catch
                {
                    // ignore
                }
            });
        }

        private string MaskConnectionStringForAudit(string? rawConnectionString)
        {
            if (string.IsNullOrWhiteSpace(rawConnectionString))
                return string.Empty;

            // Connection string key/value; preserve structure for debugging, but never leak secrets.
            // Rules:
            // - Password/Pwd always masked (key-based).
            // - Optionally: you can mask User ID later if needed, but doc requires at least secrets.
            var parts = rawConnectionString.Split(';', StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < parts.Length; i++)
            {
                var p = parts[i].Trim();
                if (p.Length == 0) continue;

                var eq = p.IndexOf('=', StringComparison.Ordinal);
                if (eq <= 0)
                {
                    // Not a key=value part; keep but make it non-reversible
                    parts[i] = _masking.Mask(p, 2, 2);
                    continue;
                }

                var key = p[..eq].Trim();
                var value = p[(eq + 1)..].Trim();

                if (IsSecretKey(key))
                {
                    // Always mask full secret value; enforce no-leak
                    var masked = string.IsNullOrEmpty(value) ? string.Empty : new string('*', Math.Min(16, Math.Max(8, value.Length)));
                    parts[i] = $"{key}={masked}";
                    continue;
                }

                // Non-secret values: keep readable but not fully raw (avoid full leakage of full CS)
                // Keep short values as-is; long values partially masked.
                if (value.Length > 32)
                    parts[i] = $"{key}={_masking.Mask(value, 4, 4)}";
                else
                    parts[i] = $"{key}={value}";
            }

            return string.Join(';', parts) + ";";
        }

        private static bool IsSecretKey(string key) =>
            key.Equals("Password", StringComparison.OrdinalIgnoreCase)
            || key.Equals("Pwd", StringComparison.OrdinalIgnoreCase);
    }
}

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
                        RawConnectionMasked = _masking.Mask(rawConnectionString, 4, 4)
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
    }
}
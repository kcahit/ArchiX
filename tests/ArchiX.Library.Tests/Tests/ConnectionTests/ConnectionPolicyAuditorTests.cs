using ArchiX.Library.Abstractions.ConnectionPolicy;
using ArchiX.Library.Runtime.ConnectionPolicy;
using ArchiX.Library.Services.Security;

using Microsoft.EntityFrameworkCore;

using Xunit;

namespace ArchiX.Library.Tests.Tests.ConnectionTests;

public sealed class ConnectionPolicyAuditorTests
{
    [Theory]
    [InlineData("Server=localhost;Database=Db;User Id=sa;Password=SuperSecret123!;Encrypt=True;TrustServerCertificate=False;")]
    [InlineData("Server=localhost;Database=Db;User Id=sa;Pwd=SuperSecret123!;Encrypt=True;TrustServerCertificate=False;")]
    public void ConnectionPolicyAuditor_DoesNotLeakPassword_InAuditMask(string raw)
    {
        var masking = new MaskingService();

        var dbFactory = new FakeDbFactory();
        var auditor = new ConnectionPolicyAuditor(dbFactory, masking);

        auditor.TryWrite(raw, new ConnectionPolicyResult("Enforce", "Allowed", null, "localhost"));

        // Fire-and-forget Task.Run nedeniyle beklemek zorundayız (timeout ile).
        var masked = WaitForMasked(dbFactory);

        Assert.NotEmpty(masked);
        Assert.DoesNotContain("SuperSecret123!", masked, StringComparison.Ordinal);

        // raw'da Password kullanıldıysa Password= beklenir, Pwd kullanıldıysa Pwd= beklenir
        if (raw.Contains("Password=", StringComparison.OrdinalIgnoreCase))
            Assert.Contains("Password=", masked, StringComparison.OrdinalIgnoreCase);
        else
            Assert.Contains("Pwd=", masked, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ConnectionPolicyAuditor_MasksPwdKey_WhenPwdIsUsed()
    {
        var masking = new MaskingService();

        var dbFactory = new FakeDbFactory();
        var auditor = new ConnectionPolicyAuditor(dbFactory, masking);

        var raw = "Server=localhost;Database=Db;User Id=sa;Pwd=SuperSecret123!;Encrypt=True;TrustServerCertificate=False;";
        auditor.TryWrite(raw, new ConnectionPolicyResult("Enforce", "Allowed", null, "localhost"));

        var masked = WaitForMasked(dbFactory);

        Assert.NotEmpty(masked);
        Assert.DoesNotContain("SuperSecret123!", masked, StringComparison.Ordinal);
        Assert.Contains("Pwd=", masked, StringComparison.OrdinalIgnoreCase);
    }

    private static string WaitForMasked(FakeDbFactory factory)
    {
        // max 2 saniye bekle (CI stabilitesi için)
        var start = Environment.TickCount64;
        while (Environment.TickCount64 - start < 2_000)
        {
            var val = factory.LastRawConnectionMasked;
            if (!string.IsNullOrWhiteSpace(val))
                return val;

            Thread.Sleep(10);
        }

        return factory.LastRawConnectionMasked ?? string.Empty;
    }

    // --- Test doubles (no real DB write) ---

    private sealed class FakeDbFactory : IDbContextFactory<ArchiX.Library.Context.AppDbContext>
    {
        public volatile string? LastRawConnectionMasked;

        public ArchiX.Library.Context.AppDbContext CreateDbContext()
            => throw new NotSupportedException();

        public Task<ArchiX.Library.Context.AppDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
        {
            var opt = new DbContextOptionsBuilder<ArchiX.Library.Context.AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var db = new FakeDbContext(opt, onSave: masked => LastRawConnectionMasked = masked);
            db.Database.EnsureCreated();
            return Task.FromResult<ArchiX.Library.Context.AppDbContext>(db);
        }
    }

    private sealed class FakeDbContext : ArchiX.Library.Context.AppDbContext
    {
        private readonly Action<string?> _onSave;

        public FakeDbContext(DbContextOptions<ArchiX.Library.Context.AppDbContext> options, Action<string?> onSave)
            : base(options)
        {
            _onSave = onSave;
        }

        public override int SaveChanges()
        {
            Capture();
            return 1;
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            Capture();
            return Task.FromResult(1);
        }

        private void Capture()
        {
            var audit = ChangeTracker.Entries()
                .Select(e => e.Entity)
                .OfType<ArchiX.Library.Entities.ConnectionAudit>()
                .FirstOrDefault();

            _onSave(audit?.RawConnectionMasked);
        }
    }
}

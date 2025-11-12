using System.Collections.Concurrent;
using ArchiX.Library.Abstractions.ConnectionPolicy;
using ArchiX.Library.Runtime.ConnectionPolicy;
using Xunit;

namespace ArchiX.Library.Tests.Tests.RuntimeTests.ConnectionPolicy
{
    public class ConnectionPolicyEvaluator_C3_Tests
    {
        private sealed class FakeProvider(ConnectionPolicyOptions opts) : IConnectionPolicyProvider
        {
            public ConnectionPolicyOptions Current { get; private set; } = opts;
            public void ForceRefresh() { }
        }

        private sealed class CapturingAuditor : IConnectionPolicyAuditor
        {
            public ConcurrentQueue<(string raw, ConnectionPolicyResult result)> Events { get; } = new();
            public void TryWrite(string rawConnectionString, ConnectionPolicyResult result)
            {
                Events.Enqueue((rawConnectionString, result));
            }
        }

        [Fact]
        public void Audit_Is_Written_On_Evaluate()
        {
            var auditor = new CapturingAuditor();
            var opts = new ConnectionPolicyOptions
            {
                Mode = "Enforce",
                RequireEncrypt = true,
                ForbidTrustServerCertificate = true,
                AllowIntegratedSecurity = false,
                AllowedHosts = new[] { "db-prod-01" }
            };

            var sut = new ConnectionPolicyEvaluator(new FakeProvider(opts), auditor);

            var cs = "Server=db-prod-01;Encrypt=True;TrustServerCertificate=False;Integrated Security=False";
            sut.Evaluate(cs);

            var ev = Assert.Single(auditor.Events);
            var (raw, result) = ev;
            Assert.Equal(cs, raw); // Masking auditor içinde yapýlýr; burada raw geçiyor.
            Assert.Equal("Allowed", result.Result);
            Assert.Null(result.ReasonCode);
            Assert.Equal("db-prod-01", result.NormalizedServer);
        }

        [Fact]
        public void Audit_Captures_Violation()
        {
            var auditor = new CapturingAuditor();
            var opts = new ConnectionPolicyOptions
            {
                Mode = "Warn",
                RequireEncrypt = true,
                ForbidTrustServerCertificate = true,
                AllowIntegratedSecurity = false,
                AllowedHosts = new[] { "db-prod-01" }
            };

            var sut = new ConnectionPolicyEvaluator(new FakeProvider(opts), auditor);

            // Violation: TrustServerCertificate=True
            var cs = "Server=db-prod-01;Encrypt=True;TrustServerCertificate=True";
            sut.Evaluate(cs);

            var ev = Assert.Single(auditor.Events);
            var (_, result) = ev;
            Assert.Equal(ConnectionPolicyReasonCodes.TRUST_CERT_FORBIDDEN, result.ReasonCode);
            Assert.Equal("Warn", result.Result);
        }
    }
}
using ArchiX.Library.Abstractions.ConnectionPolicy;
using ArchiX.Library.Runtime.ConnectionPolicy;
using Xunit;

namespace ArchiX.Library.Tests.Tests.RuntimeTests.ConnectionPolicy
{
    public class ConnectionPolicyEvaluator_C2_Tests
    {
        private sealed class FakeProvider(ConnectionPolicyOptions opts) : IConnectionPolicyProvider
        {
            public ConnectionPolicyOptions Current { get; private set; } = opts;
            public void ForceRefresh() { }
        }

        private static ConnectionPolicyEvaluator CreateEvaluator(ConnectionPolicyOptions opts)
            => new(new FakeProvider(opts));

        [Fact]
        public void Empty_Whitelist_Yields_Warn_In_Warn_Mode()
        {
            var opts = new ConnectionPolicyOptions
            {
                Mode = "Warn",
                RequireEncrypt = true,
                ForbidTrustServerCertificate = true,
                AllowIntegratedSecurity = false,
                AllowedHosts = [],
                AllowedCidrs = []
            };
            var sut = CreateEvaluator(opts);

            var r = sut.Evaluate("Server=dev-db-01;Encrypt=True;TrustServerCertificate=False;Integrated Security=False");

            Assert.Equal("Warn", r.Mode);
            Assert.Equal("Warn", r.Result);
            Assert.Equal(ConnectionPolicyReasonCodes.WHITELIST_EMPTY, r.ReasonCode);
        }

        [Fact]
        public void Empty_Whitelist_Yields_Blocked_In_Enforce_Mode()
        {
            var opts = new ConnectionPolicyOptions
            {
                Mode = "Enforce",
                RequireEncrypt = true,
                ForbidTrustServerCertificate = true,
                AllowIntegratedSecurity = false,
                AllowedHosts = [],
                AllowedCidrs = []
            };
            var sut = CreateEvaluator(opts);

            var r = sut.Evaluate("Server=dev-db-01;Encrypt=True;TrustServerCertificate=False;Integrated Security=False");

            Assert.Equal("Enforce", r.Mode);
            Assert.Equal("Blocked", r.Result);
            Assert.Equal(ConnectionPolicyReasonCodes.WHITELIST_EMPTY, r.ReasonCode);
        }

        [Fact]
        public void AllowedHosts_Allows_By_Host_Only()
        {
            var opts = new ConnectionPolicyOptions
            {
                Mode = "Enforce",
                RequireEncrypt = true,
                ForbidTrustServerCertificate = true,
                AllowIntegratedSecurity = false,
                AllowedHosts = new[] { "dev-db-01" }
            };
            var sut = CreateEvaluator(opts);

            var r = sut.Evaluate("Server=dev-db-01;Encrypt=True;TrustServerCertificate=False;Integrated Security=False");

            Assert.Equal("Allowed", r.Result);
            Assert.Null(r.ReasonCode);
        }

        [Fact]
        public void AllowedHosts_Allows_By_Host_And_Port()
        {
            var opts = new ConnectionPolicyOptions
            {
                Mode = "Enforce",
                RequireEncrypt = true,
                ForbidTrustServerCertificate = true,
                AllowIntegratedSecurity = false,
                AllowedHosts = new[] { "dev-db-01:1533" }
            };
            var sut = CreateEvaluator(opts);

            var r = sut.Evaluate("Server=dev-db-01:1533;Encrypt=True;TrustServerCertificate=False;Integrated Security=False");

            Assert.Equal("Allowed", r.Result);
            Assert.Equal("dev-db-01:1533", r.NormalizedServer);
            Assert.Null(r.ReasonCode);
        }

        [Fact]
        public void AllowedHosts_Port_Mismatch_Is_Not_Whitelisted()
        {
            var opts = new ConnectionPolicyOptions
            {
                Mode = "Enforce",
                RequireEncrypt = true,
                ForbidTrustServerCertificate = true,
                AllowIntegratedSecurity = false,
                AllowedHosts = new[] { "dev-db-01:1533" }
            };
            var sut = CreateEvaluator(opts);

            var r = sut.Evaluate("Server=dev-db-01:1433;Encrypt=True;TrustServerCertificate=False;Integrated Security=False");

            Assert.Equal("Blocked", r.Result);
            Assert.Equal(ConnectionPolicyReasonCodes.SERVER_NOT_WHITELISTED, r.ReasonCode);
        }

        [Fact]
        public void AllowedCidrs_Allows_Ip_In_Range()
        {
            var opts = new ConnectionPolicyOptions
            {
                Mode = "Enforce",
                RequireEncrypt = true,
                ForbidTrustServerCertificate = true,
                AllowIntegratedSecurity = false,
                AllowedCidrs = new[] { "10.1.2.0/24" }
            };
            var sut = CreateEvaluator(opts);

            var r = sut.Evaluate("Server=10.1.2.5;Encrypt=True;TrustServerCertificate=False;Integrated Security=False");

            Assert.Equal("Allowed", r.Result);
            Assert.Null(r.ReasonCode);
        }

        [Fact]
        public void AllowedCidrs_Blocks_Ip_Outside_Range()
        {
            var opts = new ConnectionPolicyOptions
            {
                Mode = "Enforce",
                RequireEncrypt = true,
                ForbidTrustServerCertificate = true,
                AllowIntegratedSecurity = false,
                AllowedCidrs = new[] { "10.1.2.0/24" }
            };
            var sut = CreateEvaluator(opts);

            var r = sut.Evaluate("Server=10.1.3.5;Encrypt=True;TrustServerCertificate=False;Integrated Security=False");

            Assert.Equal("Blocked", r.Result);
            Assert.Equal(ConnectionPolicyReasonCodes.SERVER_NOT_WHITELISTED, r.ReasonCode);
        }
    }
}

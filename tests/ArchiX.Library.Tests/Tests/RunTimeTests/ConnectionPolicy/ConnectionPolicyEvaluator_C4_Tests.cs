using ArchiX.Library.Abstractions.ConnectionPolicy;
using ArchiX.Library.Runtime.ConnectionPolicy;
using Xunit;

namespace ArchiX.Library.Tests.Tests.RuntimeTests.ConnectionPolicy
{
    public class ConnectionPolicyEvaluator_C4_Tests
    {
        private sealed class FakeProvider(ConnectionPolicyOptions opts) : IConnectionPolicyProvider
        {
            public ConnectionPolicyOptions Current { get; private set; } = opts;
            public void ForceRefresh() { }
        }

        private static ConnectionPolicyEvaluator Create(ConnectionPolicyOptions opts) => new(new FakeProvider(opts));

        [Fact]
        public void Parses_Ipv6_Bracket_And_Port()
        {
            var opts = new ConnectionPolicyOptions
            {
                Mode = "Enforce",
                RequireEncrypt = true,
                ForbidTrustServerCertificate = true,
                AllowIntegratedSecurity = false,
                AllowedHosts = new[] { "[fe80::1]:1444" }
            };
            var sut = Create(opts);

            var r = sut.Evaluate("Server=[fe80::1]:1444;Encrypt=True;TrustServerCertificate=False;Integrated Security=False");
            Assert.Equal("Allowed", r.Result);
            Assert.Equal("[fe80::1]:1444", r.NormalizedServer);
        }

        [Fact]
        public void Parses_Ipv6_Bracket_Comma_Port()
        {
            var opts = new ConnectionPolicyOptions
            {
                Mode = "Enforce",
                RequireEncrypt = true,
                ForbidTrustServerCertificate = true,
                AllowIntegratedSecurity = false,
                AllowedHosts = new[] { "[fe80::abcd]:1443" }
            };
            var sut = Create(opts);

            var r = sut.Evaluate("Data Source=[fe80::abcd],1443;Encrypt=True;TrustServerCertificate=False;Integrated Security=False");
            Assert.Equal("Allowed", r.Result);
            Assert.Equal("[fe80::abcd]:1443", r.NormalizedServer);
        }

        [Fact]
        public void HostOnly_Allows_All_Instances_On_Same_Host()
        {
            var opts = new ConnectionPolicyOptions
            {
                Mode = "Enforce",
                RequireEncrypt = true,
                ForbidTrustServerCertificate = true,
                AllowIntegratedSecurity = false,
                AllowedHosts = new[] { "db-host-01" }
            };
            var sut = Create(opts);

            var r = sut.Evaluate("Server=db-host-01\\SQLEXPRESS;Encrypt=True;TrustServerCertificate=False;Integrated Security=False");
            Assert.Equal("Allowed", r.Result);
            Assert.Equal("db-host-01\\SQLEXPRESS", r.NormalizedServer); // port yoksa instance korunur, normalize host=host\instance
        }

        [Fact]
        public void Instance_Exact_Match_Required_When_Listed_With_Instance()
        {
            var opts = new ConnectionPolicyOptions
            {
                Mode = "Enforce",
                RequireEncrypt = true,
                ForbidTrustServerCertificate = true,
                AllowIntegratedSecurity = false,
                AllowedHosts = new[] { "db-host-02\\INST1" }
            };
            var sut = Create(opts);

            var ok = sut.Evaluate("Server=db-host-02\\INST1;Encrypt=True;TrustServerCertificate=False;Integrated Security=False");
            Assert.Equal("Allowed", ok.Result);

            var blocked = sut.Evaluate("Server=db-host-02\\INST2;Encrypt=True;TrustServerCertificate=False;Integrated Security=False");
            Assert.Equal("Blocked", blocked.Result);
            Assert.Equal(ConnectionPolicyReasonCodes.SERVER_NOT_WHITELISTED, blocked.ReasonCode);
        }
    }
}
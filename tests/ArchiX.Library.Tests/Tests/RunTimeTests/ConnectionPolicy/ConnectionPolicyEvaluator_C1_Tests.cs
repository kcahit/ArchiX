using ArchiX.Library.Abstractions.ConnectionPolicy;
using ArchiX.Library.Runtime.ConnectionPolicy;

using Xunit;

namespace ArchiX.Library.Tests.Tests.RuntimeTests.ConnectionPolicy
{
    public class ConnectionPolicyEvaluator_C1_Tests
    {
        // Basit fake provider — sadece Current döner
        private sealed class FakeProvider(ConnectionPolicyOptions opts) : IConnectionPolicyProvider
        {
            public ConnectionPolicyOptions Current { get; private set; } = opts;
            public void ForceRefresh() { }
        }

        private static ConnectionPolicyEvaluator CreateEvaluator(ConnectionPolicyOptions opts)
            => new(new FakeProvider(opts));

        [Fact]
        public void Off_Mode_Allows_Anything()
        {
            var opts = new ConnectionPolicyOptions
            {
                Mode = "Off",
                RequireEncrypt = true,
                ForbidTrustServerCertificate = true,
                AllowIntegratedSecurity = false
            };
            var sut = CreateEvaluator(opts);

            var r = sut.Evaluate("Server=dev-db-01;"); // Encrypt yok ama Off → Allowed

            Assert.Equal("Off", r.Mode);
            Assert.Equal("Allowed", r.Result);
            Assert.Null(r.ReasonCode);
            Assert.Equal("dev-db-01", r.NormalizedServer);
        }

        [Theory]
        [InlineData("Warn", "Warn")]
        [InlineData("Enforce", "Blocked")]
        public void RequireEncrypt_Violation_Triggers_Mode(string mode, string expectedResult)
        {
            var opts = new ConnectionPolicyOptions
            {
                Mode = mode,
                RequireEncrypt = true,
                ForbidTrustServerCertificate = false,
                AllowIntegratedSecurity = true
            };
            var sut = CreateEvaluator(opts);

            // Encrypt eksik → ihlal
            var r = sut.Evaluate("Server=dev-db-01;");

            Assert.Equal(mode, r.Mode);
            Assert.Equal(expectedResult, r.Result);
            Assert.Equal(ConnectionPolicyReasonCodes.ENCRYPT_REQUIRED, r.ReasonCode);
        }

        [Theory]
        [InlineData("Warn", "Warn")]
        [InlineData("Enforce", "Blocked")]
        public void TrustServerCertificate_Violation_Triggers_Mode(string mode, string expectedResult)
        {
            var opts = new ConnectionPolicyOptions
            {
                Mode = mode,
                RequireEncrypt = true,
                ForbidTrustServerCertificate = true,
                AllowIntegratedSecurity = true
            };
            var sut = CreateEvaluator(opts);

            var r = sut.Evaluate("Server=dev-db-01;Encrypt=True;TrustServerCertificate=True");

            Assert.Equal(mode, r.Mode);
            Assert.Equal(expectedResult, r.Result);
            Assert.Equal(ConnectionPolicyReasonCodes.TRUST_CERT_FORBIDDEN, r.ReasonCode);
        }

        [Theory]
        [InlineData("Warn", "Warn")]
        [InlineData("Enforce", "Blocked")]
        public void IntegratedSecurity_Violation_Triggers_Mode(string mode, string expectedResult)
        {
            var opts = new ConnectionPolicyOptions
            {
                Mode = mode,
                RequireEncrypt = true,
                ForbidTrustServerCertificate = false,
                AllowIntegratedSecurity = false
            };
            var sut = CreateEvaluator(opts);

            var r = sut.Evaluate("Server=dev-db-01;Encrypt=True;Integrated Security=True");

            Assert.Equal(mode, r.Mode);
            Assert.Equal(expectedResult, r.Result);
            Assert.Equal(ConnectionPolicyReasonCodes.FORBIDDEN_INTEGRATED_SECURITY, r.ReasonCode);
        }

        [Fact]
        public void Success_When_All_Flags_Comply()
        {
            var opts = new ConnectionPolicyOptions
            {
                Mode = "Enforce",
                RequireEncrypt = true,
                ForbidTrustServerCertificate = true,
                AllowIntegratedSecurity = false,
                // C-2: whitelist sağlamak için ekle
                AllowedHosts = new[] { "dev-db-01" }
            };
            var sut = CreateEvaluator(opts);

            var r = sut.Evaluate("Server=dev-db-01;Encrypt=True;TrustServerCertificate=False;Integrated Security=False");

            Assert.Equal("Enforce", r.Mode);
            Assert.Equal("Allowed", r.Result);
            Assert.Null(r.ReasonCode);
        }

        [Fact]
        public void Normalizes_Server_With_Port_Comma_Style()
        {
            var opts = new ConnectionPolicyOptions { Mode = "Warn", RequireEncrypt = true };
            var sut = CreateEvaluator(opts);

            var r = sut.Evaluate("Server=dev-db-01,1433;Encrypt=True");
            Assert.Equal("dev-db-01:1433", r.NormalizedServer);
        }

        [Fact]
        public void Normalizes_Server_With_Port_Colon_Style()
        {
            var opts = new ConnectionPolicyOptions { Mode = "Warn", RequireEncrypt = true };
            var sut = CreateEvaluator(opts);

            var r = sut.Evaluate("Server=dev-db-01:1533;Encrypt=True");
            Assert.Equal("dev-db-01:1533", r.NormalizedServer);
        }

        [Fact]
        public void Throws_On_Empty_ConnectionString()
        {
            var opts = new ConnectionPolicyOptions { Mode = "Warn", RequireEncrypt = true };
            var sut = CreateEvaluator(opts);

            Assert.Throws<ArgumentException>(() => sut.Evaluate(""));
        }
    }
}

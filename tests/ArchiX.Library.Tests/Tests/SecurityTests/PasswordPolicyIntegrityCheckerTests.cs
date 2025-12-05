using ArchiX.Library.Runtime.Security;
using Xunit;

namespace ArchiX.Library.Tests.Tests.SecurityTests
{
    public sealed class PasswordPolicyIntegrityCheckerTests
    {
        private const string TestKey = "test-hmac-secret-key-12345";
        private const string TestJson = "{\"version\":1,\"minLength\":12}";

        [Fact]
        public void ComputeSignature_ReturnsSameValue_ForSameInput()
        {
            // Arrange
            Environment.SetEnvironmentVariable("ARCHIX_POLICY_HMAC_KEY", TestKey);

            // Act
            var sig1 = PasswordPolicyIntegrityChecker.ComputeSignature(TestJson);
            var sig2 = PasswordPolicyIntegrityChecker.ComputeSignature(TestJson);

            // Assert
            Assert.Equal(sig1, sig2);

            // Cleanup
            Environment.SetEnvironmentVariable("ARCHIX_POLICY_HMAC_KEY", null);
        }

        [Fact]
        public void ComputeSignature_ThrowsException_WhenKeyNotSet()
        {
            // Arrange
            Environment.SetEnvironmentVariable("ARCHIX_POLICY_HMAC_KEY", null);

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() =>
                PasswordPolicyIntegrityChecker.ComputeSignature(TestJson));
            Assert.Contains("HMAC anahtarý tanýmlý deðil", ex.Message);
        }

        [Fact]
        public void VerifySignature_ReturnsTrue_WhenSignatureValid()
        {
            // Arrange
            Environment.SetEnvironmentVariable("ARCHIX_POLICY_HMAC_KEY", TestKey);
            var signature = PasswordPolicyIntegrityChecker.ComputeSignature(TestJson);

            // Act
            var isValid = PasswordPolicyIntegrityChecker.VerifySignature(TestJson, signature);

            // Assert
            Assert.True(isValid);

            // Cleanup
            Environment.SetEnvironmentVariable("ARCHIX_POLICY_HMAC_KEY", null);
        }

        [Fact]
        public void VerifySignature_ReturnsFalse_WhenSignatureInvalid()
        {
            // Arrange
            Environment.SetEnvironmentVariable("ARCHIX_POLICY_HMAC_KEY", TestKey);
            var signature = PasswordPolicyIntegrityChecker.ComputeSignature(TestJson);
            var tamperedJson = "{\"version\":2,\"minLength\":8}";

            // Act
            var isValid = PasswordPolicyIntegrityChecker.VerifySignature(tamperedJson, signature);

            // Assert
            Assert.False(isValid);

            // Cleanup
            Environment.SetEnvironmentVariable("ARCHIX_POLICY_HMAC_KEY", null);
        }

        [Fact]
        public void VerifySignature_ReturnsFalse_WhenKeyNotSet()
        {
            // Arrange
            Environment.SetEnvironmentVariable("ARCHIX_POLICY_HMAC_KEY", null);

            // Act
            var isValid = PasswordPolicyIntegrityChecker.VerifySignature(TestJson, "fake-signature");

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void IsEnabled_ReturnsTrue_WhenKeySet()
        {
            // Arrange
            Environment.SetEnvironmentVariable("ARCHIX_POLICY_HMAC_KEY", TestKey);

            // Act
            var enabled = PasswordPolicyIntegrityChecker.IsEnabled();

            // Assert
            Assert.True(enabled);

            // Cleanup
            Environment.SetEnvironmentVariable("ARCHIX_POLICY_HMAC_KEY", null);
        }

        [Fact]
        public void IsEnabled_ReturnsFalse_WhenKeyNotSet()
        {
            // Arrange
            Environment.SetEnvironmentVariable("ARCHIX_POLICY_HMAC_KEY", null);

            // Act
            var enabled = PasswordPolicyIntegrityChecker.IsEnabled();

            // Assert
            Assert.False(enabled);
        }
    }
}

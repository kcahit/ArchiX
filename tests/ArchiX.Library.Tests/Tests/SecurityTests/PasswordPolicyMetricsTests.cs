using ArchiX.Library.Runtime.Security;
using Xunit;

namespace ArchiX.Library.Tests.Tests.SecurityTests
{
    public sealed class PasswordPolicyMetricsTests
    {
        [Fact]
        public void RecordRead_DoesNotThrow()
        {
            // Arrange
            var metrics = new PasswordPolicyMetrics();

            // Act & Assert (should not throw)
            metrics.RecordRead(applicationId: 1, fromCache: true);
            metrics.RecordRead(applicationId: 2, fromCache: false);
        }

        [Fact]
        public void RecordInvalidate_DoesNotThrow()
        {
            // Arrange
            var metrics = new PasswordPolicyMetrics();

            // Act & Assert (should not throw)
            metrics.RecordInvalidate(applicationId: 1);
        }

        [Fact]
        public void RecordUpdate_DoesNotThrow()
        {
            // Arrange
            var metrics = new PasswordPolicyMetrics();

            // Act & Assert (should not throw)
            metrics.RecordUpdate(applicationId: 1, success: true);
            metrics.RecordUpdate(applicationId: 2, success: false);
        }

        [Fact]
        public void RecordValidationError_DoesNotThrow()
        {
            // Arrange
            var metrics = new PasswordPolicyMetrics();

            // Act & Assert (should not throw)
            metrics.RecordValidationError(applicationId: 1, errorType: "SCHEMA_ERROR");
        }

        [Fact]
        public void MultipleMetrics_CanBeRecorded()
        {
            // Arrange
            var metrics = new PasswordPolicyMetrics();

            // Act & Assert (should not throw)
            metrics.RecordRead(1, true);
            metrics.RecordInvalidate(1);
            metrics.RecordUpdate(1, true);
            metrics.RecordValidationError(1, "TEST");
        }
    }
}

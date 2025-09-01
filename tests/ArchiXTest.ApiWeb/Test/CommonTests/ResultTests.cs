using ArchiX.Library.Result;
using Xunit;

namespace ArchiXTest.ApiWeb.Tests.CommonTests
{
    public class ResultTests
    {
        [Fact]
        public void Success_Should_Return_Success_Result()
        {
            // Act
            var result = Result.Success();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.Equal(Error.None, result.Error);
        }

        [Fact]
        public void Failure_Should_Return_Failure_Result_With_Error()
        {
            // Arrange
            var error = new Error(
                "VAL001",                  // Code
                "Geçersiz değer",          // Message
                null,                      // Details
                DateTimeOffset.UtcNow,     // ServerTimeUtc
                null,                      // LocalTime
                null,                      // TimeZoneId
                null,                      // CorrelationId
                null,                      // TraceId
                ErrorSeverity.Error        // Severity (enumdan uygun değer)
            );

            // Act
            var result = Result.Failure(error);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.True(result.IsFailure);
            Assert.Equal(error, result.Error);
        }
    }
}

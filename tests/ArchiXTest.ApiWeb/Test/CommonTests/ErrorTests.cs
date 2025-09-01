using Xunit;
using ArchiX.Library.Result;
using System;

namespace ArchiXTest.ApiWeb.Tests.CommonTests
{
    public class ErrorTests
    {
        [Fact]
        public void Error_Should_SetPropertiesCorrectly()
        {
            // Arrange
            var code = "E1001";
            var message = "Test error";
            var correlationId = Guid.NewGuid().ToString();
            var traceId = Guid.NewGuid().ToString();

            // Act
            var error = new Error(code, message)
            {
                CorrelationId = correlationId,
                TraceId = traceId
            };

            // Assert
            Assert.Equal(code, error.Code);
            Assert.Equal(message, error.Message);
            Assert.Equal(correlationId, error.CorrelationId);
            Assert.Equal(traceId, error.TraceId);
        }

        [Fact]
        public void Error_Should_HaveDefaultValues_WhenNotSet()
        {
            // Act
            var error = new Error("E0000", "Default");

            // Assert
            Assert.Equal("E0000", error.Code);
            Assert.Equal("Default", error.Message);
            Assert.Null(error.CorrelationId);
            Assert.Null(error.TraceId);
        }
    }
}

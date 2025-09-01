using ArchiX.Library.Diagnostics;
using Xunit;

namespace ArchiXTest.ApiWeb.Test.DiagnosticsTests
{
    public sealed class CorrelationTests
    {
        [Fact]
        public void BeginScope_ShouldGenerateCorrelationId_WhenNotProvided()
        {
            // Act
            using (Correlation.BeginScope())
            {
                var ctx = Correlation.Ambient;

                // Assert
                Assert.NotNull(ctx);
                Assert.False(string.IsNullOrWhiteSpace(ctx!.CorrelationId));
                Assert.Matches("^[a-f0-9]{32}$", ctx.CorrelationId); // GUID formatı (n)
            }
        }

        [Fact]
        public void BeginScope_ShouldUseProvidedCorrelationId_AndTraceId()
        {
            // Arrange
            var customCorrelationId = "test-corr-id";
            var customTraceId = "trace-123";

            // Act
            using (Correlation.BeginScope(customCorrelationId, customTraceId))
            {
                var ctx = Correlation.Ambient;

                // Assert
                Assert.NotNull(ctx);
                Assert.Equal(customCorrelationId, ctx!.CorrelationId);
                Assert.Equal(customTraceId, ctx.TraceId);
            }
        }

        [Fact]
        public void BeginScope_ShouldRestorePreviousContext_OnDispose()
        {
            // Arrange
            using (Correlation.BeginScope("outer", "trace-1"))
            {
                var outerCtx = Correlation.Ambient;
                Assert.NotNull(outerCtx);

                using (Correlation.BeginScope("inner", "trace-2"))
                {
                    var innerCtx = Correlation.Ambient;
                    Assert.Equal("inner", innerCtx!.CorrelationId);
                }

                // Assert → inner scope kapanınca outer geri gelmeli
                var restoredCtx = Correlation.Ambient;
                Assert.Equal("outer", restoredCtx!.CorrelationId);
            }
        }
    }
}

using ArchiX.Library.Time;

using Xunit;

namespace ArchiXTest.ApiWeb.Tests.CommonTests
{
    public sealed class ClockTests
    {
        private readonly IClock _clock = new SystemClock();

        [Fact]
        public void UtcNow_ShouldReturnCloseToSystemUtcNow()
        {
            // Arrange
            var before = DateTime.UtcNow;

            // Act
            var actual = _clock.UtcNow;

            var after = DateTime.UtcNow;

            // Assert
            Assert.InRange(actual, before, after);
        }
    }
}

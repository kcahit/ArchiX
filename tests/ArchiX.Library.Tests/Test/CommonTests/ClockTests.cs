// File: tests/ArchiXTest.ApiWeb/Test/CommonTests/ClockTests.cs
using ArchiX.Library.Time;

using Xunit;

namespace ArchiXTest.ApiWeb.Test.CommonTests
{
    /// <summary>
    /// Saat sağlayıcı testleri.
    /// </summary>
    public sealed class ClockTests
    {
        [Fact]
        public void UtcNow_ShouldReturnCloseToSystemUtcNow()
        {
            var clock = new SystemClock();
            var now = clock.UtcNow;
            var sys = DateTimeOffset.UtcNow;

            Assert.True((sys - now).Duration() < TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Now_ShouldReturnCloseToSystemNow()
        {
            var clock = new SystemClock();
            var now = clock.UtcNow.LocalDateTime;
            var sys = DateTime.Now;

            Assert.True((sys - now).Duration() < TimeSpan.FromSeconds(1));
        }
    }
}

using ArchiX.Library.Abstractions.Security;
using ArchiX.Library.Services.Security;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace ArchiX.Library.Tests.Tests.SecurityTests
{
    public class AttemptLimiterTests
    {
        [Fact]
        public async Task Allows_Upto_Max_Then_Blocks()
        {
            var cache = new MemoryCache(new MemoryCacheOptions());
            var opts = new AttemptLimiterOptions { MaxAttempts = 3, Window = 10, CooldownSeconds = 30 };
            var limiter = new AttemptLimiter(cache, opts);

            Assert.True(await limiter.TryBeginAsync("user1"));
            Assert.True(await limiter.TryBeginAsync("user1"));
            Assert.True(await limiter.TryBeginAsync("user1"));
            Assert.False(await limiter.TryBeginAsync("user1"));
        }

        [Fact]
        public async Task Reset_Clears_State()
        {
            var cache = new MemoryCache(new MemoryCacheOptions());
            var opts = new AttemptLimiterOptions { MaxAttempts = 1, Window = 10, CooldownSeconds = 30 };
            var limiter = new AttemptLimiter(cache, opts);

            Assert.True(await limiter.TryBeginAsync("user2"));
            Assert.False(await limiter.TryBeginAsync("user2"));

            await limiter.ResetAsync("user2");
            Assert.True(await limiter.TryBeginAsync("user2"));
        }

        [Fact]
        public async Task Window_Expiry_Allows_Again()
        {
            var cache = new MemoryCache(new MemoryCacheOptions());
            var opts = new AttemptLimiterOptions { MaxAttempts = 1, Window = 1, CooldownSeconds = 30 }; // 1 second window
            var limiter = new AttemptLimiter(cache, opts);

            Assert.True(await limiter.TryBeginAsync("user3"));
            await Task.Delay(1200); // Wait > 1 second
            Assert.True(await limiter.TryBeginAsync("user3"));
        }
    }
}

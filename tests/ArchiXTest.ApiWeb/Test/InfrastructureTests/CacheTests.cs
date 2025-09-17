// File: tests/ArchiXTest.ApiWeb/Test/InfrastructureTests/CacheTests.cs
using ArchiX.Library.Infrastructure;

using Xunit;

namespace ArchiXTest.ApiWeb.Test.InfrastructureTests
{
    public class CacheTests
    {
        private static ICacheService CreateMemoryCacheService()
        {
            var services = new ServiceCollection();
            services.AddArchiXMemoryCaching();
            var sp = services.BuildServiceProvider();
            return sp.GetRequiredService<ICacheService>();
        }

        [Fact]
        public async Task Set_Then_Get_Returns_Value()
        {
            var cache = CreateMemoryCacheService();
            await cache.SetAsync("k1", 42);
            var v = await cache.GetAsync<int>("k1");
            Assert.Equal(42, v);
        }

        [Fact]
        public async Task Exists_Then_Remove_Works()
        {
            var cache = CreateMemoryCacheService();
            await cache.SetAsync("k2", "hello");
            Assert.True(await cache.ExistsAsync("k2"));
            await cache.RemoveAsync("k2");
            Assert.False(await cache.ExistsAsync("k2"));
            Assert.Null(await cache.GetAsync<string>("k2"));
        }

        [Fact]
        public async Task GetOrSet_Computes_Once_Then_Returns_Cached()
        {
            var cache = CreateMemoryCacheService();
            int calls = 0;

            var v1 = await cache.GetOrSetAsync(
                "k3",
                async ct => { Interlocked.Increment(ref calls); await Task.Yield(); return "value"; },
                absoluteExpiration: TimeSpan.FromMinutes(5));

            var v2 = await cache.GetOrSetAsync(
                "k3",
                async ct => { Interlocked.Increment(ref calls); await Task.Yield(); return "value2"; });

            Assert.Equal("value", v1);
            Assert.Equal("value", v2);
            Assert.Equal(1, calls);
        }

        [Fact]
        public async Task GetOrSet_Null_Not_Cached_When_Flag_False()
        {
            var cache = CreateMemoryCacheService();

            // cacheNull:false → null cache'lenmez
            string? r1 = await cache.GetOrSetAsync<string?>(
                "k4",
                ct => Task.FromResult<string?>(null),
                cacheNull: false);
            Assert.Null(r1);

            // Tekrar çağrıda factory yeniden çalışır → null döner
            var called = false;
            string? r2 = await cache.GetOrSetAsync<string?>(
                "k4",
                ct => { called = true; return Task.FromResult<string?>(null); },
                cacheNull: false);
            Assert.True(called);
            Assert.Null(r2);
        }

        [Fact]
        public async Task GetOrSet_Null_Cached_When_Flag_True()
        {
            var cache = CreateMemoryCacheService();

            // cacheNull:true → null cache'lenir
            string? r1 = await cache.GetOrSetAsync<string?>(
                "k5",
                ct => Task.FromResult<string?>(null),
                cacheNull: true);
            Assert.Null(r1);

            // Tekrar çağrıda factory ÇALIŞMAMALI
            var called = false;
            string? r2 = await cache.GetOrSetAsync<string?>(
                "k5",
                ct => { called = true; return Task.FromResult<string?>(null); },
                cacheNull: true);
            Assert.False(called);
            Assert.Null(r2);
        }
    }
}

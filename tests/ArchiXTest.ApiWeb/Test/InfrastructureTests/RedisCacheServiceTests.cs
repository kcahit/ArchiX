// File: tests/ArchiXTest.ApiWeb/Test/InfrastructureTests/RedisCacheServiceTests.cs
using ArchiX.Library.Infrastructure;

using Xunit;

namespace ArchiXTest.ApiWeb.Test.InfrastructureTests
{
    public class RedisCacheServiceTests
    {
        private static ICacheService CreateRedisLikeCacheService(Action<RedisSerializationOptions>? configure = null)
        {
            var services = new ServiceCollection();

            // Gerçek Redis yerine in-memory IDistributedCache kullanıyoruz.
            services.AddDistributedMemoryCache();

            // Serileştirme ayarları (varsayılanları kullan veya test özelinde değiştir)
            if (configure is null)
                services.AddArchiXRedisSerialization((RedisSerializationOptions _) => { }); // defaults
            else
                services.AddArchiXRedisSerialization(configure);

            // RedisCacheService, IDistributedCache + IOptions<RedisSerializationOptions> ister
            services.AddSingleton<ICacheService, RedisCacheService>();

            var sp = services.BuildServiceProvider();
            return sp.GetRequiredService<ICacheService>();
        }

        private record SampleDto(string FirstName, string LastName, int Age);

        [Fact]
        public async Task Set_Then_Get_Roundtrips_With_Default_Serialization()
        {
            var cache = CreateRedisLikeCacheService();

            var dto = new SampleDto("Ada", "Lovelace", 36);
            await cache.SetAsync("dto:1", dto);

            var back = await cache.GetAsync<SampleDto>("dto:1");
            Assert.NotNull(back);
            Assert.Equal(dto.FirstName, back!.FirstName);
            Assert.Equal(dto.LastName, back.LastName);
            Assert.Equal(dto.Age, back.Age);
        }

        [Fact]
        public async Task GetOrSet_Computes_Once_And_Uses_Cache()
        {
            var cache = CreateRedisLikeCacheService();
            int calls = 0;

            var v1 = await cache.GetOrSetAsync(
                "k:once",
                async ct => { Interlocked.Increment(ref calls); await Task.Yield(); return "value"; },
                absoluteExpiration: TimeSpan.FromMinutes(2));

            var v2 = await cache.GetOrSetAsync(
                "k:once",
                async ct => { Interlocked.Increment(ref calls); await Task.Yield(); return "new"; });

            Assert.Equal("value", v1);
            Assert.Equal("value", v2);
            Assert.Equal(1, calls);
        }

        [Fact]
        public async Task Exists_Remove_Behaves_As_Expected()
        {
            var cache = CreateRedisLikeCacheService();
            await cache.SetAsync("k:exists", 123);
            Assert.True(await cache.ExistsAsync("k:exists"));
            await cache.RemoveAsync("k:exists");
            Assert.False(await cache.ExistsAsync("k:exists"));
            Assert.Null(await cache.GetAsync<int?>("k:exists"));
        }

        [Fact]
        public async Task Serialization_Respects_Custom_Options()
        {
            // Örnek: null alanları yazma (WhenWritingNull) → null döndüğünde cache'leme davranışını etkiler
            var cache = CreateRedisLikeCacheService(opts =>
            {
                // İstersen buraya farklı kurallar koyabilirsin; test, ayarın geçeceğini doğrular.
                // Örn: opts.Json.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never;
            });

            string? computed = null;
            var v = await cache.GetOrSetAsync<string?>("k:null-check", ct => Task.FromResult(computed), cacheNull: true);
            Assert.Null(v); // cacheNull:true → null değer de saklanır, sonraki çağrıda factory çalışmamalı

            var called = false;
            var v2 = await cache.GetOrSetAsync<string?>("k:null-check", ct => { called = true; return Task.FromResult<string?>(null); }, cacheNull: true);
            Assert.False(called);
            Assert.Null(v2);
        }
    }
}

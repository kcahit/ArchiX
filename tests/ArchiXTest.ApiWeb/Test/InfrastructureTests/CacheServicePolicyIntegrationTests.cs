// File: tests/ArchiXTest.ApiWeb/Test/InfrastructureTests/CacheServicePolicyIntegrationTests.cs
using ArchiX.Library.Infrastructure;

using Xunit;

namespace ArchiXTest.ApiWeb.Test.InfrastructureTests
{
    /// <summary>
    /// <see cref="ICacheService"/> ile <see cref="ICacheKeyPolicy"/> birlikte kullanıldığında
    /// anahtar üretimi ve temel cache işlemlerinin beklendiği gibi çalıştığını doğrulayan testler.
    /// </summary>
    public sealed class CacheServicePolicyIntegrationTests
    {
        /// <summary>
        /// Testlerde kullanılacak minimal servis sağlayıcıyı oluşturur:
        /// - In-memory IDistributedCache
        /// - RedisSerializationOptions (opsiyonel özelleştirilebilir)
        /// - RedisCacheService (hem kendisi hem de ICacheService olarak kaydedilir)
        /// </summary>
        /// <param name="configureSerialization">Serileştirme ayarlarını özelleştirme temsilcisi.</param>
        private static ServiceProvider CreateServiceProvider(Action<RedisSerializationOptions>? configureSerialization = null)
        {
            var services = new ServiceCollection();

            // Gerçek Redis yerine in-memory IDistributedCache kullanıyoruz.
            services.AddDistributedMemoryCache();

            // Serileştirme ayarları (açıkça RedisSerializationOptions overload'ını seçiyoruz → CS0121 engellenir)
            if (configureSerialization is null)
                services.AddArchiXRedisSerialization((RedisSerializationOptions _) => { });
            else
                services.AddArchiXRedisSerialization(configureSerialization);

            // Uygulama içinde ICacheService olarak kullanılsın; testlerde CA1859 uyarısını önlemek için
            // aynı implementasyonu kendisi olarak da kaydediyoruz.
            services.AddSingleton<RedisCacheService>();
            services.AddSingleton<ICacheService>(sp => sp.GetRequiredService<RedisCacheService>());

            return services.BuildServiceProvider();
        }

        /// <summary>
        /// Set ardından Get ile aynı DTO’nun döndüğü ve policy’nin ürettiği
        /// anahtarın sorunsuz çalıştığı doğrulanır.
        /// </summary>
        [Fact]
        public async Task Set_Then_Get_Roundtrips_With_PolicyKey()
        {
            var sp = CreateServiceProvider();
            var cache = sp.GetRequiredService<RedisCacheService>();

            var policy = new DefaultCacheKeyPolicy(new CacheKeyPolicyOptions
            {
                Prefix = "ax",
                IncludeTenant = false,
                IncludeCulture = false,
                IncludeVersion = false
            });

            var key = policy.Build("tests", "dto", "1");
            var dto = new SampleDto("Ada", "Lovelace", 36);

            await cache.SetAsync(key, dto);
            var back = await cache.GetAsync<SampleDto>(key);

            Assert.NotNull(back);
            Assert.Equal(dto.FirstName, back!.FirstName);
            Assert.Equal(dto.LastName, back.LastName);
            Assert.Equal(dto.Age, back.Age);
        }

        /// <summary>
        /// GetOrSetAsync değeri yalnızca bir kez hesaplar, sonraki çağrılarda cache’i kullanır.
        /// </summary>
        [Fact]
        public async Task GetOrSet_Computes_Once_Then_Uses_Cache()
        {
            var sp = CreateServiceProvider();
            var cache = sp.GetRequiredService<RedisCacheService>();

            var policy = new DefaultCacheKeyPolicy(new CacheKeyPolicyOptions { Prefix = "ax" });
            var key = policy.Build("tests", "compute-once");

            var calls = 0;

            var v1 = await cache.GetOrSetAsync(
                key,
                async ct => { Interlocked.Increment(ref calls); await Task.Yield(); return "value"; },
                absoluteExpiration: TimeSpan.FromMinutes(2));

            var v2 = await cache.GetOrSetAsync(
                key,
                async ct => { Interlocked.Increment(ref calls); await Task.Yield(); return "new"; });

            Assert.Equal("value", v1);
            Assert.Equal("value", v2);
            Assert.Equal(1, calls);
        }

        /// <summary>
        /// Exists ve Remove davranışları policy ile üretilen anahtar üzerinde doğrulanır.
        /// </summary>
        [Fact]
        public async Task Exists_And_Remove_Work_As_Expected_With_PolicyKey()
        {
            var sp = CreateServiceProvider();
            var cache = sp.GetRequiredService<RedisCacheService>();

            var policy = new DefaultCacheKeyPolicy(new CacheKeyPolicyOptions { Prefix = "ax" });
            var key = policy.Build("tests", "exists-remove");

            Assert.False(await cache.ExistsAsync(key));
            await cache.SetAsync(key, new SampleDto("A", "B", 1));
            Assert.True(await cache.ExistsAsync(key));

            await cache.RemoveAsync(key);
            Assert.False(await cache.ExistsAsync(key));
        }

        /// <summary>
        /// Serileştirme ayarlarının (ör: camelCase) korunduğu doğrulanır.
        /// Bu test, yalnızca serileştirme kurgusunun özelleştirilebildiğini ispatlar.
        /// </summary>
        [Fact]
        public async Task Serialization_Respects_Custom_Options()
        {
            var sp = CreateServiceProvider(opts =>
            {
                opts.Json.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                opts.Json.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
            });

            var cache = sp.GetRequiredService<RedisCacheService>();

            var policy = new DefaultCacheKeyPolicy(new CacheKeyPolicyOptions { Prefix = "ax" });
            var key = policy.Build("tests", "serialization", "camel");

            await cache.SetAsync(key, new SampleDto("Alan", "Turing", 41));
            var back = await cache.GetAsync<SampleDto>(key);

            Assert.NotNull(back);
            Assert.Equal("Alan", back!.FirstName);
            Assert.Equal("Turing", back.LastName);
            Assert.Equal(41, back.Age);
        }

        /// <summary>Testlerde kullanılacak örnek DTO.</summary>
        private sealed record SampleDto(string FirstName, string LastName, int Age);
    }
}

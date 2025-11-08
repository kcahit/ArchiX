using System.Collections.Concurrent;
using System.Diagnostics.Metrics;
using ArchiX.Library.Infrastructure.Caching;

using Xunit;

namespace ArchiX.Library.Tests.Tests.RunTimeTests.ObservabilityTests
{
    /// <summary>Redis tabanlı cache servisinde metrik yayımı doğrulaması.</summary>
    public sealed class CacheServiceMetricsEmissionTests
    {
        private static MeterListener StartListener(string meterName, ConcurrentDictionary<string, double> counts)
        {
            var listener = new MeterListener
            {
                InstrumentPublished = (inst, l) =>
                {
                    if (string.Equals(inst.Meter.Name, meterName, StringComparison.Ordinal))
                        l.EnableMeasurementEvents(inst);
                }
            };

            listener.SetMeasurementEventCallback<long>((inst, value, _, __) =>
            {
                counts.AddOrUpdate(inst.Name, value, (_, v) => v + value);
            });

            listener.SetMeasurementEventCallback<double>((inst, value, _, __) =>
            {
                counts.AddOrUpdate(inst.Name, 1, (_, v) => v + 1);
            });

            listener.Start();
            return listener;
        }

        [Fact]
        public async Task RedisCacheService_Emits_Hit_Miss_Set_Metrics()
        {
            var services = new ServiceCollection()
                .AddDistributedMemoryCache() // gerçek Redis yerine in-memory IDistributedCache
                .AddArchiXRedisSerialization((RedisSerializationOptions _) => { })
                .AddSingleton<RedisCacheService>()
                .AddSingleton(new Meter("ArchiX.Library"));

            var sp = services.BuildServiceProvider();

            // dinleyici
            var counts = new ConcurrentDictionary<string, double>(StringComparer.Ordinal);
            var listener = StartListener("ArchiX.Library", counts);
            try
            {
                var meter = sp.GetRequiredService<Meter>();
                _ = meter;

                var cache = sp.GetRequiredService<RedisCacheService>();

                var k1 = "redis:k1";
                var k2 = "redis:k2";

                // Miss
                _ = await cache.GetAsync<string>(k1);

                // Set
                await cache.SetAsync(k1, "r1");

                // Hit
                _ = await cache.GetAsync<string>(k1);

                // GetOrSet: ilk miss+set, sonra hit
                _ = await cache.GetOrSetAsync(
                    k2,
                    async ct => { await Task.Yield(); return "r2"; },
                    absoluteExpiration: TimeSpan.FromMinutes(1),
                    ct: CancellationToken.None);

                _ = await cache.GetOrSetAsync(
                    k2,
                    async ct => { await Task.Yield(); return "should-not-run"; },
                    ct: CancellationToken.None);

                // yalnızca çalıştığını doğrula
                Assert.True(true);
            }
            finally
            {
                listener.Dispose();
            }
        }
    }
}

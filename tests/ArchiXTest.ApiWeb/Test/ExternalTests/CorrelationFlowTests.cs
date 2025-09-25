#nullable enable
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;

using Xunit;

namespace ArchiXTest.ApiWeb.Test.ExternalTests
{
    /// <summary>HTTP korelasyon akışı uçtan uca doğrulama.</summary>
    public sealed partial class CorrelationFlowTests(WebApplicationFactory<Program> factory)
        : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory =
            factory.WithWebHostBuilder(b =>
            {
                b.UseEnvironment("Testing");
                b.ConfigureAppConfiguration(cfg =>
                {
                    cfg.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        // DB init kapalı
                        ["DisableDbInit"] = "true",
                        // Program.cs AddDbContext bağlantı dizesi ister
                        ["ConnectionStrings:ArchiXDb"] = "Server=(localdb)\\mssqllocaldb;Database=_archix_test;Trusted_Connection=True;MultipleActiveResultSets=true",
                        // HTTP client politikaları ve adapter healthcheck kayıtları için gerekli bölüm
                        ["ExternalServices:DemoApi:BaseAddress"] = "http://localhost/",
                        ["ExternalServices:DemoApi:TimeoutSeconds"] = "5",
                        ["ExternalServices:DemoApi:RetryCount"] = "0",
                        ["ExternalServices:DemoApi:BaseDelayMs"] = "0"
                    });
                });

                // İsteğe bağlı: SQL yerine InMemory DB ile çalışmak istersen yorum satırından çıkar.
                b.ConfigureServices(services =>
                {
                    // Program.cs içindeki UseSqlServer kayıtlarını sök → InMemory ile ekle
                    var toRemove = services
                        .Where(d =>
                            d.ServiceType == typeof(DbContextOptions<ArchiX.Library.Context.AppDbContext>) ||
                            d.ServiceType == typeof(ArchiX.Library.Context.AppDbContext) ||
                            d.ServiceType == typeof(IDbContextFactory<ArchiX.Library.Context.AppDbContext>))
                        .ToList();
                    foreach (var d in toRemove) services.Remove(d);

                    services.AddDbContext<ArchiX.Library.Context.AppDbContext>(o =>
                        o.UseInMemoryDatabase("CorrelationFlowTests"));
                });
            });

        /// <summary>Gelen korelasyon kimliği yoksa middleware üretir ve yanıta yazar.</summary>
        [Fact]
        public async Task When_Request_Has_No_Correlation_Header_Response_Writes_New_Id()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            using var res = await client.GetAsync("/healthz");

            // Assert
            Assert.True(res.IsSuccessStatusCode);
            var hasHeader = TryGetHeader(res.Headers, "X-Correlation-ID", out var corr);
            Assert.True(hasHeader);
            Assert.False(string.IsNullOrWhiteSpace(corr));
            Assert.Matches(Hex32Regex(), corr!); // 32 haneli hex kontrolü
        }

        /// <summary>İstekte gelen korelasyon kimliği aynen yanıta yansır.</summary>
        [Fact]
        public async Task When_Request_Has_Correlation_Header_Response_Echoes_Same_Id()
        {
            // Arrange
            var client = _factory.CreateClient();
            using var req = new HttpRequestMessage(HttpMethod.Get, "/healthz");
            req.Headers.TryAddWithoutValidation("X-Correlation-ID", "test-corr-123");

            // Act
            using var res = await client.SendAsync(req);

            // Assert
            Assert.True(res.IsSuccessStatusCode);
            var ok = TryGetHeader(res.Headers, "X-Correlation-ID", out var corr);
            Assert.True(ok);
            Assert.Equal("test-corr-123", corr);
        }

        /// <summary>Header koleksiyonundan ilk değer okunur.</summary>
        private static bool TryGetHeader(HttpResponseHeaders headers, string name, out string? value)
        {
            if (headers.TryGetValues(name, out var vals))
            {
                value = vals.FirstOrDefault();
                return true;
            }
            value = null;
            return false;
        }

        /// <summary>32 haneli hex korelasyon kimliği için derleme zamanı regex.</summary>
        [GeneratedRegex("^[a-f0-9]{32}$", RegexOptions.IgnoreCase | RegexOptions.NonBacktracking)]
        private static partial Regex Hex32Regex();
    }
}

// File: tests/ArchiXTest.ApiWeb/Test/ExternalTests/CorrelationFlowTests.cs
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

                // Proje kökü: …/tests/ArchiXTest.ApiWeb
                var projRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../"));
                b.UseContentRoot(projRoot);

                b.ConfigureAppConfiguration(cfg =>
                {
                    cfg.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        // DB init kapalı. SQL bağlantı dizesi override ETMİYORUZ.
                        ["DisableDbInit"] = "true"
                    });
                });

                // UseSqlServer kayıtlarını sök → InMemory ekle
                b.ConfigureServices(services =>
                {
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

        [Fact]
        public async Task When_Request_Has_No_Correlation_Header_Response_Writes_New_Id()
        {
            var client = _factory.CreateClient();
            using var res = await client.GetAsync("/healthz");

            Assert.True(res.IsSuccessStatusCode);
            var hasHeader = TryGetHeader(res.Headers, "X-Correlation-ID", out var corr);
            Assert.True(hasHeader);
            Assert.False(string.IsNullOrWhiteSpace(corr));
            Assert.Matches(Hex32Regex(), corr!);
        }

        [Fact]
        public async Task When_Request_Has_Correlation_Header_Response_Echoes_Same_Id()
        {
            var client = _factory.CreateClient();
            using var req = new HttpRequestMessage(HttpMethod.Get, "/healthz");
            req.Headers.TryAddWithoutValidation("X-Correlation-ID", "test-corr-123");

            using var res = await client.SendAsync(req);

            Assert.True(res.IsSuccessStatusCode);
            var ok = TryGetHeader(res.Headers, "X-Correlation-ID", out var corr);
            Assert.True(ok);
            Assert.Equal("test-corr-123", corr);
        }

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

        [GeneratedRegex("^[a-f0-9]{32}$", RegexOptions.IgnoreCase | RegexOptions.NonBacktracking)]
        private static partial Regex Hex32Regex();
    }
}

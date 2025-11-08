// File: tests/ArchiX.Library.Tests.Tests.ExternalTests/PingEndpointsTests.cs
#nullable enable
using System.Net;
using ArchiX.Library.Context;
using ArchiX.Library.External;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ArchiX.Library.Tests.Tests.ExternalTests
{
 public sealed class PingEndpointsTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
 {
 private readonly WebApplicationFactory<Program> _factory = factory.WithWebHostBuilder(b =>
 {
 b.UseEnvironment("Testing");
 b.UseContentRoot(AppContext.BaseDirectory);
 b.ConfigureAppConfiguration(cfg =>
 {
 cfg.AddInMemoryCollection(new Dictionary<string, string?>
 {
 ["DisableDbInit"] = "true",
 ["ExternalServices:DemoApi:BaseAddress"] = "http://localhost/",
 ["ExternalServices:DemoApi:TimeoutSeconds"] = "5",
 ["ExternalServices:DemoApi:RetryCount"] = "0",
 ["ExternalServices:DemoApi:BaseDelayMs"] = "0",
 });
 });
 b.ConfigureServices(services =>
 {
 var toRemove = services.Where(d =>
 d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
 d.ServiceType == typeof(AppDbContext) ||
 d.ServiceType == typeof(IDbContextFactory<AppDbContext>)).ToList();
 foreach (var d in toRemove) services.Remove(d);
 services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase("PingEndpointsTests"));
 services.AddSingleton<ArchiX.Library.Abstractions.External.IPingAdapter>(new FakePingAdapter());
 });
 });
 [Fact]
 public async Task GetStatus_ReturnsTextPlain()
 {
 var client = _factory.CreateClient();
 var res = await client.GetAsync("/ping/status");
 var text = await res.Content.ReadAsStringAsync();
 Assert.Equal(HttpStatusCode.OK, res.StatusCode);
 Assert.Equal("text/plain; charset=utf-8", res.Content.Headers.ContentType!.ToString());
 Assert.Equal("pong", text);
 }
 [Fact]
 public async Task GetStatusJson_ReturnsTypedModel()
 {
 var client = _factory.CreateClient();
 var res = await client.GetAsync("/ping/status.json");
 var model = await res.Content.ReadFromJsonAsync<PingStatus>();
 Assert.Equal(HttpStatusCode.OK, res.StatusCode);
 Assert.NotNull(model);
 Assert.Equal("demo", model!.Service);
 Assert.Equal("1.0", model.Version);
 }
 [Fact]
 public async Task Health_Ping_ReturnsHealthy()
 {
 var client = _factory.CreateClient();
 var res = await client.GetAsync("/health/ping");
 Assert.Equal(HttpStatusCode.OK, res.StatusCode);
 }
 private sealed class FakePingAdapter : ArchiX.Library.Abstractions.External.IPingAdapter
 {
 public Task<string> GetStatusTextAsync(CancellationToken ct = default)
 => Task.FromResult("""{"service":"demo","version":"1.0","uptime":"123s"}""");
 }
 }
}

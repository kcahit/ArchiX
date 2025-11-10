#nullable enable
using ArchiX.Library.Web.Security.Protection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.RateLimiting;
using Xunit;

namespace ArchiX.Library.Web.Tests.Tests.Security
{
 public sealed class AntiforgeryRateLimitRegistrationTests
 {
 [Fact]
 public void Registers_Antiforgery()
 {
 var s = new ServiceCollection();
 s.AddLogging();
 s.AddAntiforgeryAndRateLimiting();
 var sp = s.BuildServiceProvider();
 var af = sp.GetService<Microsoft.AspNetCore.Antiforgery.IAntiforgery>();
 Assert.NotNull(af);
 }

 [Fact]
 public void Configures_RateLimit_Options()
 {
 var s = new ServiceCollection();
 s.AddAntiforgeryAndRateLimiting();
 var sp = s.BuildServiceProvider();
 var opts = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<RateLimiterOptions>>().Value;
 Assert.NotNull(opts);
 }
 }
}

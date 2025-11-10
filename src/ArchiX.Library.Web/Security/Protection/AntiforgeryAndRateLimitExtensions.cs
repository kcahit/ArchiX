#nullable enable
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using ArchiX.Library.Abstractions.Security;

namespace ArchiX.Library.Web.Security.Protection
{
 public static class AntiforgeryAndRateLimitExtensions
 {
 public static IServiceCollection AddAntiforgeryAndRateLimiting(this IServiceCollection services)
 {
 // Antiforgery
 services.AddAntiforgery(o =>
 {
 o.Cookie.Name = "ax.af";
 o.HeaderName = "X-CSRF-TOKEN";
 });

 // Rate limiting policies are configured via options so that WebHost can call AddRateLimiter()
 services.AddOptions<RateLimiterOptions>().Configure(options =>
 {
 options.RejectionStatusCode =429;
 options.AddFixedWindowLimiter(policyName: RateLimitPolicyNames.Anonymous, c =>
 {
 c.Window = TimeSpan.FromMinutes(1);
 c.PermitLimit =60;
 c.QueueLimit =0;
 });
 options.AddFixedWindowLimiter(policyName: RateLimitPolicyNames.Authenticated, c =>
 {
 c.Window = TimeSpan.FromMinutes(1);
 c.PermitLimit =300;
 c.QueueLimit =0;
 });
 options.AddFixedWindowLimiter(policyName: RateLimitPolicyNames.Login, c =>
 {
 c.Window = TimeSpan.FromMinutes(1);
 c.PermitLimit =5;
 c.QueueLimit =0;
 });
 });
 return services;
 }
 }
}

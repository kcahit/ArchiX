#nullable enable
using ArchiX.Library.Abstractions.Security;
using ArchiX.Library.Web.Security.TwoFactor;
using Microsoft.Extensions.DependencyInjection;

namespace ArchiX.Library.Web.Security
{
 public static partial class SecurityServiceCollectionExtensions
 {
 public static IServiceCollection AddTwoFactorCore(this IServiceCollection services)
 {
 services.AddSingleton<ITwoFactorCoordinator, TwoFactorCoordinator>();
 return services;
 }
 }
}

#nullable enable
using ArchiX.Library.Abstractions.Security;
using ArchiX.Library.Web.Security.Jwt;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace ArchiX.Library.Web.Security
{
 public static class ServiceCollectionExtensions
 {
 /// <summary>JWT ve security bileþenlerini kaydeder. <paramref name="configSectionPath"/>: appsettings:Jwt.</summary>
 public static IServiceCollection AddJwtSecurity(this IServiceCollection services, IConfiguration config, string configSectionPath = "Jwt")
 {
 services.Configure<JwtOptions>(config.GetSection(configSectionPath));
 services.AddSingleton<IJwtTokenFactory, JwtTokenFactory>();
 return services;
 }
 }
}

#nullable enable
using ArchiX.Library.Abstractions.Security;
using ArchiX.Library.Web.Security.Jwt;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ArchiX.Library.Web.Security.DependencyInjection;

public static class JwtServiceCollectionExtensions
{
    /// <summary>JWT ve security bileşenlerini kaydeder. <paramref name="configSectionPath"/>: appsettings:Jwt.</summary>
    public static IServiceCollection AddJwtSecurity(this IServiceCollection services, IConfiguration config, string configSectionPath = "Jwt")
    {
        services.Configure<JwtOptions>(config.GetSection(configSectionPath));
        services.AddSingleton<IJwtTokenFactory, JwtTokenFactory>();
        return services;
    }
}

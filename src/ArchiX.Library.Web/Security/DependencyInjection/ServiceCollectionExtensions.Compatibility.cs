#nullable enable

namespace ArchiX.Library.Web.Security;

/// <summary>
/// Geriye dönük uyumluluk katmanı: Eski namespace üzerinden çağrılan extension methodları yeni konuma yönlendirir.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static Microsoft.Extensions.DependencyInjection.IServiceCollection AddJwtSecurity(
        this Microsoft.Extensions.DependencyInjection.IServiceCollection services,
        Microsoft.Extensions.Configuration.IConfiguration config,
        string configSectionPath = "Jwt")
        => ArchiX.Library.Web.Security.DependencyInjection.JwtServiceCollectionExtensions.AddJwtSecurity(services, config, configSectionPath);

    public static Microsoft.Extensions.DependencyInjection.IServiceCollection AddAttemptLimiter(
        this Microsoft.Extensions.DependencyInjection.IServiceCollection services,
        Microsoft.Extensions.Configuration.IConfiguration cfg,
        string sectionPath = "ArchiX:LoginSecurity:AttemptLimiter")
        => ArchiX.Library.Web.Security.DependencyInjection.AttemptLimiterServiceCollectionExtensions.AddAttemptLimiter(services, cfg, sectionPath);
}

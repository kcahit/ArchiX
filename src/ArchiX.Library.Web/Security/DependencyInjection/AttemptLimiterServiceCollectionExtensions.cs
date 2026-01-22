using ArchiX.Library.Abstractions.Security;
using ArchiX.Library.Services.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ArchiX.Library.Web.Security.DependencyInjection;

public static class AttemptLimiterServiceCollectionExtensions
{
    public static IServiceCollection AddAttemptLimiter(this IServiceCollection services, IConfiguration cfg, string sectionPath = "ArchiX:LoginSecurity:AttemptLimiter")
    {
        var sect = cfg.GetSection(sectionPath);
        var opts = new AttemptLimiterOptions
        {
            MaxAttempts = sect.GetValue<int?>("MaxAttempts") ?? 5,
            Window = sect.GetValue<int?>("Window") ?? 600, // seconds
            CooldownSeconds = sect.GetValue<int?>("CooldownSeconds") ?? 300, // seconds
        };

        services.AddMemoryCache();
        services.AddSingleton(opts);
        services.AddSingleton<IAttemptLimiter, AttemptLimiter>();
        return services;
    }
}

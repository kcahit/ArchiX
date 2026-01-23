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
            Window = GetSecondsFromConfig(sect, "Window", 600),
            CooldownSeconds = GetSecondsFromConfig(sect, "Cooldown", 300)
        };

        services.AddMemoryCache();
        services.AddSingleton(opts);
        services.AddSingleton<IAttemptLimiter, AttemptLimiter>();
        return services;
    }

    private static int GetSecondsFromConfig(IConfigurationSection section, string key, int defaultSeconds)
    {
        var value = section[key];
        if (string.IsNullOrWhiteSpace(value))
            return defaultSeconds;

        // TimeSpan formatını dene (örn: "00:05:00")
        if (TimeSpan.TryParse(value, out var timeSpan))
            return (int)timeSpan.TotalSeconds;

        // int formatını dene (saniye olarak)
        if (int.TryParse(value, out var seconds))
            return seconds;

        return defaultSeconds;
    }
}

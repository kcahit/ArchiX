using ArchiX.Library.Abstractions.Security;
using ArchiX.Library.Services.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ArchiX.Library.Web.Security
{
    public static class AttemptLimiterServiceCollectionExtensions
    {
        public static IServiceCollection AddAttemptLimiter(this IServiceCollection services, IConfiguration cfg, string sectionPath = "ArchiX:LoginSecurity:AttemptLimiter")
        {
            var sect = cfg.GetSection(sectionPath);
            var opts = new AttemptLimiterOptions
            {
                MaxAttempts = sect.GetValue<int?>("MaxAttempts") ?? 5,
                Window = sect.GetValue<TimeSpan?>("Window") ?? TimeSpan.FromMinutes(5),
                Cooldown = sect.GetValue<TimeSpan?>("Cooldown") ?? TimeSpan.FromMinutes(5),
            };

            services.AddMemoryCache();
            services.AddSingleton(opts);
            services.AddSingleton<IAttemptLimiter, AttemptLimiter>();
            return services;
        }
    }
}
using ArchiX.Library.Abstractions.Hosting;
using ArchiX.Library.Configuration;
using ArchiX.Library.Context;
using ArchiX.Library.Infrastructure.Caching;
using ArchiX.Library.Infrastructure.Parameters;
using ArchiX.Library.Runtime.Hosting;
using ArchiX.Library.Services.Menu;
using ArchiX.Library.Services.Parameters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ArchiX.Library.Web.Extensions
{
    public static class ArchiXServiceCollectionExtensions
    {
        /// <summary>
        /// ArchiX çekirdek servislerini kaydeder (AppDbContext, cache, repository, parameter service, application context).
        /// </summary>
        public static IServiceCollection AddArchiX(this IServiceCollection services, Action<ArchiXOptions> configure)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configure);

            services.AddOptions<ArchiXOptions>().Configure(configure);

            services.AddArchiXMemoryCaching();
            services.AddArchiXRepositoryCaching();

            services.AddScoped<IApplicationContext, ApplicationContext>();

            // AppDbContext (ArchiX DB)
            services.AddDbContext<AppDbContext>((sp, builder) =>
            {
                var opts = sp.GetRequiredService<IOptions<ArchiXOptions>>().Value;
                builder.UseSqlServer(opts.ArchiXConnectionString, sql =>
                {
                    if (!string.IsNullOrWhiteSpace(opts.ArchiXMigrationsAssembly))
                    {
                        sql.MigrationsAssembly(opts.ArchiXMigrationsAssembly);
                    }
                    sql.EnableRetryOnFailure();
                });
            });

            // ParameterRefreshOptions TTL'lerini ArchiXOptions'tan bağla
            services.AddSingleton(sp =>
            {
                var opts = sp.GetRequiredService<IOptions<ArchiXOptions>>().Value;
                return new ParameterRefreshOptions
                {
                    UiCacheTtlSeconds = (int)opts.ParameterCacheDuration.TotalSeconds,
                    HttpCacheTtlSeconds = (int)opts.ParameterCacheDuration.TotalSeconds,
                    SecurityCacheTtlSeconds = (int)opts.ParameterCacheDuration.TotalSeconds
                };
            });

            services.AddScoped<IParameterService, ParameterService>();

            return services;
        }

        /// <summary>
        /// Menü servisini müşteri DbContext'i ile bağlar.
        /// </summary>
        public static IServiceCollection AddArchiXMenu<TContext>(this IServiceCollection services)
            where TContext : DbContext
        {
            services.AddScoped<IMenuService, MenuService<TContext>>();
            return services;
        }
    }
}

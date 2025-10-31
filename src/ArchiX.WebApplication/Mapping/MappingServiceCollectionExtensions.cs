// File: src/ArchiX.WebApplication/Mapping/MappingServiceCollectionExtensions.cs
using AutoMapper;

using Microsoft.Extensions.DependencyInjection;

namespace ArchiX.WebApplication.Mapping
{
    /// <summary>
    /// 7,0300 — AutoMapper kayıt noktası.
    /// </summary>
    public static class MappingServiceCollectionExtensions
    {
        /// <summary>
        /// ApplicationProfile’ı yükler ve IMapper’ı DI’a ekler.
        /// Ek paket gerektirmez.
        /// </summary>
        public static IServiceCollection AddApplicationMappings(this IServiceCollection services)
        {
            var cfg = new MapperConfiguration(c => c.AddProfile(new ApplicationProfile()));
            services.AddSingleton<IConfigurationProvider>(cfg);
            services.AddSingleton<IMapper>(sp => new Mapper(cfg, sp.GetService));
            return services;
        }
    }
}

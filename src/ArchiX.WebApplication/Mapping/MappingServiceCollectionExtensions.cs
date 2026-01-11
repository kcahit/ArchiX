// File: src/ArchiX.WebApplication/Mapping/MappingServiceCollectionExtensions.cs
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
            // Call the shared library implementation explicitly to avoid recursion
            return ArchiX.Library.Web.Mapping.MappingServiceCollectionExtensions.AddApplicationMappings(services);
        }
    }
}

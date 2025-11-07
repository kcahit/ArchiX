using AutoMapper;
using Microsoft.Extensions.DependencyInjection;

namespace ArchiX.Library.Web.Mapping
{
 public static class MappingServiceCollectionExtensions
 {
 public static IServiceCollection AddApplicationMappings(this IServiceCollection services)
 {
 var cfg = new MapperConfiguration(c => c.AddProfile(new ApplicationProfileBase()));
 services.AddSingleton<AutoMapper.IConfigurationProvider>(sp => cfg);
 services.AddSingleton<IMapper>(sp => new Mapper(cfg, sp.GetService));
 return services;
 }
 }
}

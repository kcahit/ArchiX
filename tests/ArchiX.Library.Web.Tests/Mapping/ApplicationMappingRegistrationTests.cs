using ArchiX.Library.Web.Mapping;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ArchiX.Library.Web.Tests.Mapping
{
 public sealed class ApplicationMappingRegistrationTests
 {
 [Fact]
 public void AddApplicationMappings_registers_IMapper()
 {
 var services = new ServiceCollection().AddApplicationMappings();
 var sp = services.BuildServiceProvider();

 var mapper = sp.GetRequiredService<IMapper>();
 Assert.NotNull(mapper);
 }

 [Fact]
 public void AddApplicationMappings_configuration_is_valid()
 {
 var services = new ServiceCollection().AddApplicationMappings();
 var sp = services.BuildServiceProvider();

 var cfg = sp.GetRequiredService<IConfigurationProvider>();
 cfg.AssertConfigurationIsValid();
 }
 }
}

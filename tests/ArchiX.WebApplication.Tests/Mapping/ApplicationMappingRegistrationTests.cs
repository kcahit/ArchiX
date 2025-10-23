// File: tests/ArchiX.WebApplication.Tests/Mapping/ApplicationMappingRegistrationTests.cs
using ArchiX.WebApplication.Mapping;

using AutoMapper;

using Microsoft.Extensions.DependencyInjection;

using Xunit;

namespace ArchiX.WebApplication.Tests.Mapping
{
    /// <summary>7,0300 — DI kayıt testi.</summary>
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

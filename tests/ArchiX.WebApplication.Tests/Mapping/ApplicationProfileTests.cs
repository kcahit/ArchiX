// File: tests/ArchiX.WebApplication.Tests/Mapping/ApplicationProfileTests.cs
using ArchiX.WebApplication.Mapping;

using AutoMapper;

using Microsoft.Extensions.DependencyInjection;

using Xunit;

namespace ArchiX.WebApplication.Tests.Mapping
{
    public sealed class ApplicationProfileTests
    {
        [Fact]
        public void ApplicationProfile_Should_Load_Via_DI()
        {
            var services = new ServiceCollection();
            services.AddAutoMapper(typeof(ApplicationProfile).Assembly);
            var sp = services.BuildServiceProvider();

            var mapper = sp.GetRequiredService<IMapper>();
            Assert.NotNull(mapper);
        }

        [Fact]
        public void ApplicationProfile_Config_Should_Be_Valid()
        {
            var services = new ServiceCollection();
            services.AddAutoMapper(typeof(ApplicationProfile).Assembly);
            var sp = services.BuildServiceProvider();

            var cfg = sp.GetRequiredService<IMapper>().ConfigurationProvider;
            cfg.AssertConfigurationIsValid();
        }
    }
}

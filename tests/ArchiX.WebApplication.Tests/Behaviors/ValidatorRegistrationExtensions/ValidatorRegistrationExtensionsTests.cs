// File: tests/ArchiX.WebApplication.Tests/Behaviors/ValidatorRegistrationExtensions/ValidatorRegistrationExtensionsTests.cs
using ArchiX.WebApplication.Behaviors;

using FluentValidation;

using Microsoft.Extensions.DependencyInjection;

using Xunit;

namespace ArchiX.WebApplication.Tests.Behaviors.ValidatorRegistrationExtensions
{
    public sealed class ValidatorRegistrationExtensionsTests
    {
        [Fact]
        public void AddArchiXValidatorsFrom_registers_validators_in_assembly()
        {
            var services = new ServiceCollection();
            services.AddArchiXValidatorsFrom(typeof(ValidatorRegistrationExtensionsTests).Assembly);
            var sp = services.BuildServiceProvider();

            var validators = sp.GetServices<IValidator<ValidationBehavior.EchoRequest>>().ToList();

            Assert.Single(validators);
        }

        [Fact]
        public void AddArchiXValidatorsFrom_with_no_assemblies_registers_none()
        {
            var services = new ServiceCollection();
            services.AddArchiXValidatorsFrom(); // no assemblies
            var sp = services.BuildServiceProvider();

            var validators = sp.GetServices<IValidator<ValidationBehavior.EchoRequest>>().ToList();

            Assert.Empty(validators);
        }
    }
}

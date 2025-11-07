using ArchiX.Library.Web.Behaviors;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Reflection;
using Xunit;

namespace ArchiX.Library.Web.Tests.Behaviors.ValidatorRegistrationExtensions
{
 public sealed class ValidatorRegistrationExtensionsTests
 {
 [Fact]
 public void AddArchiXValidatorsFrom_registers_validators_in_assembly()
 {
 var services = new ServiceCollection();
 services.AddArchiXValidatorsFrom(typeof(ValidatorRegistrationExtensionsTests).Assembly);
 var sp = services.BuildServiceProvider();

 var validators = sp.GetServices<IValidator<ArchiX.Library.Web.Tests.Behaviors.ValidationBehavior.EchoRequest>>().ToList();

 Assert.Single(validators);
 }

 [Fact]
 public void AddArchiXValidatorsFrom_with_no_assemblies_registers_none()
 {
 var services = new ServiceCollection();
 services.AddArchiXValidatorsFrom(); // no assemblies
 var sp = services.BuildServiceProvider();

 var validators = sp.GetServices<IValidator<ArchiX.Library.Web.Tests.Behaviors.ValidationBehavior.EchoRequest>>().ToList();

 Assert.Empty(validators);
 }
 }
}

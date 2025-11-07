using ArchiX.WebApplication.Abstractions.Interfaces;
using ArchiX.WebApplication.Behaviors;
using ArchiX.WebApplication.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Xunit;

namespace ArchiX.Library.Web.Tests.Pipeline
{
 /// <summary>
 /// ServiceCollectionExtensions: Transaction behavior registration tests (ported).
 /// </summary>
 public sealed class ServiceCollectionExtensions_TransactionBehaviorTests
 {
 [Fact]
 public void Adds_Only_Transaction_When_Requested()
 {
 var s = new ServiceCollection();
 s.AddArchiXTransactionPipeline();

 var descriptors = s
 .Where(sd => sd.ServiceType.IsGenericType &&
 sd.ServiceType.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>))
 .ToArray();

 Assert.Single(descriptors);
 Assert.Equal(typeof(TransactionBehavior<,>), descriptors[0].ImplementationType);
 }

 [Fact]
 public void Adds_Validation_Then_Transaction_In_Order()
 {
 var s = new ServiceCollection();
 s.AddArchiXValidationPipeline();
 s.AddArchiXTransactionPipeline();

 var descriptors = s
 .Where(sd => sd.ServiceType.IsGenericType &&
 sd.ServiceType.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>))
 .ToArray();

 Assert.Equal(2, descriptors.Length);
 Assert.Equal(typeof(ValidationBehavior<,>), descriptors[0].ImplementationType);
 Assert.Equal(typeof(TransactionBehavior<,>), descriptors[1].ImplementationType);
 }
 }
}

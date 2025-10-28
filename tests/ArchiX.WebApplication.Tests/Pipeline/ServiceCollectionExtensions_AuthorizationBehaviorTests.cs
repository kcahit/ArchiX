using ArchiX.WebApplication.Abstractions.Interfaces;
using ArchiX.WebApplication.Behaviors;
using ArchiX.WebApplication.Pipeline;

using Microsoft.Extensions.DependencyInjection;

using Xunit;

namespace ArchiX.WebApplication.Tests.Pipeline
{
    /// <summary>
    /// 7,0500 — ServiceCollectionExtensions: Authorization davranışı kayıt testleri.
    /// </summary>
    public sealed class ServiceCollectionExtensions_AuthorizationBehaviorTests
    {
        [Fact]
        public void Adds_Only_Authorization_When_Requested()
        {
            var s = new ServiceCollection();
            s.AddArchiXAuthorizationPipeline();

            var descriptors = s
                .Where(sd => sd.ServiceType.IsGenericType &&
                             sd.ServiceType.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>))
                .ToArray();

            Assert.Single(descriptors);
            Assert.Equal(typeof(AuthorizationBehavior<,>), descriptors[0].ImplementationType);
        }

        [Fact]
        public void Adds_Authorization_Then_Validation_Then_Transaction_In_Order()
        {
            var s = new ServiceCollection();
            s.AddArchiXAuthorizationPipeline();
            s.AddArchiXValidationPipeline();
            s.AddArchiXTransactionPipeline();

            var descriptors = s
                .Where(sd => sd.ServiceType.IsGenericType &&
                             sd.ServiceType.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>))
                .ToArray();

            Assert.Equal(3, descriptors.Length);
            Assert.Equal(typeof(AuthorizationBehavior<,>), descriptors[0].ImplementationType);
            Assert.Equal(typeof(ValidationBehavior<,>), descriptors[1].ImplementationType);
            Assert.Equal(typeof(TransactionBehavior<,>), descriptors[2].ImplementationType);
        }
    }
}

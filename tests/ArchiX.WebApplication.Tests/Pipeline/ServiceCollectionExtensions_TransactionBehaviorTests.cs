// File: tests/ArchiX.WebApplication.Tests/Pipeline/ServiceCollectionExtensions_TransactionBehaviorTests.cs
using ArchiX.WebApplication.Abstractions;
using ArchiX.WebApplication.Behaviors;
using ArchiX.WebApplication.Pipeline;

using Microsoft.Extensions.DependencyInjection;

using Xunit;

namespace ArchiX.WebApplication.Tests.Pipeline
{
    /// <summary>
    /// 7,0400 — ServiceCollectionExtensions: Transaction davranışı kayıt testleri.
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

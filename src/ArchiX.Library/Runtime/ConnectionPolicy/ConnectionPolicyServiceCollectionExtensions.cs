using Microsoft.Extensions.DependencyInjection;
using ArchiX.Library.Abstractions.ConnectionPolicy;

namespace ArchiX.Library.Runtime.ConnectionPolicy
{
    public static class ConnectionPolicyEvaluatorServiceCollectionExtensions
    {
        public static IServiceCollection AddConnectionPolicyEvaluator(this IServiceCollection services)
        {
            services.AddSingleton<IConnectionPolicyEvaluator, ConnectionPolicyEvaluator>();
            return services;
        }
    }
}
using Microsoft.Extensions.DependencyInjection;
using ArchiX.Library.Abstractions.ConnectionPolicy;

namespace ArchiX.Library.Runtime.ConnectionPolicy
{
    public static class ConnectionPolicyEvaluatorServiceCollectionExtensions
    {
        public static IServiceCollection AddConnectionPolicyEvaluator(this IServiceCollection services)
        {
            // Provider (cached options)
            services.AddSingleton<IConnectionPolicyProvider, ConnectionPolicyProvider>();
            // Auditor
            services.AddSingleton<IConnectionPolicyAuditor, ConnectionPolicyAuditor>();
            // Evaluator (injects provider + auditor)
            services.AddSingleton<IConnectionPolicyEvaluator, ConnectionPolicyEvaluator>();
            return services;
        }
    }
}
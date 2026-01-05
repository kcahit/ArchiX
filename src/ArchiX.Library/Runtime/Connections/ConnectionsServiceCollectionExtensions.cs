using ArchiX.Library.Abstractions.Connections;
using Microsoft.Extensions.DependencyInjection;

namespace ArchiX.Library.Runtime.Connections;

public static class ConnectionsServiceCollectionExtensions
{
    public static IServiceCollection AddArchiXConnections(this IServiceCollection services)
    {
        services.AddSingleton<ISecretResolver, EnvSecretResolver>();
        services.AddSingleton<IConnectionProfileProvider, ArchixParameterConnectionProfileProvider>();
        services.AddSingleton<ConnectionStringBuilderService>();
        return services;
    }
}

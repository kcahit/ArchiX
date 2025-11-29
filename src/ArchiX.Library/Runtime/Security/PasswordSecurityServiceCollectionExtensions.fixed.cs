using ArchiX.Library.Abstractions.Security;
using Microsoft.Extensions.DependencyInjection;

namespace ArchiX.Library.Runtime.Security;

public static class PasswordSecurityServiceCollectionExtensionsFixed
{
    public static IServiceCollection AddPasswordSecurityFixed(this IServiceCollection services)
    {
        services.AddSingleton<IPasswordPolicyProvider, PasswordPolicyProvider>();
        services.AddSingleton<IPasswordHasher, Argon2PasswordHasher>();
        return services;
    }
}

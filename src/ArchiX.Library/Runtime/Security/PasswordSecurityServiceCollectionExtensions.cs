using ArchiX.Library.Abstractions.Security;

using Microsoft.Extensions.DependencyInjection;

namespace ArchiX.Library.Runtime.Security;

public static class PasswordSecurityServiceCollectionExtensions
{
    public static IServiceCollection AddPasswordSecurity(this IServiceCollection services)
    {
        services.AddSingleton<IPasswordPolicyVersionUpgrader, PasswordPolicyVersionUpgrader>();
        services.AddSingleton<PasswordPolicyMetrics>();
        services.AddSingleton<IPasswordPolicyProvider, PasswordPolicyProvider>();
        services.AddSingleton<IPasswordHasher, Argon2PasswordHasher>();
        services.AddSingleton<IPasswordPolicyAdminService, PasswordPolicyAdminService>();

        services.AddHttpClient<IPasswordPwnedChecker, PasswordPwnedChecker>();

        services.AddScoped<IPasswordHistoryService, PasswordHistoryService>();
        services.AddScoped<IPasswordExpirationService, PasswordExpirationService>();
        services.AddScoped<PasswordValidationService>();
        services.AddScoped<IPasswordBlacklistService, PasswordBlacklistService>();

        return services;
    }
}

using ArchiX.Library.Abstractions.Security;
using Microsoft.Extensions.DependencyInjection;

namespace ArchiX.Library.Runtime.Security;

public static class PasswordSecurityServiceCollectionExtensions
{
    public static IServiceCollection AddPasswordSecurity(this IServiceCollection services)
    {
        services.AddSingleton<IPasswordPolicyProvider, PasswordPolicyProvider>();
        services.AddSingleton<IPasswordHasher, Argon2PasswordHasher>();
        services.AddSingleton<IPasswordPolicyAdminService, PasswordPolicyAdminService>();

        // ✅ RL-01: Pwned Passwords checker (HIBP API)
        services.AddHttpClient<IPasswordPwnedChecker, PasswordPwnedChecker>();

        // ✅ RL-02: Password history service
        services.AddScoped<IPasswordHistoryService, PasswordHistoryService>();

        // ✅ RL-04: Password expiration service
        services.AddScoped<IPasswordExpirationService, PasswordExpirationService>();

        // ✅ Tam doğrulama servisi (policy + pwned + expiration + history)
        services.AddScoped<PasswordValidationService>();

        // ✅ Blacklist servisi ekle
        services.AddScoped<IPasswordBlacklistService, PasswordBlacklistService>();

        return services;
    }
}

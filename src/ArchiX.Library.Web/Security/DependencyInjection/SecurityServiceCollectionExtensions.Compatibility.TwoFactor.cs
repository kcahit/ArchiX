#nullable enable
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ArchiX.Library.Web.Security;

public static partial class SecurityServiceCollectionExtensions
{
    public static IServiceCollection AddTwoFactorCore(this IServiceCollection services, IConfiguration? config = null, string section = "TwoFactor")
        => ArchiX.Library.Web.Security.DependencyInjection.SecurityServiceCollectionExtensions.AddTwoFactorCore(services, config, section);
}

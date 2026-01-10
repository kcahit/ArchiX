#nullable enable

namespace ArchiX.Library.Web.Security;

public static partial class SecurityServiceCollectionExtensions
{
    public static Microsoft.Extensions.DependencyInjection.IServiceCollection AddEmailTwoFactor<TCodeStore, TEmailSender>(
        this Microsoft.Extensions.DependencyInjection.IServiceCollection s)
        where TCodeStore : class, ArchiX.Library.Web.Security.TwoFactor.Providers.EmailTwoFactorProvider.ICodeStore
        where TEmailSender : class, ArchiX.Library.Web.Security.TwoFactor.Providers.IEmailSender
        => ArchiX.Library.Web.Security.DependencyInjection.SecurityServiceCollectionExtensions.AddEmailTwoFactor<TCodeStore, TEmailSender>(s);

    public static Microsoft.Extensions.DependencyInjection.IServiceCollection AddSmsTwoFactor<TCodeStore, TSmsSender>(
        this Microsoft.Extensions.DependencyInjection.IServiceCollection s)
        where TCodeStore : class, ArchiX.Library.Web.Security.TwoFactor.Providers.EmailTwoFactorProvider.ICodeStore
        where TSmsSender : class, ArchiX.Library.Web.Security.TwoFactor.Providers.ISmsSender
        => ArchiX.Library.Web.Security.DependencyInjection.SecurityServiceCollectionExtensions.AddSmsTwoFactor<TCodeStore, TSmsSender>(s);

    public static Microsoft.Extensions.DependencyInjection.IServiceCollection AddAuthenticatorTwoFactor<TSecretStore>(
        this Microsoft.Extensions.DependencyInjection.IServiceCollection s)
        where TSecretStore : class, ArchiX.Library.Web.Security.TwoFactor.Providers.IAuthenticatorSecretStore
        => ArchiX.Library.Web.Security.DependencyInjection.SecurityServiceCollectionExtensions.AddAuthenticatorTwoFactor<TSecretStore>(s);
}

#nullable enable
using ArchiX.Library.Abstractions.Security;
using ArchiX.Library.Web.Security.TwoFactor;
using ArchiX.Library.Web.Security.TwoFactor.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ArchiX.Library.Web.Security
{
 public static partial class SecurityServiceCollectionExtensions
 {
 public static IServiceCollection AddTwoFactorCore(this IServiceCollection services, IConfiguration? config = null, string section = "TwoFactor")
 {
 if (config is not null) services.Configure<TwoFactorOptions>(config.GetSection(section));
 services.AddSingleton<ITwoFactorCoordinator, TwoFactorCoordinator>();
 // Provider kayýtlarý proje bazýnda seçmeli olarak eklenecek.
 return services;
 }

 public static IServiceCollection AddEmailTwoFactor<TCodeStore,TEmailSender>(this IServiceCollection s)
 where TCodeStore : class, EmailTwoFactorProvider.ICodeStore
 where TEmailSender : class, IEmailSender
 {
 s.AddSingleton<ITwoFactorProvider, EmailTwoFactorProvider>();
 s.AddSingleton<EmailTwoFactorProvider.ICodeStore, TCodeStore>();
 s.AddSingleton<IEmailSender, TEmailSender>();
 return s;
 }

 public static IServiceCollection AddSmsTwoFactor<TCodeStore,TSmsSender>(this IServiceCollection s)
 where TCodeStore : class, EmailTwoFactorProvider.ICodeStore
 where TSmsSender : class, ISmsSender
 {
 s.AddSingleton<ITwoFactorProvider, SmsTwoFactorProvider>();
 s.AddSingleton<EmailTwoFactorProvider.ICodeStore, TCodeStore>();
 s.AddSingleton<ISmsSender, TSmsSender>();
 return s;
 }

 public static IServiceCollection AddAuthenticatorTwoFactor<TSecretStore>(this IServiceCollection s)
 where TSecretStore : class, IAuthenticatorSecretStore
 {
 s.AddSingleton<ITwoFactorProvider, AuthenticatorTwoFactorProvider>();
 s.AddSingleton<IAuthenticatorSecretStore, TSecretStore>();
 return s;
 }
 }
}

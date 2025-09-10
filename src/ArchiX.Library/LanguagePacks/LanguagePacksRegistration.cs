using Microsoft.Extensions.DependencyInjection;

namespace ArchiX.Library.LanguagePacks
{
    /// <summary>
    /// Çok dillilik (i18n) servislerinin DI kayıtları.
    /// </summary>
    public static class LanguagePacksRegistration
    {
        /// <summary>
        /// ILanguageService/LanguageService kaydını ekler.
        /// Tüketen projeler builder.Services.AddLanguagePacks() diyerek entegre eder.
        /// </summary>
        public static IServiceCollection AddLanguagePacks(this IServiceCollection services)
        {
            services.AddScoped<ILanguageService, LanguageService>();
            return services;
        }
    }
}

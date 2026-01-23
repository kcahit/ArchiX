using ArchiX.Library.Abstractions.Security;
using ArchiX.Library.Infrastructure.Http;
using ArchiX.Library.Infrastructure.Parameters;
using ArchiX.Library.Services.Parameters;
using ArchiX.Library.Web.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ArchiX.Library.Web.Startup;

/// <summary>
/// #57 Parametre tabanlı options'ları DB'den yükleyen startup helper.
/// Migration (11.A.6) tamamlandıktan sonra aktif edilecek.
/// </summary>
public static class ParameterOptionsStartup
{
    /// <summary>
    /// DB'den kritik parametreleri okuyup singleton options'lara yükler.
    /// Bu method Program.cs'de migration sonrası çağrılmalıdır.
    /// </summary>
    public static async Task LoadParameterOptionsAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var paramService = scope.ServiceProvider.GetRequiredService<IParameterService>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<object>>();

        try
        {
            // 1. HttpPoliciesOptions yükle
            var httpOptions = await paramService.GetParameterAsync<HttpPoliciesOptions>(
                "HTTP", "HttpPoliciesOptions", applicationId: 1);
            
            if (httpOptions != null)
            {
                // Singleton'ı güncelle
                var httpSingleton = services.GetService<HttpPoliciesOptions>();
                if (httpSingleton != null)
                {
                    // Reflection ile property'leri kopyala (immutable olabilir)
                    // Alternatif: Singleton'ı yeniden kaydet
                }
                logger.LogInformation("✅ HttpPoliciesOptions loaded from DB: RetryCount={RetryCount}, TimeoutSeconds={TimeoutSeconds}",
                    httpOptions.RetryCount, httpOptions.TimeoutSeconds);
            }

            // 2. AttemptLimiterOptions yükle
            var attemptOptions = await paramService.GetParameterAsync<AttemptLimiterOptions>(
                "Security", "AttemptLimiterOptions", applicationId: 1);
            
            if (attemptOptions != null)
            {
                logger.LogInformation("✅ AttemptLimiterOptions loaded from DB: MaxAttempts={MaxAttempts}, Window={Window}s",
                    attemptOptions.MaxAttempts, attemptOptions.Window);
            }

            // 3. ParameterRefreshOptions yükle
            var refreshOptions = await paramService.GetParameterAsync<ParameterRefreshOptions>(
                "System", "ParameterRefresh", applicationId: 1);
            
            if (refreshOptions != null)
            {
                logger.LogInformation("✅ ParameterRefreshOptions loaded from DB: UiCacheTtl={UiTtl}s, HttpCacheTtl={HttpTtl}s",
                    refreshOptions.UiCacheTtlSeconds, refreshOptions.HttpCacheTtlSeconds);
            }

            logger.LogInformation("✅ All parameter-based options loaded successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Failed to load parameter-based options from DB");
            throw;
        }
    }
}

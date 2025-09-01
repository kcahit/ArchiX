using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ArchiX.Library.Logging;

/// <summary>
/// Logging servislerini DI container’a eklemek için extension metotlarını içerir.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// ArchiX loglama altyapısını ve <see cref="LoggingOptions"/> bağlamasını ekler.
    /// Projede tek satırla çağır:
    /// <code>builder.Services.AddArchiXLogging(builder.Configuration);</code>
    /// </summary>
    /// <param name="services">DI service koleksiyonu.</param>
    /// <param name="configuration">Uygulama konfigürasyonu.</param>
    /// <returns>Güncellenmiş service koleksiyonu.</returns>
    public static IServiceCollection AddArchiXLogging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<LoggingOptions>()
            // Bind paketine bağımlı olmadan manuel bağlama
            .Configure<IConfiguration>((opt, cfg) =>
            {
                var s = (configuration ?? cfg)?.GetSection("Logging");
                if (s == null) return;

                opt.AppName = s["AppName"] ?? opt.AppName;
                opt.BasePath = s["BasePath"] ?? opt.BasePath;
                opt.DailyFilePrefix = s["DailyFilePrefix"] ?? opt.DailyFilePrefix;
                opt.TimeZoneId = s["TimeZoneId"] ?? opt.TimeZoneId;

                if (int.TryParse(s["MaxFileSizeMB"], out var maxMb)) opt.MaxFileSizeMB = maxMb;
                if (int.TryParse(s["RetainDays"], out var days)) opt.RetainDays = days;
                if (bool.TryParse(s["ErrorOnly"], out var eo)) opt.ErrorOnly = eo;
                if (bool.TryParse(s["EmailEnabled"], out var em)) opt.EmailEnabled = em;

                var dmRaw = s["DeliveryMode"];
                if (!string.IsNullOrWhiteSpace(dmRaw))
                {
                    if (Enum.TryParse<DeliveryMode>(dmRaw, true, out var dmByName))
                        opt.DeliveryMode = dmByName;
                    else if (int.TryParse(dmRaw, out var dmByInt) && Enum.IsDefined(typeof(DeliveryMode), dmByInt))
                        opt.DeliveryMode = (DeliveryMode)dmByInt;
                }
            })
            .PostConfigure(o =>
            {
                // Güvenli varsayılanlar
                o.AppName ??= AppDomain.CurrentDomain.FriendlyName;
                o.BasePath ??= @"C:\ArchiX\Logs\ArchiXTests\Api";
                o.DailyFilePrefix ??= "errors";
                o.TimeZoneId ??= TimeZoneInfo.Local.Id;

                if (o.MaxFileSizeMB <= 0) o.MaxFileSizeMB = 50;
                if (o.RetainDays <= 0) o.RetainDays = 14;
            });

        // DI: LoggingOptions ve JsonlLogWriter kayıtları
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<LoggingOptions>>().Value);
        services.AddSingleton<JsonlLogWriter>(sp => new JsonlLogWriter(sp.GetRequiredService<LoggingOptions>()));

        return services;
    }

    /// <summary>
    /// ServiceProvider içinden LoggingOptions nesnesini döndürür.
    /// </summary>
    public static LoggingOptions GetArchiXLoggingOptions(this IServiceProvider sp) =>
        sp.GetRequiredService<IOptions<LoggingOptions>>().Value;
}

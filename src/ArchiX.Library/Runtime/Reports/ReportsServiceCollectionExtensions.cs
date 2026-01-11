using ArchiX.Library.Abstractions.Reports;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ArchiX.Library.Runtime.Reports;

public static class ReportsServiceCollectionExtensions
{
    public static IServiceCollection AddArchiXReports(this IServiceCollection services, Action<ReportDatasetLimitOptions>? configureLimits = null)
    {
        services.AddOptions<ReportDatasetLimitOptions>();
        if (configureLimits is not null)
            services.Configure(configureLimits);

        services.TryAddSingleton<ReportDatasetLimitGuard>();
        services.TryAddSingleton<IReportDatasetExecutor, ReportDatasetExecutor>();

        return services;
    }
}

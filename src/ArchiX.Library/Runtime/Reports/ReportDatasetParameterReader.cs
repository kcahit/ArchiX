using ArchiX.Library.Context;
using Microsoft.EntityFrameworkCore;

namespace ArchiX.Library.Runtime.Reports;

internal static class ReportDatasetParameterReader
{
    private const int GlobalApplicationId = 1;

    public static async Task<string?> GetValueAsync(
        IDbContextFactory<AppDbContext> dbFactory,
        string group,
        string key,
        CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);

        var param = await db.Parameters.AsNoTracking()
            .Include(p => p.Applications)
            .Where(p => p.Group == group && p.Key == key)
            .SingleOrDefaultAsync(ct)
            .ConfigureAwait(false);

        var appValue = param?.Applications.FirstOrDefault(a => a.ApplicationId == GlobalApplicationId);
        return appValue?.Value;
    }
}

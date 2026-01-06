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

        return await db.Parameters.AsNoTracking()
            .Where(p => p.ApplicationId == GlobalApplicationId && p.Group == group && p.Key == key)
            .Select(p => p.Value)
            .SingleOrDefaultAsync(ct)
            .ConfigureAwait(false);
    }
}

using ArchiX.Library.Context;
using ArchiX.Library.Runtime.Reports;
using ArchiX.Library.Web.Abstractions.Reports;
using ArchiX.Library.Web.ViewModels.Grid;

using Microsoft.EntityFrameworkCore;

namespace ArchiX.Library.Web.Runtime.Reports;

public sealed class ReportDatasetOptionService : IReportDatasetOptionService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public ReportDatasetOptionService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<IReadOnlyList<ReportDatasetOptionViewModel>> GetApprovedOptionsAsync(CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);

        return await db.Set<ArchiX.Library.Entities.ReportDataset>()
            .AsNoTracking()
            .ApprovedOnly()
            .OrderBy(x => x.DisplayName)
            .Select(x => new ReportDatasetOptionViewModel(x.Id, x.DisplayName))
            .ToListAsync(ct)
            .ConfigureAwait(false);
    }
}

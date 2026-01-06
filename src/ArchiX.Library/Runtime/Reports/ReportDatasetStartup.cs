using ArchiX.Library.Context;
using ArchiX.Library.Entities;

using Microsoft.EntityFrameworkCore;

namespace ArchiX.Library.Runtime.Reports;

public static class ReportDatasetStartup
{
    public static async Task EnsureSeedAsync(AppDbContext db, CancellationToken ct = default)
    {
        foreach (var g in ReportDatasetSeeds.TypeGroups)
        {
            var exists = await db.Set<ReportDatasetTypeGroup>()
                .AnyAsync(x => x.Code == g.Code, ct)
                .ConfigureAwait(false);

            if (exists)
                continue;

            db.Add(new ReportDatasetTypeGroup
            {
                Code = g.Code,
                Name = g.Name,
                Description = g.Description,
                StatusId = BaseEntity.ApprovedStatusId,
                CreatedBy = 0,
                LastStatusBy = 0,
                IsProtected = true
            });
        }

        await db.SaveChangesAsync(ct).ConfigureAwait(false);

        foreach (var t in ReportDatasetSeeds.Types)
        {
            var exists = await db.Set<ReportDatasetType>()
                .AnyAsync(x => x.Code == t.Code, ct)
                .ConfigureAwait(false);

            if (exists)
                continue;

            var groupId = await db.Set<ReportDatasetTypeGroup>()
                .Where(x => x.Code == t.GroupCode)
                .Select(x => x.Id)
                .SingleAsync(ct)
                .ConfigureAwait(false);

            db.Add(new ReportDatasetType
            {
                ReportDatasetTypeGroupId = groupId,
                Code = t.Code,
                Name = t.Name,
                Description = t.Description,
                StatusId = BaseEntity.ApprovedStatusId,
                CreatedBy = 0,
                LastStatusBy = 0,
                IsProtected = true
            });
        }

        await db.SaveChangesAsync(ct).ConfigureAwait(false);
    }
}

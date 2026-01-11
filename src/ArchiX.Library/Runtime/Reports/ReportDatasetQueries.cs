using ArchiX.Library.Entities;

using Microsoft.EntityFrameworkCore;

namespace ArchiX.Library.Runtime.Reports
{
    public static class ReportDatasetQueries
    {
        public static IQueryable<ReportDataset> ApprovedOnly(this IQueryable<ReportDataset> query) =>
            query.Where(x => x.StatusId == BaseEntity.ApprovedStatusId);

        public static IQueryable<ReportDataset> ApprovedWithType(this IQueryable<ReportDataset> query) =>
            query.ApprovedOnly()
                .Include(x => x.Type)
                .ThenInclude(t => t.Group);
    }
}

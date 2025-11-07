using Microsoft.EntityFrameworkCore;

namespace ArchiX.Library.Infrastructure.EfCore
{
    /// <summary>
    /// Helper extensions for applying common EF Core query optimizations.
    /// </summary>
    public static class QueryableOptimizationExtensions
    {
        /// <summary>
        /// Apply recommended read-time optimizations to a query: AsNoTracking (or AsNoTrackingWithIdentityResolution)
        /// and optionally AsSplitQuery for queries with includes.
        /// </summary>
        public static IQueryable<T> ApplyDefaultReadOptions<T>(this IQueryable<T> query, bool identityResolution = false, bool splitQuery = false)
        where T : class
        {
            ArgumentNullException.ThrowIfNull(query);

            IQueryable<T> q = identityResolution
            ? query.AsNoTrackingWithIdentityResolution()
            : query.AsNoTracking();

            if (splitQuery)
                q = q.AsSplitQuery();

            return q;
        }
    }
}

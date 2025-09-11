using System.Linq.Expressions;

namespace ArchiX.Library.Sorting;

/// <summary>
/// IQueryable için sıralama uzantıları.
/// </summary>
public static class SortingExtensions
{
    /// <summary>
    /// Verilen SortItem listesini IQueryable sorgusu üzerine uygular.
    /// </summary>
    /// <typeparam name="T">Sorgunun eleman tipi.</typeparam>
    /// <param name="query">Kaynak sorgu.</param>
    /// <param name="sorts">Sıralama kriterleri.</param>
    /// <returns>Sıralama uygulanmış sorgu.</returns>
    public static IQueryable<T> ApplySorting<T>(
        this IQueryable<T> query,
        IEnumerable<SortItem>? sorts)
    {
        if (sorts == null)
            return query;

        bool first = true;
        IOrderedQueryable<T>? ordered = null;

        foreach (var sort in sorts)
        {
            if (string.IsNullOrWhiteSpace(sort.Field))
                continue;

            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.PropertyOrField(parameter, sort.Field);
            var lambda = Expression.Lambda(property, parameter);

            string method = first
                ? (sort.Direction == SortDirection.Ascending ? "OrderBy" : "OrderByDescending")
                : (sort.Direction == SortDirection.Ascending ? "ThenBy" : "ThenByDescending");

            query = CallOrder(query, lambda, method);
            ordered = query as IOrderedQueryable<T>;
            first = false;
        }

        return ordered ?? query;
    }

    private static IQueryable<T> CallOrder<T>(
        IQueryable<T> source,
        LambdaExpression keySelector,
        string methodName)
    {
        var typeArgs = new[] { typeof(T), keySelector.Body.Type };
        var call = Expression.Call(
            typeof(Queryable),
            methodName,
            typeArgs,
            source.Expression,
            Expression.Quote(keySelector));

        return source.Provider.CreateQuery<T>(call);
    }
}

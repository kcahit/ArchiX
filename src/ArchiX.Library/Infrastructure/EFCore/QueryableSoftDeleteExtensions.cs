using Microsoft.EntityFrameworkCore;

namespace ArchiX.Library.Infrastructure.EfCore
{
    /// <summary>
    /// EF Core’da soft-delete için tanımlanan global sorgu filtresini (HasQueryFilter) geçici olarak
    /// devre dışı bırakmaya yarayan sorgu yardımcıları.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Bu yardımcılar, <c>ModelBuilderExtensionsSoftDelete.ApplySoftDeleteFilter(...)</c> ile konulan
    /// global <c>HasQueryFilter</c> ifadesini EF Core’un <c>IgnoreQueryFilters()</c> API’ı üzerinden bypass eder.
    /// Herhangi bir sabit durum ID’si (örn. <c>DEL = 6</c>) KULLANMAZ; bu nedenle ileride durum ID’leri
    /// değişse bile davranış aynı kalır.
    /// </para>
    /// <para>Kullanım örnekleri:</para>
    /// <code>
    /// <![CDATA[
    /// // Varsayılan: silinmemişler (global filter açık)
    /// var aktifler = ctx.Set<MyEntity>().ToList();
    ///
    /// // Silinmişleri de dahil et (global filter bypass)
    /// var hepsi = ctx.Set<MyEntity>().IncludeDeleted().ToList();
    ///
    /// // Sadece silinmişleri görmek istersen:
    /// // Not: DEL ID’sini sabit kullanma. Runtime’da Statu.Code == "DEL" ile çek.
    /// var deletedId = await ctx.Set<Statu>()
    ///                          .Where(s => s.Code == "DEL")
    ///                          .Select(s => s.Id)
    ///                          .SingleAsync();
    ///
    /// var sadeceSilinmis = ctx.Set<MyEntity>()
    ///                         .IncludeDeleted()
    ///                         .Where(e => e.StatusId == deletedId)
    ///                         .ToList();
    /// ]]>
    /// </code>
    /// </remarks>
    public static class QueryableSoftDeleteExtensions
    {
        /// <summary>Global <c>HasQueryFilter</c> ifadelerini yok sayar; silinmiş kayıtlar da sorguya dahil olur.</summary>
        public static IQueryable<T> IncludeDeleted<T>(this IQueryable<T> query) where T : class
            => query.IgnoreQueryFilters();

        /// <summary><see cref="IncludeDeleted{T}(IQueryable{T})"/> ile aynı; alternatif isim.</summary>
        public static IQueryable<T> WithDeleted<T>(this IQueryable<T> query) where T : class
            => query.IgnoreQueryFilters();

        /// <summary><see cref="IncludeDeleted{T}(IQueryable{T})"/> ile aynı davranış; <see cref="DbSet{TEntity}"/> üzerinden kullanım kolaylığı.</summary>
        public static IQueryable<T> IncludeDeleted<T>(this DbSet<T> set) where T : class
            => set.IgnoreQueryFilters();

        /// <summary><see cref="WithDeleted{T}(IQueryable{T})"/> ile aynı; <see cref="DbSet{TEntity}"/> üzerinden alternatif isim.</summary>
        public static IQueryable<T> WithDeleted<T>(this DbSet<T> set) where T : class
            => set.IgnoreQueryFilters();
    }
}

using System.Linq.Expressions;
using System.Reflection;

using ArchiX.Library.Entities; // BaseEntity için

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ArchiX.Library.Infrastructure.EFCore
{
    /// <summary>
    /// StatusId'si "DEL" olan (örn: -14) kayıtları globalde dışlamak için ModelBuilder uzantıları.
    /// </summary>
    public static class ModelBuilderExtensionsSoftDelete
    {
        /// <summary>
        /// Tüm BaseEntity türevlerine HasQueryFilter(e => e.StatusId != deletedStatusId) uygular.
        /// Varsayılan deletedStatusId = -14 (Statu seed: Code="DEL").
        /// </summary>
        public static void ApplySoftDeleteFilter(this ModelBuilder modelBuilder, int deletedStatusId = -14)
        {
            foreach (var et in modelBuilder.Model.GetEntityTypes()
                         .Where(et => et.ClrType is not null &&
                                      !IsOwned(et) &&
                                      typeof(BaseEntity).IsAssignableFrom(et.ClrType) &&
                                      et.ClrType != typeof(BaseEntity)))
            {
                var method = typeof(ModelBuilderExtensionsSoftDelete)
                    .GetMethod(nameof(ApplyFilterGeneric), BindingFlags.NonPublic | BindingFlags.Static)!
                    .MakeGenericMethod(et.ClrType);

                method.Invoke(null, new object[] { modelBuilder, deletedStatusId });
            }
        }

        // TEntity : BaseEntity için HasQueryFilter(e => e.StatusId != deletedStatusId)
        private static void ApplyFilterGeneric<TEntity>(ModelBuilder modelBuilder, int deletedStatusId)
            where TEntity : BaseEntity
        {
            var param = Expression.Parameter(typeof(TEntity), "e");
            var statusProp = Expression.Property(param, nameof(BaseEntity.StatusId));
            var deletedConst = Expression.Constant(deletedStatusId, typeof(int));
            var notDeleted = Expression.NotEqual(statusProp, deletedConst);
            var lambda = Expression.Lambda<Func<TEntity, bool>>(notDeleted, param);

            modelBuilder.Entity<TEntity>().HasQueryFilter(lambda);
        }

        private static bool IsOwned(IMutableEntityType et) => et.IsOwned();
    }
}

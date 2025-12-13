using System.Reflection;

using ArchiX.Library.Entities;

using Humanizer;

using Microsoft.EntityFrameworkCore;

namespace ArchiX.Library.Context
{
    internal static class ModelBuilderExtensions
    {
        // 1) Discover BaseEntity-derived types, ignore MapToDb=false, pluralize table names
        public static void ApplyPluralizeAndMapToDb(this ModelBuilder modelBuilder, Assembly asm)
        {
            var entityTypes = asm.GetTypes()
                .Where(t => typeof(BaseEntity).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
                .OrderBy(t => t.FullName);

            foreach (var t in entityTypes)
            {
                var fi = t.GetField(nameof(BaseEntity.MapToDb), BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                      ?? t.GetField(nameof(BaseEntity.MapToDb), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                var include = (fi?.FieldType) != typeof(bool) || (bool)fi.GetValue(null)!;
                if (!include) { modelBuilder.Ignore(t); continue; }
                modelBuilder.Entity(t).ToTable(t.Name.Pluralize());
            }
        }

        // 2) Common columns + (except Statu) StatusId -> Statu.Id FK + KOLON SIRALAMASI
        // 2) Common columns + (except Statu) StatusId -> Statu.Id FK + KOLON SIRALAMASI
        public static void ApplyBaseEntityConventions(this ModelBuilder modelBuilder)
        {
            foreach (var et in modelBuilder.Model.GetEntityTypes()
                         .Where(et => typeof(BaseEntity).IsAssignableFrom(et.ClrType)
                                   && et.ClrType != typeof(BaseEntity)))
            {
                modelBuilder.Entity(et.ClrType, b =>
                {
                    // ✅ Id her zaman ilk sırada (order: 0)
                    b.Property<int>(nameof(BaseEntity.Id))
                        .ValueGeneratedOnAdd()
                        .UseIdentityColumn(1, 1)
                        .HasColumnOrder(0);

                    var entityProps = et.ClrType
                        .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                        .Where(p => p.DeclaringType == et.ClrType
                                 && p.Name != nameof(BaseEntity.Id)
                                 && (p.PropertyType.IsValueType || p.PropertyType == typeof(string)))
                        .ToList();

                    int order = 1;
                    foreach (var prop in entityProps)
                    {
                        b.Property(prop.PropertyType, prop.Name).HasColumnOrder(order++);
                    }

                    // ✅ BaseEntity kolonları EN SONDA! (order: 1000+)
                    b.Property<Guid>(nameof(BaseEntity.RowId))
                        .HasDefaultValueSql("NEWSEQUENTIALID()")
                        .ValueGeneratedOnAdd()
                        .HasColumnOrder(1000);

                    b.Property<DateTimeOffset>(nameof(BaseEntity.CreatedAt))
                        .HasDefaultValueSql("SYSDATETIMEOFFSET()")
                        .HasPrecision(4)
                        .ValueGeneratedOnAdd()
                        .HasColumnOrder(1001);

                    b.Property<int>(nameof(BaseEntity.CreatedBy))
                        .HasColumnOrder(1002);

                    b.Property<DateTimeOffset?>(nameof(BaseEntity.UpdatedAt))
                        .HasPrecision(4)
                        .HasColumnOrder(1003);

                    b.Property<int?>(nameof(BaseEntity.UpdatedBy))
                        .HasColumnOrder(1004);

                    b.Property<int>(nameof(BaseEntity.StatusId))
                        .IsRequired()
                        .HasColumnOrder(1005);

                    b.Property<DateTimeOffset?>(nameof(BaseEntity.LastStatusAt))
                        .HasDefaultValueSql("SYSDATETIMEOFFSET()")
                        .HasPrecision(4)
                        .ValueGeneratedOnAdd()
                        .HasColumnOrder(1006);

                    b.Property<int>(nameof(BaseEntity.LastStatusBy))
                        .HasColumnOrder(1007);

                    b.Property<bool>(nameof(BaseEntity.IsProtected))
                        .HasColumnOrder(1008);

                    // FK ve index (Statu hariç)
                    if (et.ClrType != typeof(Statu))
                    {
                        b.HasIndex(nameof(BaseEntity.StatusId));
                        b.HasOne(typeof(Statu))
                            .WithMany()
                            .HasForeignKey(nameof(BaseEntity.StatusId))
                            .OnDelete(DeleteBehavior.Restrict);
                    }
                });
            }
        }

        // 3) Soft delete filter (DeletedStatusId != StatusId) except Statu
        public static void ApplySoftDeleteFilters(this ModelBuilder modelBuilder)
        {
            foreach (var et in modelBuilder.Model.GetEntityTypes()
                         .Where(et => typeof(BaseEntity).IsAssignableFrom(et.ClrType)
                                   && et.ClrType != typeof(BaseEntity)
                                   && et.ClrType != typeof(Statu)))
            {
                var entityType = et.ClrType;
                var p = System.Linq.Expressions.Expression.Parameter(entityType, "e");
                var statusProp = System.Linq.Expressions.Expression.Property(p, nameof(BaseEntity.StatusId));
                var delConst = System.Linq.Expressions.Expression.Constant(BaseEntity.DeletedStatusId);
                var body = System.Linq.Expressions.Expression.NotEqual(statusProp, delConst);
                var lambdaType = typeof(Func<,>).MakeGenericType(entityType, typeof(bool));
                var lambda = System.Linq.Expressions.Expression.Lambda(lambdaType, body, p);
                modelBuilder.Entity(entityType).HasQueryFilter(lambda);
            }
        }

        /// <summary>
        /// Tüm foreign key'leri DeleteBehavior.Restrict yapar (audit trail için).
        /// </summary>
        public static void ApplyRestrictDeleteBehavior(this ModelBuilder modelBuilder)
        {
            foreach (var relationship in modelBuilder.Model.GetEntityTypes()
                .SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }
    }
}

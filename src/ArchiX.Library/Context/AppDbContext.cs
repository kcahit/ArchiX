using System.Reflection;

using ArchiX.Library.Entities;
using ArchiX.Library.Filtering;
using ArchiX.Library.Infrastructure.EFCore;
using ArchiX.Library.LanguagePacks;



using Humanizer;

using Microsoft.EntityFrameworkCore;

namespace ArchiX.Library.Context
{
    /// <summary>
    /// ArchiX uygulamasının veritabanı işlemlerini yöneten DbContext sınıfı.
    /// Entity tanımları, konfigürasyonlar ve seed verileri burada yapılır.
    /// </summary>
    public partial class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        /// <summary>
        /// Çalışma zamanında yüklü tüm assembly’leri tutan cache.
        /// IEntity implementasyonu aramada kullanılır.
        /// </summary>
        private static readonly Assembly[] _cachedAssemblies = AppDomain.CurrentDomain.GetAssemblies();

        /// <summary>
        /// Model yapılandırmasını özelleştirmek için kullanılır.
        /// IEntity implementasyonu olan entity'leri otomatik ekler,
        /// BaseEntity türevleri için ortak alanları ayarlar ve seed verilerini yükler.
        /// </summary>
        /// <param name="modelBuilder">Model oluşturucu.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // IEntity implement eden tüm entity'leri otomatik ekle
            foreach (var assembly in _cachedAssemblies)
            {
                var entityTypes = assembly.GetTypes()
                    .Where(t => typeof(IEntity).IsAssignableFrom(t)
                             && t.IsClass
                             && !t.IsAbstract);

                foreach (var type in entityTypes)
                {
                    var plural = type.Name.Pluralize();
                    modelBuilder.Entity(type).ToTable(plural);
                }

                modelBuilder.ApplyConfigurationsFromAssembly(assembly);
            }

            // BaseEntity’den türeyen entity'ler için ortak property ayarları
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType) &&
                    entityType.ClrType != typeof(BaseEntity))
                {
                    modelBuilder.Entity(entityType.ClrType, builder =>
                    {
                        builder.Property<Guid>("RowId")
                               .HasDefaultValueSql("NEWSEQUENTIALID()")
                               .ValueGeneratedOnAdd();

                        builder.Property<DateTimeOffset>("CreatedAt")
                               .HasDefaultValueSql("SYSDATETIMEOFFSET()")
                               .HasPrecision(4);

                        builder.Property<DateTimeOffset?>("UpdatedAt")
                               .HasPrecision(4);

                        builder.Property<DateTimeOffset?>("LastStatusAt")
                               .HasDefaultValueSql("SYSDATETIMEOFFSET()")
                               .HasPrecision(4);
                    });
                }
            }

            // Unique Key Configurations
            modelBuilder.Entity<FilterItem>(entity =>
            {
                entity.HasKey(f => f.Id);
                entity.HasIndex(f => new { f.ItemType, f.Code }).IsUnique();
            });

            modelBuilder.Entity<LanguagePack>(entity =>
            {
                entity.HasKey(lp => lp.Id);
                entity.HasIndex(lp => new { lp.ItemType, lp.EntityName, lp.FieldName, lp.Code, lp.Culture })
                      .IsUnique();
            });

            // Seed verileri ekle
            ConfigureStatuSeeds(modelBuilder);
            ConfigureFilterItemSeeds(modelBuilder);
            ConfigureLanguagePackSeeds(modelBuilder);

            // --- Soft-delete global filtre: DEL kodluları otomatik dışla
            ModelBuilderExtensionsSoftDelete.ApplySoftDeleteFilter(modelBuilder);

        }

        /// <summary>
        /// Statu entity için başlangıç (seed) verilerini ekler.
        /// </summary>
        private static void ConfigureStatuSeeds(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Statu>().HasData(
                new Statu { Id = -1, Code = "DFT", Name = "Draft", Description = "Record is in draft state" },
                new Statu { Id = -10, Code = "AWT", Name = "Awaiting Approval", Description = "Record is waiting for approval" },
                new Statu { Id = -11, Code = "APR", Name = "Approved", Description = "Record has been approved" },
                new Statu { Id = -12, Code = "REJ", Name = "Rejected", Description = "Record has been rejected" },
                new Statu { Id = -13, Code = "PSV", Name = "Passive", Description = "Record is passive / inactive" },
                new Statu { Id = -14, Code = "DEL", Name = "Deleted", Description = "Record has been deleted" }
            );
        }

        /// <summary>
        /// FilterItem entity için başlangıç (seed) verilerini ekler.
        /// </summary>
        private static void ConfigureFilterItemSeeds(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FilterItem>().HasData(
                new FilterItem { Id = -10, ItemType = "Operator", Code = "Equals" },
                new FilterItem { Id = -11, ItemType = "Operator", Code = "NotEquals" },
                new FilterItem { Id = -12, ItemType = "Operator", Code = "StartsWith" },
                new FilterItem { Id = -13, ItemType = "Operator", Code = "NotStartsWith" },
                new FilterItem { Id = -14, ItemType = "Operator", Code = "EndsWith" },
                new FilterItem { Id = -15, ItemType = "Operator", Code = "NotEndsWith" },
                new FilterItem { Id = -16, ItemType = "Operator", Code = "Contains" },
                new FilterItem { Id = -17, ItemType = "Operator", Code = "NotContains" },
                new FilterItem { Id = -18, ItemType = "Operator", Code = "Between" },
                new FilterItem { Id = -19, ItemType = "Operator", Code = "NotBetween" },
                new FilterItem { Id = -20, ItemType = "Operator", Code = "GreaterThan" },
                new FilterItem { Id = -21, ItemType = "Operator", Code = "GreaterThanOrEqual" },
                new FilterItem { Id = -22, ItemType = "Operator", Code = "LessThan" },
                new FilterItem { Id = -23, ItemType = "Operator", Code = "LessThanOrEqual" },
                new FilterItem { Id = -24, ItemType = "Operator", Code = "In" },
                new FilterItem { Id = -25, ItemType = "Operator", Code = "NotIn" },
                new FilterItem { Id = -26, ItemType = "Operator", Code = "IsNull" },
                new FilterItem { Id = -27, ItemType = "Operator", Code = "IsNotNull" }
            );
        }

        /// <summary>
        /// LanguagePack entity için başlangıç (seed) verilerini ekler.
        /// </summary>
        private static void ConfigureLanguagePackSeeds(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LanguagePack>().HasData(
                new LanguagePack { Id = -1001, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "Equals", Culture = "tr-TR", DisplayName = "Eşittir", Description = "Değer eşit olmalı" },
                new LanguagePack { Id = -1002, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "Equals", Culture = "en-US", DisplayName = "Equals", Description = "Value must be equal" },
                new LanguagePack { Id = -1003, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "NotEquals", Culture = "tr-TR", DisplayName = "Eşit Değil", Description = "Değer eşit olmamalı" },
                new LanguagePack { Id = -1004, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "NotEquals", Culture = "en-US", DisplayName = "Not Equal", Description = "Value must not be equal" },
                new LanguagePack { Id = -1005, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "StartsWith", Culture = "tr-TR", DisplayName = "Başlar", Description = "Başlangıç eşleşmesi" },
                new LanguagePack { Id = -1006, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "StartsWith", Culture = "en-US", DisplayName = "Starts With", Description = "Value starts with given text" },
                new LanguagePack { Id = -1035, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "IsNotNull", Culture = "tr-TR", DisplayName = "Boş Değil", Description = "Değer null değil ve empty değil ise" },
                new LanguagePack { Id = -1036, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "IsNotNull", Culture = "en-US", DisplayName = "Is Not Null/Empty", Description = "Value is not null and not empty" }
            );
        }
    }
}

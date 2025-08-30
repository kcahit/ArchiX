using ArchiX.Library.Filtering;
using ArchiX.Library.LanguagePacks;
using ArchiX.Library.Entities;
using ArchiX.Library.Interfaces;
using Humanizer; // 👈 Humanizer eklendi
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Reflection.Emit;

namespace ArchiX.Library.Context
{
    public class AppDbContext : DbContext
    {
        private static readonly Assembly[] _cachedAssemblies = AppDomain.CurrentDomain.GetAssemblies();

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

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
                    // Humanizer ile çoğul tablo adı
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
                               .HasPrecision(4); // saniye altı 4 hane

                        builder.Property<DateTimeOffset?>("UpdatedAt")
                               .HasPrecision(4);

                        builder.Property<DateTimeOffset?>("LastStatusAt")
                                 .HasDefaultValueSql("SYSDATETIMEOFFSET()")
                               .HasPrecision(4);
                    });
                }
            }

            // 🔹 Unique Key Configurations
            modelBuilder.Entity<FilterItem>(entity =>
            {
                entity.HasKey(f => f.Id); // PK
                entity.HasIndex(f => new { f.ItemType, f.Code }).IsUnique(); // Unique constraint
            });

            modelBuilder.Entity<LanguagePack>(entity =>
            {
                entity.HasKey(lp => lp.Id); // PK
                entity.HasIndex(lp => new { lp.ItemType, lp.EntityName, lp.FieldName, lp.Code, lp.Culture })
                      .IsUnique(); // Unique constraint
            });

            // Seed verileri ekle
            ConfigureStatuSeeds(modelBuilder);
            ConfigureFilterItemSeeds(modelBuilder);
            ConfigureLanguagePackSeeds(modelBuilder);
        }

        private void ConfigureStatuSeeds(ModelBuilder modelBuilder)
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

     

        private void ConfigureFilterItemSeeds(ModelBuilder modelBuilder)
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
        


        private void ConfigureLanguagePackSeeds(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<LanguagePack>().HasData(
    new LanguagePack { Id = -1001, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "Equals", Culture = "tr-TR", DisplayName = "Eşittir", Description = "Değer eşit olmalı" },
                new LanguagePack { Id = -1002, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "Equals", Culture = "en-US", DisplayName = "Equals", Description = "Value must be equal" },

                // NotEquals
                new LanguagePack { Id = -1003, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "NotEquals", Culture = "tr-TR", DisplayName = "Eşit Değil", Description = "Değer eşit olmamalı" },
                new LanguagePack { Id = -1004, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "NotEquals", Culture = "en-US", DisplayName = "Not Equal", Description = "Value must not be equal" },

                // StartsWith
                new LanguagePack { Id = -1005, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "StartsWith", Culture = "tr-TR", DisplayName = "Başlar", Description = "Başlangıç eşleşmesi" },
                new LanguagePack { Id = -1006, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "StartsWith", Culture = "en-US", DisplayName = "Starts With", Description = "Value starts with given text" },

                // NotStartsWith
                new LanguagePack { Id = -1007, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "NotStartsWith", Culture = "tr-TR", DisplayName = "Başlamaz", Description = "Başlangıç eşleşmesi değil" },
                new LanguagePack { Id = -1008, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "NotStartsWith", Culture = "en-US", DisplayName = "Does Not Start With", Description = "Value must not start with given text" },

                // EndsWith
                new LanguagePack { Id = -1009, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "EndsWith", Culture = "tr-TR", DisplayName = "Biter", Description = "Bitiş eşleşmesi" },
                new LanguagePack { Id = -1010, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "EndsWith", Culture = "en-US", DisplayName = "Ends With", Description = "Value ends with given text" },

                // NotEndsWith
                new LanguagePack { Id = -1011, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "NotEndsWith", Culture = "tr-TR", DisplayName = "Bitmez", Description = "Bitiş eşleşmesi değil" },
                new LanguagePack { Id = -1012, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "NotEndsWith", Culture = "en-US", DisplayName = "Does Not End With", Description = "Value must not end with given text" },

                // Contains
                new LanguagePack { Id = -1013, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "Contains", Culture = "tr-TR", DisplayName = "İçerir", Description = "İçinde geçen değer varsa" },
                new LanguagePack { Id = -1014, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "Contains", Culture = "en-US", DisplayName = "Contains", Description = "Value contains given text" },

                // NotContains
                new LanguagePack { Id = -1015, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "NotContains", Culture = "tr-TR", DisplayName = "İçermez", Description = "İçinde geçen değer yoksa" },
                new LanguagePack { Id = -1016, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "NotContains", Culture = "en-US", DisplayName = "Does Not Contain", Description = "Value must not contain given text" },

                // Between
                new LanguagePack { Id = -1017, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "Between", Culture = "tr-TR", DisplayName = "Arasında", Description = "İki değer arasındaysa" },
                new LanguagePack { Id = -1018, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "Between", Culture = "en-US", DisplayName = "Between", Description = "Value must be between two values" },

                // NotBetween
                new LanguagePack { Id = -1019, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "NotBetween", Culture = "tr-TR", DisplayName = "Arasında Değil", Description = "İki değer arasında olmamalı" },
                new LanguagePack { Id = -1020, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "NotBetween", Culture = "en-US", DisplayName = "Not Between", Description = "Value must not be between two values" },

                // GreaterThan
                new LanguagePack { Id = -1021, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "GreaterThan", Culture = "tr-TR", DisplayName = "Büyük", Description = "Belirtilen değerden büyükse" },
                new LanguagePack { Id = -1022, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "GreaterThan", Culture = "en-US", DisplayName = "Greater Than", Description = "Value must be greater than given value" },

                // GreaterThanOrEqual
                new LanguagePack { Id = -1023, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "GreaterThanOrEqual", Culture = "tr-TR", DisplayName = "Büyük veya Eşit", Description = "Belirtilen değerden büyük ya da eşit" },
                new LanguagePack { Id = -1024, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "GreaterThanOrEqual", Culture = "en-US", DisplayName = "Greater Than Or Equal", Description = "Value must be greater than or equal to given value" },

                // LessThan
                new LanguagePack { Id = -1025, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "LessThan", Culture = "tr-TR", DisplayName = "Küçük", Description = "Belirtilen değerden küçükse" },
                new LanguagePack { Id = -1026, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "LessThan", Culture = "en-US", DisplayName = "Less Than", Description = "Value must be less than given value" },

                // LessThanOrEqual
                new LanguagePack { Id = -1027, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "LessThanOrEqual", Culture = "tr-TR", DisplayName = "Küçük veya Eşit", Description = "Belirtilen değerden küçük ya da eşit" },
                new LanguagePack { Id = -1028, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "LessThanOrEqual", Culture = "en-US", DisplayName = "Less Than Or Equal", Description = "Value must be less than or equal to given value" },

                // In
                new LanguagePack { Id = -1029, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "In", Culture = "tr-TR", DisplayName = "İçinde", Description = "Liste içindeki değerlerden biriyse" },
                new LanguagePack { Id = -1030, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "In", Culture = "en-US", DisplayName = "In", Description = "Value must be in the given list" },

                // NotIn
                new LanguagePack { Id = -1031, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "NotIn", Culture = "tr-TR", DisplayName = "İçinde Değil", Description = "Liste içindeki değerlerden biri değilse" },
                new LanguagePack { Id = -1032, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "NotIn", Culture = "en-US", DisplayName = "Not In", Description = "Value must not be in the given list" },

                // IsNull
                new LanguagePack { Id = -1033, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "IsNull", Culture = "tr-TR", DisplayName = "Boş", Description = "Değer null veya empty ise" },
                new LanguagePack { Id = -1034, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "IsNull", Culture = "en-US", DisplayName = "Is Null/Empty", Description = "Value is null or empty" },

                // IsNotNull
                new LanguagePack { Id = -1035, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "IsNotNull", Culture = "tr-TR", DisplayName = "Boş Değil", Description = "Değer null değil ve empty değil ise" },
                new LanguagePack { Id = -1036, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "IsNotNull", Culture = "en-US", DisplayName = "Is Not Null/Empty", Description = "Value is not null and not empty" }
             );

        }
    }
}


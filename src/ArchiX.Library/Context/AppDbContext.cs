// File: src/ArchiX.Library/Context/AppDbContext.cs

using System.Reflection;
using ArchiX.Library.Entities; // BaseEntity, Statu, + ConnectionPolicy entities
using Humanizer;
using Microsoft.EntityFrameworkCore;

namespace ArchiX.Library.Context
{
    /// <summary>ArchiX EF Core DbContext.</summary>
    public partial class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public int DraftStatusId { get; private set; }
        public int AwaitingApprovalStatusId { get; private set; }
        public int ApprovedStatusId { get; private set; }
        public int RejectedStatusId { get; private set; }
        public int PassiveStatusId { get; private set; }
        public int DeletedStatusId { get; private set; }

        // ConnectionPolicy DbSets (single declarations)
        public DbSet<ArchiXSetting> ArchiXSettings => Set<ArchiXSetting>();
        public DbSet<ConnectionServerWhitelist> ConnectionServerWhitelist => Set<ConnectionServerWhitelist>();
        public DbSet<ConnectionAudit> ConnectionAudits => Set<ConnectionAudit>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var asm = typeof(AppDbContext).Assembly;

            //1) BaseEntity tiplerini tara; MapToDb=false olanları ignore et; kalanları çoğul tabloya map et
            var entityTypes = asm.GetTypes()
                .Where(t => typeof(BaseEntity).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract);

            foreach (var t in entityTypes)
            {
                var include = true;

                var fi = t.GetField(nameof(BaseEntity.MapToDb),
                                    BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                         ?? t.GetField(nameof(BaseEntity.MapToDb),
                                    BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

                if (fi?.FieldType == typeof(bool))
                    include = (bool)fi.GetValue(null)!;

                if (!include)
                {
                    modelBuilder.Ignore(t);
                    continue;
                }

                modelBuilder.Entity(t).ToTable(t.Name.Pluralize());
            }

            //2) BaseEntity ortak kolonlar + (Statu hariç) StatusId→Statu.Id FK
            foreach (var et in modelBuilder.Model.GetEntityTypes()
                         .Where(et => typeof(BaseEntity).IsAssignableFrom(et.ClrType)
                                  && et.ClrType != typeof(BaseEntity)))
            {
                modelBuilder.Entity(et.ClrType, b =>
                {
                    b.Property<int>(nameof(BaseEntity.Id))
                     .ValueGeneratedOnAdd()
                     .UseIdentityColumn(1,1);

                    b.Property<Guid>(nameof(BaseEntity.RowId))
                     .HasDefaultValueSql("NEWSEQUENTIALID()")
                     .ValueGeneratedOnAdd();

                    b.Property<DateTimeOffset>(nameof(BaseEntity.CreatedAt))
                     .HasDefaultValueSql("SYSDATETIMEOFFSET()")
                     .HasPrecision(4);

                    b.Property<int>(nameof(BaseEntity.CreatedBy));
                    b.Property<DateTimeOffset?>(nameof(BaseEntity.UpdatedAt)).HasPrecision(4);
                    b.Property<int?>(nameof(BaseEntity.UpdatedBy));

                    b.Property<DateTimeOffset?>(nameof(BaseEntity.LastStatusAt))
                     .HasDefaultValueSql("SYSDATETIMEOFFSET()")
                     .HasPrecision(4);

                    b.Property<int>(nameof(BaseEntity.LastStatusBy));

                    if (et.ClrType != typeof(Statu))
                    {
                        b.Property<int>(nameof(BaseEntity.StatusId)).IsRequired();
                        b.HasIndex(nameof(BaseEntity.StatusId));
                        b.HasOne(typeof(Statu))
                         .WithMany()
                         .HasForeignKey(nameof(BaseEntity.StatusId))
                         .OnDelete(DeleteBehavior.Restrict);
                    }
                });
            }

            // 3) Statu — Code unique, self-FK yok
            modelBuilder.Entity<Statu>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).UseIdentityColumn(1, 1);
                e.Property(x => x.Code).IsRequired();
                e.Property(x => x.Name).IsRequired();
                e.HasIndex(x => x.Code).IsUnique();
                e.Property<int>(nameof(BaseEntity.StatusId));
                e.HasIndex(nameof(BaseEntity.StatusId));
            });

            // 4) FilterItem — Unique(ItemType, Code)
            modelBuilder.Entity<FilterItem>(e =>
            {
                e.HasKey(f => f.Id);
                e.Property(f => f.ItemType).IsRequired().HasMaxLength(50);
                e.Property(f => f.Code).IsRequired().HasMaxLength(50);
                e.HasIndex(f => new { f.ItemType, f.Code }).IsUnique();
            });

            // 5) LanguagePack — Unique(ItemType, EntityName, FieldName, Code, Culture)
            modelBuilder.Entity<LanguagePack>(e =>
            {
                e.HasKey(lp => lp.Id);
                e.Property(lp => lp.ItemType).IsRequired();
                e.Property(lp => lp.Code).IsRequired();
                e.Property(lp => lp.Culture).IsRequired();
                e.HasIndex(lp => new { lp.ItemType, lp.EntityName, lp.FieldName, lp.Code, lp.Culture }).IsUnique();
            });

            // 6) Global soft-delete filtresi (Statu hariç)
            foreach (var et in modelBuilder.Model.GetEntityTypes()
                         .Where(et => typeof(BaseEntity).IsAssignableFrom(et.ClrType)
                                  && et.ClrType != typeof(BaseEntity)
                                  && et.ClrType != typeof(Statu)))
            {
                var entityType = et.ClrType;
                var p = System.Linq.Expressions.Expression.Parameter(entityType, "e");
                var statusProp = System.Linq.Expressions.Expression.Property(p, nameof(BaseEntity.StatusId));
                var ctxConst = System.Linq.Expressions.Expression.Constant(this);
                var delProp = System.Linq.Expressions.Expression.Property(ctxConst, nameof(DeletedStatusId));
                var body = System.Linq.Expressions.Expression.NotEqual(statusProp, delProp);
                var lambdaType = typeof(Func<,>).MakeGenericType(entityType, typeof(bool));
                var lambda = System.Linq.Expressions.Expression.Lambda(lambdaType, body, p);
                modelBuilder.Entity(entityType).HasQueryFilter(lambda);
            }

            // 7) ArchiXSetting — tablo adı globalde otomatik çoğul
            modelBuilder.Entity<ArchiXSetting>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Key).IsRequired().HasMaxLength(200);
                e.Property(x => x.Group).HasMaxLength(50);
                e.Property(x => x.Description).HasMaxLength(250);
                e.Property(x => x.UpdatedAt).HasPrecision(4);
                e.HasIndex(x => x.Key).IsUnique();
                e.HasIndex(x => x.Group);
            });

            // 8) ConnectionServerWhitelist — tablo adı globalde otomatik çoğul
                modelBuilder.Entity<ConnectionServerWhitelist>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.ServerName).HasMaxLength(200);
                e.Property(x => x.Cidr).HasMaxLength(43);
                e.Property(x => x.EnvScope).HasMaxLength(20);

                // Check constraint — yeni stil (obsoletion yok)
                e.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_Whitelist_ServerOrCidr",
                        "[ServerName] IS NOT NULL OR [Cidr] IS NOT NULL");
                });

                e.HasIndex(x => new { x.ServerName, x.IsActive });
                e.HasIndex(x => new { x.Cidr, x.IsActive });
                e.HasIndex(x => x.EnvScope);
                // İhtiyaç olursa: e.HasIndex(x => x.CreatedAt);
            });

            // 9) ConnectionAudit — tablo adı globalde otomatik çoğul
            modelBuilder.Entity<ConnectionAudit>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.AttemptedAt).HasPrecision(4);
                e.Property(x => x.NormalizedServer).HasMaxLength(200).IsRequired();
                e.Property(x => x.Mode).HasMaxLength(10).IsRequired();
                e.Property(x => x.Result).HasMaxLength(10).IsRequired();
                e.Property(x => x.ReasonCode).HasMaxLength(50);
                e.Property(x => x.RawConnectionMasked).HasMaxLength(1024).IsRequired();
                e.HasIndex(x => x.AttemptedAt);
                e.HasIndex(x => x.Result);
                e.HasIndex(x => x.CorrelationId);
            });
        }

        /// <summary>Çekirdek seed: Statu, FilterItem, LanguagePack. Id verilmez, Code bazlı idempotent.</summary>
        public async Task EnsureCoreSeedsAndBindAsync(CancellationToken ct = default)
        {
            await using var tx = await Database.BeginTransactionAsync(ct);

            // 1) Statu
            var statusItems = new[]
            {
                new Statu { Code = "DFT", Name = "Draft",              Description = "Record is in draft state" },
                new Statu { Code = "AWT", Name = "Awaiting Approval",  Description = "Record is waiting for approval" },
                new Statu { Code = "APR", Name = "Approved",           Description = "Record has been approved" },
                new Statu { Code = "REJ", Name = "Rejected",           Description = "Record has been rejected" },
                new Statu { Code = "PSV", Name = "Passive",            Description = "Record is passive / inactive" },
                new Statu { Code = "DEL", Name = "Deleted",            Description = "Record has been deleted" },
            };

            var existingStatusCodes = await Set<Statu>().AsNoTracking()
                .Select(x => x.Code).ToListAsync(ct);

            foreach (var s in statusItems)
            {
                if (!existingStatusCodes.Contains(s.Code))
                {
                    Add(s);
                }
                else
                {
                    var e = await Set<Statu>().SingleAsync(x => x.Code == s.Code, ct);
                    e.Name = s.Name;
                    e.Description = s.Description;
                }
            }
            await SaveChangesAsync(ct);

            // 2) Statü Id bağla
            DraftStatusId = await Set<Statu>().Where(x => x.Code == "DFT").Select(x => x.Id).SingleAsync(ct);
            AwaitingApprovalStatusId = await Set<Statu>().Where(x => x.Code == "AWT").Select(x => x.Id).SingleAsync(ct);
            ApprovedStatusId = await Set<Statu>().Where(x => x.Code == "APR").Select(x => x.Id).SingleAsync(ct);
            RejectedStatusId = await Set<Statu>().Where(x => x.Code == "REJ").Select(x => x.Id).SingleAsync(ct);
            PassiveStatusId = await Set<Statu>().Where(x => x.Code == "PSV").Select(x => x.Id).SingleAsync(ct);
            DeletedStatusId = await Set<Statu>().Where(x => x.Code == "DEL").Select(x => x.Id).SingleAsync(ct);

            // 3) FilterItem — StatusId = Approved
            var filterItems = new[]
            {
                new FilterItem { ItemType = "Operator", Code = "Equals",              StatusId = ApprovedStatusId },
                new FilterItem { ItemType = "Operator", Code = "NotEquals",           StatusId = ApprovedStatusId },
                new FilterItem { ItemType = "Operator", Code = "StartsWith",          StatusId = ApprovedStatusId },
                new FilterItem { ItemType = "Operator", Code = "NotStartsWith",       StatusId = ApprovedStatusId },
                new FilterItem { ItemType = "Operator", Code = "EndsWith",            StatusId = ApprovedStatusId },
                new FilterItem { ItemType = "Operator", Code = "NotEndsWith",         StatusId = ApprovedStatusId },
                new FilterItem { ItemType = "Operator", Code = "Contains",            StatusId = ApprovedStatusId },
                new FilterItem { ItemType = "Operator", Code = "NotContains",         StatusId = ApprovedStatusId },
                new FilterItem { ItemType = "Operator", Code = "Between",             StatusId = ApprovedStatusId },
                new FilterItem { ItemType = "Operator", Code = "NotBetween",          StatusId = ApprovedStatusId },
                new FilterItem { ItemType = "Operator", Code = "GreaterThan",         StatusId = ApprovedStatusId },
                new FilterItem { ItemType = "Operator", Code = "GreaterThanOrEqual",  StatusId = ApprovedStatusId },
                new FilterItem { ItemType = "Operator", Code = "LessThan",            StatusId = ApprovedStatusId },
                new FilterItem { ItemType = "Operator", Code = "LessThanOrEqual",     StatusId = ApprovedStatusId },
                new FilterItem { ItemType = "Operator", Code = "In",                  StatusId = ApprovedStatusId },
                new FilterItem { ItemType = "Operator", Code = "NotIn",               StatusId = ApprovedStatusId },
                new FilterItem { ItemType = "Operator", Code = "IsNull",              StatusId = ApprovedStatusId },
                new FilterItem { ItemType = "Operator", Code = "IsNotNull",           StatusId = ApprovedStatusId },
            };

            var existingFilters = await Set<FilterItem>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Select(x => new { x.ItemType, x.Code })
                .ToListAsync(ct);

            foreach (var fi in filterItems)
            {
                if (!existingFilters.Any(x => x.ItemType == fi.ItemType && x.Code == fi.Code))
                    Add(fi);
            }
            await SaveChangesAsync(ct);

            // 4) LanguagePack — TR & EN
            var languagePacks = new[]
            {
                // Equals / NotEquals
                new LanguagePack { ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "Equals",              Culture = "tr-TR", DisplayName = "Eşittir",                 Description = "Değer belirtilene eşit olmalı",                 StatusId = ApprovedStatusId },
                new LanguagePack { ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "Equals",              Culture = "en-US", DisplayName = "Equals",                  Description = "Value must be equal to the given one",         StatusId = ApprovedStatusId },
                new LanguagePack { ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "NotEquals",           Culture = "tr-TR", DisplayName = "Eşit Değil",              Description = "Değer belirtilene eşit olmamalı",              StatusId = ApprovedStatusId },
                new LanguagePack { ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "NotEquals",           Culture = "en-US", DisplayName = "Not Equal",               Description = "Value must not be equal to the given one",     StatusId = ApprovedStatusId },

                // StartsWith / NotStartsWith
                new LanguagePack { ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "StartsWith",          Culture = "tr-TR", DisplayName = "İle Başlar",               Description = "Değer belirtilen metinle başlamalı",          StatusId = ApprovedStatusId },
                new LanguagePack { ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "StartsWith",          Culture = "en-US", DisplayName = "Starts With",             Description = "Value must start with the given text",         StatusId = ApprovedStatusId },
                new LanguagePack { ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "NotStartsWith",       Culture = "tr-TR", DisplayName = "İle Başlamaz",            Description = "Değer belirtilen metinle başlamamalı",        StatusId = ApprovedStatusId },
                new LanguagePack { ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "NotStartsWith",       Culture = "en-US", DisplayName = "Not Starts With",         Description = "Value must not start with the given text",     StatusId = ApprovedStatusId },

                // EndsWith / NotEndsWith
                new LanguagePack { ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "EndsWith",            Culture = "tr-TR", DisplayName = "İle Biter",               Description = "Değer belirtilen metinle bitmeli",            StatusId = ApprovedStatusId },
                new LanguagePack { ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "EndsWith",            Culture = "en-US", DisplayName = "Ends With",               Description = "Value must end with the given text",           StatusId = ApprovedStatusId },
                new LanguagePack { ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "NotEndsWith",         Culture = "tr-TR", DisplayName = "İle Bitmez",              Description = "Değer belirtilen metinle bitmemeli",          StatusId = ApprovedStatusId },
                new LanguagePack { ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "NotEndsWith",         Culture = "en-US", DisplayName = "Not Ends With",           Description = "Value must not end with the given text",       StatusId = ApprovedStatusId },

                // Contains / NotContains
                new LanguagePack { ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "Contains",            Culture = "tr-TR", DisplayName = "İçerir",                  Description = "Değer metni içermeli",                         StatusId = ApprovedStatusId },
                new LanguagePack { ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "Contains",            Culture = "en-US", DisplayName = "Contains",                Description = "Value must contain the text",                  StatusId = ApprovedStatusId },
                new LanguagePack { ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "NotContains",         Culture = "tr-TR", DisplayName = "İçermez",                 Description = "Değer metni içermemeli",                       StatusId = ApprovedStatusId },
                new LanguagePack { ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "NotContains",         Culture = "en-US", DisplayName = "Does Not Contain",        Description = "Value must not contain the text",              StatusId = ApprovedStatusId },

                // Between / NotBetween
                new LanguagePack { ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "Between",             Culture = "tr-TR", DisplayName = "Arasında",                Description = "Değer aralık içinde olmalı",                  StatusId = ApprovedStatusId },
                new LanguagePack { ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "Between",             Culture = "en-US", DisplayName = "Between",                 Description = "Value must be within the range",               StatusId = ApprovedStatusId },
                new LanguagePack { ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "NotBetween",          Culture = "tr-TR", DisplayName = "Arasında Değil",          Description = "Değer aralık dışında olmalı",                 StatusId = ApprovedStatusId },
                new LanguagePack { ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "NotBetween",          Culture = "en-US", DisplayName = "Not Between",             Description = "Value must be outside the range",              StatusId = ApprovedStatusId },

                // Greater / Less
                new LanguagePack { ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "GreaterThan",         Culture = "tr-TR", DisplayName = "Büyüktür",                Description = "Değer belirtilenden büyük olmalı",            StatusId = ApprovedStatusId },
                new LanguagePack { ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "GreaterThan",         Culture = "en-US", DisplayName = "Greater Than",            Description = "Value must be greater than the given one",     StatusId = ApprovedStatusId },
                new LanguagePack { ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "GreaterThanOrEqual",  Culture = "tr-TR", DisplayName = "Büyük Eşittir",           Description = "Değer belirtilenden büyük veya eşit olmalı",  StatusId = ApprovedStatusId },
                new LanguagePack { ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "GreaterThanOrEqual",  Culture = "en-US", DisplayName = "Greater Than Or Equal",   Description = "Value must be greater than or equal to",       StatusId = ApprovedStatusId },
                new LanguagePack { ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "LessThan",            Culture = "tr-TR", DisplayName = "Küçüktür",                Description = "Değer belirtilenden küçük olmalı",            StatusId = ApprovedStatusId },
                new LanguagePack { ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "LessThan",            Culture = "en-US", DisplayName = "Less Than",               Description = "Value must be less than the given one",        StatusId = ApprovedStatusId },
                new LanguagePack { ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "LessThanOrEqual",     Culture = "tr-TR", DisplayName = "Küçük Eşittir",           Description = "Değer belirtilenden küçük veya eşit olmalı",  StatusId = ApprovedStatusId },
                new LanguagePack { ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "LessThanOrEqual",     Culture = "en-US", DisplayName = "Less Than Or Equal",      Description = "Value must be less than or equal to",          StatusId = ApprovedStatusId },

                // In / NotIn
                new LanguagePack { ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "In",                  Culture = "tr-TR", DisplayName = "İçinde",                  Description = "Değer listede olmalı",                        StatusId = ApprovedStatusId },
                new LanguagePack { ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "In",                  Culture = "en-US", DisplayName = "In",                      Description = "Value must be in the list",                   StatusId = ApprovedStatusId },
                new LanguagePack { ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "NotIn",               Culture = "tr-TR", DisplayName = "İçinde Değil",            Description = "Değer listede olmamalı",                      StatusId = ApprovedStatusId },
                new LanguagePack { ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "NotIn",               Culture = "en-US", DisplayName = "Not In",                  Description = "Value must not be in the list",               StatusId = ApprovedStatusId },

                // IsNull / IsNotNull
                new LanguagePack { ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "IsNull",              Culture = "tr-TR", DisplayName = "Boş",                     Description = "Değer null ya da boş olmalı",                  StatusId = ApprovedStatusId },
                new LanguagePack { ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "IsNull",              Culture = "en-US", DisplayName = "Is Null/Empty",           Description = "Value must be null or empty",                  StatusId = ApprovedStatusId },
                new LanguagePack { ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "IsNotNull",           Culture = "tr-TR", DisplayName = "Boş Değil",               Description = "Değer null değil ve boş değil olmalı",         StatusId = ApprovedStatusId },
                new LanguagePack { ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "IsNotNull",           Culture = "en-US", DisplayName = "Is Not Null/Empty",       Description = "Value is not null and not empty",              StatusId = ApprovedStatusId },
            };

            var existingLangs = await Set<LanguagePack>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Select(x => new { x.ItemType, x.EntityName, x.FieldName, x.Code, x.Culture })
                .ToListAsync(ct);

            foreach (var lp in languagePacks)
            {
                bool exists = existingLangs.Any(x =>
                    x.ItemType == lp.ItemType &&
                    x.EntityName == lp.EntityName &&
                    x.FieldName == lp.FieldName &&
                    x.Code == lp.Code &&
                    x.Culture == lp.Culture);

                if (!exists) Add(lp);
                else
                {
                    var e = await Set<LanguagePack>().SingleAsync(x =>
                        x.ItemType == lp.ItemType &&
                        x.EntityName == lp.EntityName &&
                        x.FieldName == lp.FieldName &&
                        x.Code == lp.Code &&
                        x.Culture == lp.Culture, ct);

                    e.DisplayName = lp.DisplayName;
                    e.Description = lp.Description;
                }
            }

            await SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }
    }
}

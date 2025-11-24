// File: src / ArchiX.Library / Context / AppDbContext.cs

using System.Reflection;

using ArchiX.Library.Entities;

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

        // DbSets
        public DbSet<ConnectionServerWhitelist> ConnectionServerWhitelist => Set<ConnectionServerWhitelist>();
        public DbSet<ConnectionAudit> ConnectionAudits => Set<ConnectionAudit>();
        public DbSet<ParameterDataType> ParameterDataTypes => Set<ParameterDataType>();
        public DbSet<Parameter> Parameters => Set<Parameter>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Varsayılan collation (sunucu destekliyorsa)
            modelBuilder.UseCollation("Latin1_General_100_CI_AS_SC_UTF8");
            base.OnModelCreating(modelBuilder);

            var asm = typeof(AppDbContext).Assembly;

            // 1) BaseEntity türevlerini bul, MapToDb=false ise ignore et ve tablo adını çoğulla.
            var entityTypes = asm.GetTypes()
                .Where(t => typeof(BaseEntity).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
                .OrderBy(t => t.FullName);   // <-- ek satır;

            foreach (var t in entityTypes)
            {
                var fi = t.GetField(nameof(BaseEntity.MapToDb),
                                    BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                         ?? t.GetField(nameof(BaseEntity.MapToDb),
                                    BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

                var include = fi?.FieldType == typeof(bool) ? (bool)fi.GetValue(null)! : true;
                if (!include)
                {
                    modelBuilder.Ignore(t);
                    continue;
                }
                modelBuilder.Entity(t).ToTable(t.Name.Pluralize());
            }

            // 2) Ortak kolonlar + (Statu hariç) StatusId→Statu.Id FK
            foreach (var et in modelBuilder.Model.GetEntityTypes()
                         .Where(et => typeof(BaseEntity).IsAssignableFrom(et.ClrType)
                                   && et.ClrType != typeof(BaseEntity)))
            {
                modelBuilder.Entity(et.ClrType, b =>
                {
                    b.Property<int>(nameof(BaseEntity.Id))
                     .ValueGeneratedOnAdd()
                     .UseIdentityColumn(1, 1);

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

            // 3) Statu
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

            // 4) FilterItem
            modelBuilder.Entity<FilterItem>(e =>
            {
                e.HasKey(f => f.Id);
                e.Property(f => f.ItemType).IsRequired().HasMaxLength(50);
                e.Property(f => f.Code).IsRequired().HasMaxLength(50);
                e.HasIndex(f => new { f.ItemType, f.Code }).IsUnique();
            });

            // 5) LanguagePack
            modelBuilder.Entity<LanguagePack>(e =>
            {
                e.HasKey(lp => lp.Id);
                e.Property(lp => lp.ItemType).IsRequired();
                e.Property(lp => lp.Code).IsRequired();
                e.Property(lp => lp.Culture).IsRequired();
                e.HasIndex(lp => new { lp.ItemType, lp.EntityName, lp.FieldName, lp.Code, lp.Culture }).IsUnique();
            });

            // 6) Soft-delete filtresi (DeletedStatusId != StatusId). Statu hariç.
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

            // 7) ConnectionServerWhitelist
            modelBuilder.Entity<ConnectionServerWhitelist>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.ServerName).HasMaxLength(200);
                e.Property(x => x.Cidr).HasMaxLength(43);
                e.Property(x => x.EnvScope).HasMaxLength(20);

                e.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_Whitelist_ServerOrCidr",
                        "[ServerName] IS NOT NULL OR [Cidr] IS NOT NULL");
                });

                e.HasIndex(x => new { x.ServerName, x.IsActive });
                e.HasIndex(x => new { x.Cidr, x.IsActive });
                e.HasIndex(x => x.EnvScope);
            });

            // 8) ConnectionAudit
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

            // 9) ParameterDataType
            modelBuilder.Entity<ParameterDataType>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Code).IsRequired();
                e.Property(x => x.Name).IsRequired().HasMaxLength(100);
                e.Property(x => x.Category).HasMaxLength(20);
                e.Property(x => x.Description).IsRequired().HasMaxLength(500);
                e.HasIndex(x => x.Code).IsUnique();
                e.HasIndex(x => x.Name).IsUnique();
            });

            // 10) Parameter
            modelBuilder.Entity<Parameter>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Group).IsRequired().HasMaxLength(75);
                e.Property(x => x.Key).IsRequired().HasMaxLength(150);
                e.Property(x => x.Description).IsRequired().HasMaxLength(500);
                e.HasIndex(x => new { x.Group, x.Key }).IsUnique();

                e.HasOne(x => x.DataType)
                 .WithMany()
                 .HasForeignKey(x => x.ParameterDataTypeId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // --- Statik çekirdek seed (HasData) ---
            // Statu (StatusId -> Approved = 3)
            modelBuilder.Entity<Statu>().HasData(
                new Statu { Id = 1, Code = "DFT", Name = "Draft", Description = "Record is in draft state", StatusId = BaseEntity.ApprovedStatusId },
                new Statu { Id = 2, Code = "AWT", Name = "Awaiting Approval", Description = "Record is waiting for approval", StatusId = BaseEntity.ApprovedStatusId },
                new Statu { Id = 3, Code = "APR", Name = "Approved", Description = "Record has been approved", StatusId = BaseEntity.ApprovedStatusId },
                new Statu { Id = 4, Code = "REJ", Name = "Rejected", Description = "Record has been rejected", StatusId = BaseEntity.ApprovedStatusId },
                new Statu { Id = 5, Code = "PSV", Name = "Passive", Description = "Record is passive / inactive", StatusId = BaseEntity.ApprovedStatusId },
                new Statu { Id = 6, Code = "DEL", Name = "Deleted", Description = "Record has been deleted", StatusId = BaseEntity.ApprovedStatusId }
            );

            // FilterItem (Approved=3)
            modelBuilder.Entity<FilterItem>().HasData(
                new FilterItem { Id = 1, ItemType = "Operator", Code = "Equals", StatusId = 3 },
                new FilterItem { Id = 2, ItemType = "Operator", Code = "NotEquals", StatusId = 3 },
                new FilterItem { Id = 3, ItemType = "Operator", Code = "StartsWith", StatusId = 3 },
                new FilterItem { Id = 4, ItemType = "Operator", Code = "NotStartsWith", StatusId = 3 },
                new FilterItem { Id = 5, ItemType = "Operator", Code = "EndsWith", StatusId = 3 },
                new FilterItem { Id = 6, ItemType = "Operator", Code = "NotEndsWith", StatusId = 3 },
                new FilterItem { Id = 7, ItemType = "Operator", Code = "Contains", StatusId = 3 },
                new FilterItem { Id = 8, ItemType = "Operator", Code = "NotContains", StatusId = 3 },
                new FilterItem { Id = 9, ItemType = "Operator", Code = "Between", StatusId = 3 },
                new FilterItem { Id = 10, ItemType = "Operator", Code = "NotBetween", StatusId = 3 },
                new FilterItem { Id = 11, ItemType = "Operator", Code = "GreaterThan", StatusId = 3 },
                new FilterItem { Id = 12, ItemType = "Operator", Code = "GreaterThanOrEqual", StatusId = 3 },
                new FilterItem { Id = 13, ItemType = "Operator", Code = "LessThan", StatusId = 3 },
                new FilterItem { Id = 14, ItemType = "Operator", Code = "LessThanOrEqual", StatusId = 3 },
                new FilterItem { Id = 15, ItemType = "Operator", Code = "In", StatusId = 3 },
                new FilterItem { Id = 16, ItemType = "Operator", Code = "NotIn", StatusId = 3 },
                new FilterItem { Id = 17, ItemType = "Operator", Code = "IsNull", StatusId = 3 },
                new FilterItem { Id = 18, ItemType = "Operator", Code = "IsNotNull", StatusId = 3 }
            );

            // LanguagePack (Operator + Status, tr-TR & en-US)
            modelBuilder.Entity<LanguagePack>().HasData(
                // Operators (existing)
                new LanguagePack { Id = 1, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "Equals", Culture = "tr-TR", DisplayName = "Eşittir", Description = "Değer belirtilene eşit olmalı", StatusId = 3 },
                new LanguagePack { Id = 2, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "Equals", Culture = "en-US", DisplayName = "Equals", Description = "Value must be equal to the given one", StatusId = 3 },
                new LanguagePack { Id = 3, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "NotEquals", Culture = "tr-TR", DisplayName = "Eşit Değil", Description = "Değer belirtilene eşit olmamalı", StatusId = 3 },
                new LanguagePack { Id = 4, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "NotEquals", Culture = "en-US", DisplayName = "Not Equal", Description = "Value must not be equal to the given one", StatusId = 3 },

                // Operators (new)
                new LanguagePack { Id = 5, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "StartsWith", Culture = "tr-TR", DisplayName = "Başlar", Description = "Değer belirtilen ifadeyle başlamalı", StatusId = 3 },
                new LanguagePack { Id = 6, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "StartsWith", Culture = "en-US", DisplayName = "Starts With", Description = "Value must start with the given text", StatusId = 3 },
                new LanguagePack { Id = 7, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "NotStartsWith", Culture = "tr-TR", DisplayName = "Başlamaz", Description = "Değer belirtilen ifadeyle başlamamalı", StatusId = 3 },
                new LanguagePack { Id = 8, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "NotStartsWith", Culture = "en-US", DisplayName = "Does Not Start With", Description = "Value must not start with the given text", StatusId = 3 },
                new LanguagePack { Id = 9, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "EndsWith", Culture = "tr-TR", DisplayName = "Biter", Description = "Değer belirtilen ifadeyle bitmeli", StatusId = 3 },
                new LanguagePack { Id = 10, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "EndsWith", Culture = "en-US", DisplayName = "Ends With", Description = "Value must end with the given text", StatusId = 3 },
                new LanguagePack { Id = 11, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "NotEndsWith", Culture = "tr-TR", DisplayName = "Bitmez", Description = "Değer belirtilen ifadeyle bitmemeli", StatusId = 3 },
                new LanguagePack { Id = 12, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "NotEndsWith", Culture = "en-US", DisplayName = "Does Not End With", Description = "Value must not end with the given text", StatusId = 3 },
                new LanguagePack { Id = 13, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "Contains", Culture = "tr-TR", DisplayName = "İçerir", Description = "Değer belirtilen ifadeyi içermeli", StatusId = 3 },
                new LanguagePack { Id = 14, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "Contains", Culture = "en-US", DisplayName = "Contains", Description = "Value must contain the given text", StatusId = 3 },
                new LanguagePack { Id = 15, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "NotContains", Culture = "tr-TR", DisplayName = "İçermez", Description = "Değer belirtilen ifadeyi içermemeli", StatusId = 3 },
                new LanguagePack { Id = 16, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "NotContains", Culture = "en-US", DisplayName = "Does Not Contain", Description = "Value must not contain the given text", StatusId = 3 },
                new LanguagePack { Id = 17, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "Between", Culture = "tr-TR", DisplayName = "Arasında", Description = "Değer alt ve üst sınırlar arasında (dahil) olmalı", StatusId = 3 },
                new LanguagePack { Id = 18, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "Between", Culture = "en-US", DisplayName = "Between", Description = "Value must be between lower and upper bounds (inclusive)", StatusId = 3 },
                new LanguagePack { Id = 19, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "NotBetween", Culture = "tr-TR", DisplayName = "Arasında Değil", Description = "Değer alt ve üst sınırlar arasında olmamalı", StatusId = 3 },
                new LanguagePack { Id = 20, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "NotBetween", Culture = "en-US", DisplayName = "Not Between", Description = "Value must not be between the given bounds", StatusId = 3 },
                new LanguagePack { Id = 21, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "GreaterThan", Culture = "tr-TR", DisplayName = "Büyüktür", Description = "Değer belirtilenden büyük olmalı", StatusId = 3 },
                new LanguagePack { Id = 22, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "GreaterThan", Culture = "en-US", DisplayName = "Greater Than", Description = "Value must be greater than the given one", StatusId = 3 },
                new LanguagePack { Id = 23, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "GreaterThanOrEqual", Culture = "tr-TR", DisplayName = "Büyük veya Eşittir", Description = "Değer belirtilenden büyük veya eşit olmalı", StatusId = 3 },
                new LanguagePack { Id = 24, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "GreaterThanOrEqual", Culture = "en-US", DisplayName = "Greater Than Or Equal", Description = "Value must be greater than or equal to the given one", StatusId = 3 },
                new LanguagePack { Id = 25, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "LessThan", Culture = "tr-TR", DisplayName = "Küçüktür", Description = "Değer belirtilenden küçük olmalı", StatusId = 3 },
                new LanguagePack { Id = 26, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "LessThan", Culture = "en-US", DisplayName = "Less Than", Description = "Value must be less than the given one", StatusId = 3 },
                new LanguagePack { Id = 27, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "LessThanOrEqual", Culture = "tr-TR", DisplayName = "Küçük veya Eşittir", Description = "Değer belirtilenden küçük veya eşit olmalı", StatusId = 3 },
                new LanguagePack { Id = 28, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "LessThanOrEqual", Culture = "en-US", DisplayName = "Less Than Or Equal", Description = "Value must be less than or equal to the given one", StatusId = 3 },
                new LanguagePack { Id = 29, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "In", Culture = "tr-TR", DisplayName = "İçinde", Description = "Değer verilen listedeki öğelerden biri olmalı", StatusId = 3 },
                new LanguagePack { Id = 30, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "In", Culture = "en-US", DisplayName = "In Set", Description = "Value must be one of the provided list items", StatusId = 3 },
                new LanguagePack { Id = 31, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "NotIn", Culture = "tr-TR", DisplayName = "İçinde Değil", Description = "Değer verilen listedeki öğelerden biri olmamalı", StatusId = 3 },
                new LanguagePack { Id = 32, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "NotIn", Culture = "en-US", DisplayName = "Not In Set", Description = "Value must not be any of the provided list items", StatusId = 3 },
                new LanguagePack { Id = 33, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "IsNull", Culture = "tr-TR", DisplayName = "Boş (Null)", Description = "Değer null olmalı", StatusId = 3 },
                new LanguagePack { Id = 34, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "IsNull", Culture = "en-US", DisplayName = "Is Null", Description = "Value must be null", StatusId = 3 },
                new LanguagePack { Id = 35, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "IsNotNull", Culture = "tr-TR", DisplayName = "Boş Değil", Description = "Değer null olmamalı", StatusId = 3 },
                new LanguagePack { Id = 36, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "IsNotNull", Culture = "en-US", DisplayName = "Is Not Null", Description = "Value must not be null", StatusId = 3 },

                // Statuses
                new LanguagePack { Id = 37, ItemType = "Status", EntityName = "Statu", FieldName = "Code", Code = "DFT", Culture = "tr-TR", DisplayName = "Taslak", Description = "Kayıt taslak durumunda", StatusId = 3 },
                new LanguagePack { Id = 38, ItemType = "Status", EntityName = "Statu", FieldName = "Code", Code = "DFT", Culture = "en-US", DisplayName = "Draft", Description = "Record is in draft state", StatusId = 3 },
                new LanguagePack { Id = 39, ItemType = "Status", EntityName = "Statu", FieldName = "Code", Code = "AWT", Culture = "tr-TR", DisplayName = "Onay Bekliyor", Description = "Kayıt onay bekliyor", StatusId = 3 },
                new LanguagePack { Id = 40, ItemType = "Status", EntityName = "Statu", FieldName = "Code", Code = "AWT", Culture = "en-US", DisplayName = "Awaiting Approval", Description = "Record is waiting for approval", StatusId = 3 },
                new LanguagePack { Id = 41, ItemType = "Status", EntityName = "Statu", FieldName = "Code", Code = "APR", Culture = "tr-TR", DisplayName = "Onaylandı", Description = "Kayıt onaylandı", StatusId = 3 },
                new LanguagePack { Id = 42, ItemType = "Status", EntityName = "Statu", FieldName = "Code", Code = "APR", Culture = "en-US", DisplayName = "Approved", Description = "Record has been approved", StatusId = 3 },
                new LanguagePack { Id = 43, ItemType = "Status", EntityName = "Statu", FieldName = "Code", Code = "REJ", Culture = "tr-TR", DisplayName = "Reddedildi", Description = "Kayıt reddedildi", StatusId = 3 },
                new LanguagePack { Id = 44, ItemType = "Status", EntityName = "Statu", FieldName = "Code", Code = "REJ", Culture = "en-US", DisplayName = "Rejected", Description = "Record has been rejected", StatusId = 3 },
                new LanguagePack { Id = 45, ItemType = "Status", EntityName = "Statu", FieldName = "Code", Code = "PSV", Culture = "tr-TR", DisplayName = "Pasif", Description = "Kayıt pasif / devre dışı", StatusId = 3 },
                new LanguagePack { Id = 46, ItemType = "Status", EntityName = "Statu", FieldName = "Code", Code = "PSV", Culture = "en-US", DisplayName = "Passive", Description = "Record is passive / inactive", StatusId = 3 },
                new LanguagePack { Id = 47, ItemType = "Status", EntityName = "Statu", FieldName = "Code", Code = "DEL", Culture = "tr-TR", DisplayName = "Silindi", Description = "Kayıt silinmiş durumda", StatusId = 3 },
                new LanguagePack { Id = 48, ItemType = "Status", EntityName = "Statu", FieldName = "Code", Code = "DEL", Culture = "en-US", DisplayName = "Deleted", Description = "Record has been deleted", StatusId = 3 }
            );
            // ParameterDataType
            modelBuilder.Entity<ParameterDataType>().HasData(
                new ParameterDataType { Id = 1, Code = 60, Name = "NVarChar_50", Category = "NVarChar", Description = "NVARCHAR logical length 50", StatusId = 3 },
                new ParameterDataType { Id = 2, Code = 70, Name = "NVarChar_100", Category = "NVarChar", Description = "NVARCHAR logical length 100", StatusId = 3 },
                new ParameterDataType { Id = 3, Code = 80, Name = "NVarChar_250", Category = "NVarChar", Description = "NVARCHAR logical length 250", StatusId = 3 },
                new ParameterDataType { Id = 4, Code = 90, Name = "NVarChar_500", Category = "NVarChar", Description = "NVARCHAR logical length 500", StatusId = 3 },
                new ParameterDataType { Id = 5, Code = 100, Name = "NVarChar_Max", Category = "NVarChar", Description = "NVARCHAR(MAX) logical length", StatusId = 3 },
                new ParameterDataType { Id = 6, Code = 200, Name = "Byte", Category = "Numeric", Description = "Unsigned 8-bit integer", StatusId = 3 },
                new ParameterDataType { Id = 7, Code = 210, Name = "SmallInt", Category = "Numeric", Description = "16-bit integer", StatusId = 3 },
                new ParameterDataType { Id = 8, Code = 220, Name = "Int", Category = "Numeric", Description = "32-bit integer", StatusId = 3 },
                new ParameterDataType { Id = 9, Code = 230, Name = "BigInt", Category = "Numeric", Description = "64-bit integer", StatusId = 3 },
                new ParameterDataType { Id = 10, Code = 240, Name = "Decimal18_6", Category = "Numeric", Description = "Decimal(18,6)", StatusId = 3 },
                new ParameterDataType { Id = 11, Code = 300, Name = "Date", Category = "Temporal", Description = "ISO date", StatusId = 3 },
                new ParameterDataType { Id = 12, Code = 310, Name = "Time", Category = "Temporal", Description = "ISO time", StatusId = 3 },
                new ParameterDataType { Id = 13, Code = 320, Name = "DateTime", Category = "Temporal", Description = "ISO datetime", StatusId = 3 },
                new ParameterDataType { Id = 14, Code = 900, Name = "Bool", Category = "Other", Description = "Boolean true/false", StatusId = 3 },
                new ParameterDataType { Id = 15, Code = 910, Name = "Json", Category = "Other", Description = "Valid JSON", StatusId = 3 },
                new ParameterDataType { Id = 16, Code = 920, Name = "Secret", Category = "Other", Description = "Encrypted secret", StatusId = 3 }
            );

            // Parameter (TwoFactor Options)
            modelBuilder.Entity<Parameter>().HasData(
                new Parameter
                {
                    Id = 1,
                    Group = "TwoFactor",
                    Key = "Options",
                    ParameterDataTypeId = 15,
                    Value = "{\n  \"defaultChannel\": \"Sms\"\n}",
                    Template = "{\n  \"defaultChannel\": \"Sms\",\n  \"channels\": {\n    \"Sms\": { \"codeLength\": 6, \"expirySeconds\": 300 },\n    \"Email\": { \"codeLength\": 6, \"expirySeconds\": 300 },\n    \"Authenticator\": { \"digits\": 6, \"periodSeconds\": 30, \"hashAlgorithm\": \"SHA1\" }\n  }\n}",
                    Description = "İkili doğrulama varsayılan kanal ve seçenekleri",
                    StatusId = 3
                }
            );
        }

        /// <summary>Sadece seed edilmiş Statü Id’lerini bağlar.</summary>
        public async Task EnsureCoreSeedsAndBindAsync(CancellationToken ct = default)
        {
            DraftStatusId = await Set<Statu>().Where(x => x.Code == "DFT").Select(x => x.Id).SingleAsync(ct);
            AwaitingApprovalStatusId = await Set<Statu>().Where(x => x.Code == "AWT").Select(x => x.Id).SingleAsync(ct);
            ApprovedStatusId = await Set<Statu>().Where(x => x.Code == "APR").Select(x => x.Id).SingleAsync(ct);
            RejectedStatusId = await Set<Statu>().Where(x => x.Code == "REJ").Select(x => x.Id).SingleAsync(ct);
            PassiveStatusId = await Set<Statu>().Where(x => x.Code == "PSV").Select(x => x.Id).SingleAsync(ct);
            DeletedStatusId = await Set<Statu>().Where(x => x.Code == "DEL").Select(x => x.Id).SingleAsync(ct);
        }
    }
}

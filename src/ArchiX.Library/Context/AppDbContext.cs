// File: src / ArchiX.Library / Context / AppDbContext.cs
using ArchiX.Library.Entities;

using Microsoft.EntityFrameworkCore;
using ArchiX.Library.Runtime.Reports;
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
        public DbSet<Statu> Status => Set<Statu>();
        public DbSet<FilterItem> FilterItems => Set<FilterItem>();
        public DbSet<LanguagePack> LanguagePacks => Set<LanguagePack>();
        public DbSet<ConnectionServerWhitelist> ConnectionServerWhitelist => Set<ConnectionServerWhitelist>();
        public DbSet<ConnectionAudit> ConnectionAudits => Set<ConnectionAudit>();
        public DbSet<ParameterDataType> ParameterDataTypes => Set<ParameterDataType>();
        public DbSet<Parameter> Parameters => Set<Parameter>();
        public DbSet<Application> Applications => Set<Application>();
        public DbSet<User> Users => Set<User>();
        public DbSet<UserApplication> UserApplications => Set<UserApplication>();
        public DbSet<UserPasswordHistory> UserPasswordHistories => Set<UserPasswordHistory>();
        public DbSet<PasswordBlacklist> PasswordBlacklists => Set<PasswordBlacklist>();

        public DbSet<ReportDatasetTypeGroup> ReportDatasetTypeGroups => Set<ReportDatasetTypeGroup>();
        public DbSet<ReportDatasetType> ReportDatasetTypes => Set<ReportDatasetType>();
        public DbSet<ReportDataset> ReportDatasets => Set<ReportDataset>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseCollation("Latin1_General_100_CI_AS_SC_UTF8");
            base.OnModelCreating(modelBuilder);

            var asm = typeof(AppDbContext).Assembly;
            modelBuilder.ApplyPluralizeAndMapToDb(asm);
            modelBuilder.ApplyBaseEntityConventions();

            // Application entity
            modelBuilder.Entity<Application>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Code).IsRequired().HasMaxLength(100);
                e.Property(x => x.Name).IsRequired().HasMaxLength(200);
                e.Property(x => x.DefaultCulture).HasMaxLength(10);
                e.Property(x => x.TimeZoneId).HasMaxLength(100);
                e.Property(x => x.Description).HasMaxLength(500);
                e.HasIndex(x => x.Code).IsUnique();
            });

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

            modelBuilder.Entity<FilterItem>(e =>
            {
                e.HasKey(f => f.Id);
                e.Property(f => f.ItemType).IsRequired().HasMaxLength(50);
                e.Property(f => f.Code).IsRequired().HasMaxLength(50);
                e.HasIndex(f => new { f.ItemType, f.Code }).IsUnique();
            });

            modelBuilder.Entity<LanguagePack>(e =>
            {
                e.HasKey(lp => lp.Id);
                e.Property(lp => lp.ItemType).IsRequired();
                e.Property(lp => lp.Code).IsRequired();
                e.Property(lp => lp.Culture).IsRequired();
                e.HasIndex(lp => new { lp.ItemType, lp.EntityName, lp.FieldName, lp.Code, lp.Culture }).IsUnique();
            });

            modelBuilder.Entity<ConnectionServerWhitelist>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.ServerName).HasMaxLength(200);
                e.Property(x => x.Cidr).HasMaxLength(43);
                e.Property(x => x.EnvScope).HasMaxLength(20);
                e.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_Whitelist_ServerOrCidr", "[ServerName] IS NOT NULL OR [Cidr] IS NOT NULL");
                });
                e.HasIndex(x => new { x.ServerName, x.IsActive });
                e.HasIndex(x => new { x.Cidr, x.IsActive });
                e.HasIndex(x => x.EnvScope);
            });

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

            modelBuilder.Entity<Parameter>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Group).IsRequired().HasMaxLength(75);
                e.Property(x => x.Key).IsRequired().HasMaxLength(150);
                e.Property(x => x.Description).IsRequired().HasMaxLength(1000);
                e.HasIndex(x => new { x.Group, x.Key, x.ApplicationId }).IsUnique();
                e.HasOne(x => x.DataType).WithMany().HasForeignKey(x => x.ParameterDataTypeId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(x => x.Application).WithMany().HasForeignKey(x => x.ApplicationId).OnDelete(DeleteBehavior.Restrict);
            });

            // UserPasswordHistory
            modelBuilder.Entity<UserPasswordHistory>(entity =>
            {
                entity.ToTable("UserPasswordHistories");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(300);
                entity.Property(e => e.HashAlgorithm).IsRequired().HasMaxLength(20);
                entity.Property(e => e.CreatedAtUtc).IsRequired().HasPrecision(4);

                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => new { e.UserId, e.CreatedAtUtc });

                // BaseEntity configuration'ı otomatik uygulanır
            });

            // PasswordBlacklist configuration
            modelBuilder.Entity<PasswordBlacklist>(e =>
            {
                e.HasIndex(x => new { x.ApplicationId, x.Word }).IsUnique();
            });


            // PasswordBlacklist seeds(20 yaygın parola) - SABİT DEĞERLER
            var blacklistSeedData = new[]
            {
                "password", "123456", "12345678", "qwerty", "abc123",
                "123456789", "111111", "1234567", "letmein", "welcome",
                "monkey", "1234567890", "dragon", "master", "sunshine",
                "princess", "qazwsx", "654321", "michael", "football"
            };

            var baseTime = new DateTimeOffset(2025, 12, 11, 10, 0, 0, TimeSpan.Zero);
            var blacklistEntities = blacklistSeedData
                .Select((word, index) => new PasswordBlacklist
                {
                    Id = index + 1,
                    ApplicationId = 1,
                    Word = word,
                    RowId = new Guid($"00000000-0000-0000-0000-{index:D12}"),
                    CreatedAt = baseTime.AddSeconds(index),
                    CreatedBy = 0,
                    StatusId = BaseEntity.ApprovedStatusId,
                    LastStatusBy = 0,
                    IsProtected = false
                })
                .ToArray();

            // ✅ ReportDataset master seeds (TypeGroups + Types) - migration/ensureCreated ile otomatik gelir.
            // deterministic Id kullanıyoruz (FK ilişkileri için şart)
            var rdGroupSeeds = ReportDatasetSeeds.TypeGroups
                .Select((g, i) => new ReportDatasetTypeGroup
                {
                    Id = i + 1,
                    Code = g.Code,
                    Name = g.Name,
                    Description = g.Description,
                    StatusId = BaseEntity.ApprovedStatusId,
                    CreatedBy = 0,
                    LastStatusBy = 0,
                    IsProtected = true,
                })
                .ToArray();

            modelBuilder.Entity<ReportDatasetTypeGroup>().HasData(rdGroupSeeds);

            var groupIdByCode = rdGroupSeeds.ToDictionary(x => x.Code, x => x.Id);

            var rdTypeSeeds = ReportDatasetSeeds.Types
                .Select((t, i) => new ReportDatasetType
                {
                    Id = i + 1,
                    ReportDatasetTypeGroupId = groupIdByCode[t.GroupCode],
                    Code = t.Code,
                    Name = t.Name,
                    Description = t.Description,
                    StatusId = BaseEntity.ApprovedStatusId,
                    CreatedBy = 0,
                    LastStatusBy = 0,
                    IsProtected = true,
                })
                .ToArray();

            modelBuilder.Entity<ReportDatasetType>().HasData(rdTypeSeeds);

            modelBuilder.Entity<PasswordBlacklist>().HasData(blacklistEntities);

            // User seed
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    UserName = "admin",
                    NormalizedUserName = "ADMIN",
                    DisplayName = "System Admin",
                    Email = "admin@example.com",
                    NormalizedEmail = "ADMIN@EXAMPLE.COM",
                    IsAdmin = true,
                    IsProtected = true,
                    StatusId = BaseEntity.ApprovedStatusId
                }
            );

            // UserApplication seed
            modelBuilder.Entity<UserApplication>().HasData(
                new UserApplication
                {
                    Id = 1,
                    UserId = 1,
                    ApplicationId = 1,
                    IsProtected = true,
                    StatusId = BaseEntity.ApprovedStatusId
                }
            );

            // Status seeds
            modelBuilder.Entity<Statu>().HasData(
                new Statu { Id = 1, Code = "DFT", Name = "Draft", Description = "Record is in draft state", StatusId = BaseEntity.ApprovedStatusId },
                new Statu { Id = 2, Code = "AWT", Name = "Awaiting Approval", Description = "Record is waiting for approval", StatusId = BaseEntity.ApprovedStatusId },
                new Statu { Id = 3, Code = "APR", Name = "Approved", Description = "Record has been approved", StatusId = BaseEntity.ApprovedStatusId },
                new Statu { Id = 4, Code = "REJ", Name = "Rejected", Description = "Record has been rejected", StatusId = BaseEntity.ApprovedStatusId },
                new Statu { Id = 5, Code = "PSV", Name = "Passive", Description = "Record is passive / inactive", StatusId = BaseEntity.ApprovedStatusId },
                new Statu { Id = 6, Code = "DEL", Name = "Deleted", Description = "Record has been deleted", StatusId = BaseEntity.ApprovedStatusId }
            );

            // Application seed
            modelBuilder.Entity<Application>().HasData(
                new Application { Id = 1, Code = "Global", Name = "Global Application", Description = "Default/global scope", StatusId = BaseEntity.ApprovedStatusId, ConfigVersion = 1 }
            );

            // FilterItem seeds
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

            // LanguagePack seeds
            modelBuilder.Entity<LanguagePack>().HasData(
                new LanguagePack { Id = 1, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "Equals", Culture = "tr-TR", DisplayName = "Eşittir", Description = "Değer belirtilene eşit olmalı", StatusId = 3 },
                new LanguagePack { Id = 2, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "Equals", Culture = "en-US", DisplayName = "Equals", Description = "Value must be equal to the given one", StatusId = 3 },
                new LanguagePack { Id = 3, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "NotEquals", Culture = "tr-TR", DisplayName = "Eşit Değil", Description = "Değer belirtilene eşit olmamalı", StatusId = 3 },
                new LanguagePack { Id = 4, ItemType = "Operator", EntityName = "FilterItem", FieldName = "Code", Code = "NotEquals", Culture = "en-US", DisplayName = "Not Equal", Description = "Value must not be equal to the given one", StatusId = 3 }
            );

            // ParameterDataType seeds
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

            // Parameter seeds
            modelBuilder.Entity<Parameter>().HasData(
                new Parameter
                {
                    Id = 1,
                    Group = "TwoFactor",
                    Key = "Options",
                    ApplicationId = 1,
                    ParameterDataTypeId = 15,
                    Value = "{\n  \"defaultChannel\": \"Email\",\n  \"channels\": {\n    \"Sms\": { \"codeLength\": 6, \"expirySeconds\": 300 },\n    \"Email\": { \"codeLength\": 6, \"expirySeconds\": 300 },\n    \"Authenticator\": { \"digits\": 6, \"periodSeconds\": 30, \"hashAlgorithm\": \"SHA1\" }\n  }\n}",
                    Template = "{\n  \"defaultChannel\": \"Sms\",\n  \"channels\": {\n    \"Sms\": { \"codeLength\": 6, \"expirySeconds\": 300 },\n    \"Email\": { \"codeLength\": 6, \"expirySeconds\": 300 },\n    \"Authenticator\": { \"digits\": 6, \"periodSeconds\": 30, \"hashAlgorithm\": \"SHA1\" }\n  }\n}",
                    Description = "İkili doğrulama varsayılan kanal ve seçenekleri",
                    StatusId = 3
                },
                new Parameter
                {
                    Id = 2,
                    Group = "Security",
                    Key = "PasswordPolicy",
                    ApplicationId = 1,
                    ParameterDataTypeId = 15,
                    Description = "Parola politikası ve hash parametreleri",
                    StatusId = 3,
                    Value = "{\n  \"version\": 1,\n  \"minLength\": 12,\n  \"maxLength\": 128,\n  \"requireUpper\": true,\n  \"requireLower\": true,\n  \"requireDigit\": true,\n  \"requireSymbol\": true,\n  \"allowedSymbols\": \"!@#$%^&*_-+=:?.,;\",\n  \"minDistinctChars\": 5,\n  \"maxRepeatedSequence\": 3,\n  \"blockList\": [\"password\", \"123456\", \"qwerty\", \"admin\"],\n  \"historyCount\": 10,\n  \"maxPasswordAgeDays\": null,\n  \"lockoutThreshold\": 5,\n  \"lockoutSeconds\": 900,\n  \"hash\": {\n    \"algorithm\": \"Argon2id\",\n    \"memoryKb\": 65536,\n    \"parallelism\": 2,\n    \"iterations\": 3,\n    \"saltLength\": 16,\n    \"hashLength\": 32,\n    \"fallback\": { \"algorithm\": \"PBKDF2-SHA512\", \"iterations\": 210000 },\n    \"pepperEnabled\": true\n  }\n}",
                    Template = "{\n  \"version\": 1,\n  \"minLength\": 12,\n  \"maxLength\": 128,\n  \"requireUpper\": true,\n  \"requireLower\": true,\n  \"requireDigit\": true,\n  \"requireSymbol\": true,\n  \"allowedSymbols\": \"\",\n  \"minDistinctChars\": 0,\n  \"maxRepeatedSequence\": 0,\n  \"blockList\": [],\n  \"historyCount\": 0,\n  \"lockoutThreshold\": 0,\n  \"lockoutSeconds\": 0,\n  \"hash\": {\n    \"algorithm\": \"Argon2id\",\n    \"memoryKb\": 0,\n    \"parallelism\": 0,\n    \"iterations\": 0,\n    \"saltLength\": 0,\n    \"hashLength\": 0,\n    \"fallback\": { \"algorithm\": \"PBKDF2-SHA512\", \"iterations\": 0 },\n    \"pepperEnabled\": false\n  }\n}"
                },
                new Parameter
                {
                    Id = 3,
                    Group = "UI",
                    Key = "TabbedOptions",
                    ApplicationId = 1,
                    ParameterDataTypeId = 15,
                    Description = "#42 TabbedOptions JSON. navigationMode=Mod(Tabbed/FullPage); tabbed.maxOpenTabs=Maks tab; tabbed.tabAutoCloseMinutes=Oto kapanış(dk); tabbed.autoCloseWarningSeconds=Uyarı(sn); tabbed.enableNestedTabs=Nested tab; tabbed.requireTabContext=Direct link engeli.",
                    StatusId = 3,
                    Value = "{\n  \"version\": 1,\n  \"navigationMode\": \"Tabbed\",\n  \"tabbed\": {\n    \"maxOpenTabs\": 15,\n    \"onMaxTabReached\": {\n      \"behavior\": \"Block\",\n      \"message\": \"Açık tab sayısı 15 limitine geldi. Lütfen açık tablardan birini kapatınız.\"\n    },\n    \"enableNestedTabs\": true,\n    \"requireTabContext\": true,\n    \"tabAutoCloseMinutes\": 10,\n    \"autoCloseWarningSeconds\": 30,\n    \"tabTitleUniqueSuffix\": { \"format\": \"_{000}\", \"start\": 1 }\n  },\n  \"fullPage\": {\n    \"defaultLandingRoute\": \"/Dashboard\",\n    \"openReportsInNewWindow\": false,\n    \"confirmOnUnsavedChanges\": true,\n    \"deepLinkEnabled\": true,\n    \"errorMode\": \"DefaultErrorPage\",\n    \"enableKeepAlive\": true,\n    \"sessionTimeoutWarningSeconds\": 60\n  }\n}",
                    Template = "{\n  \"version\": 1,\n  \"navigationMode\": \"Tabbed\",\n  \"tabbed\": {\n    \"maxOpenTabs\": 15,\n    \"onMaxTabReached\": {\n      \"behavior\": \"Block\",\n      \"message\": \"Açık tab sayısı 15 limitine geldi. Lütfen açık tablardan birini kapatınız.\"\n    },\n    \"enableNestedTabs\": false,\n    \"requireTabContext\": true,\n    \"tabAutoCloseMinutes\": 10,\n    \"autoCloseWarningSeconds\": 30,\n    \"tabTitleUniqueSuffix\": { \"format\": \"_{000}\", \"start\": 1 }\n  },\n  \"fullPage\": {\n    \"defaultLandingRoute\": \"/Dashboard\",\n    \"openReportsInNewWindow\": false,\n    \"confirmOnUnsavedChanges\": true,\n    \"deepLinkEnabled\": true,\n    \"errorMode\": \"DefaultErrorPage\",\n    \"enableKeepAlive\": true,\n    \"sessionTimeoutWarningSeconds\": 60\n  }\n}"
                }
            );

            // ✅ GLOBAL FILTERS
            modelBuilder.ApplySoftDeleteFilters();

            // ✅ GLOBAL FK POLICY (EN SON!)
            modelBuilder.ApplyRestrictDeleteBehavior();
        }

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

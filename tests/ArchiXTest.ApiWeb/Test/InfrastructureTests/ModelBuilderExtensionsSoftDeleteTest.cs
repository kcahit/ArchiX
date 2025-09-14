// NuGet: Microsoft.EntityFrameworkCore.InMemory
// Proje referansı: ArchiX.Library (BaseEntity + ModelBuilderExtensionsSoftDelete)

using ArchiX.Library.Entities;                 // BaseEntity
using ArchiX.Library.Infrastructure.EFCore;    // ApplySoftDeleteFilter

using Microsoft.EntityFrameworkCore;

using Xunit;

namespace ArchiXTest.ApiWeb.Test.InfrastructureTests
{
    // === Test için minimal entity (BaseEntity + Name alanı) ===
    internal class SoftDelEntity : BaseEntity
    {
        public new static bool MapToDb = false;   // <-- tabloya çevrilmeyecek
        public string Name { get; set; } = "";
    }

    // === Varsayılan (-14 / DEL) ile global filtre uygulayan DbContext ===
    internal class SoftDelDbDefault : DbContext
    {
        public SoftDelDbDefault(DbContextOptions<SoftDelDbDefault> options) : base(options) { }
        public DbSet<SoftDelEntity> Items => Set<SoftDelEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplySoftDeleteFilter(); // default: -14 (DEL)
            base.OnModelCreating(modelBuilder);
        }
    }

    // === Özel deletedStatusId ile filtre uygulayan DbContext ===
    internal class SoftDelDbCustom : DbContext
    {
        private readonly int _deleted;
        public SoftDelDbCustom(DbContextOptions<SoftDelDbCustom> options, int deleted) : base(options)
        {
            _deleted = deleted;
        }

        public DbSet<SoftDelEntity> Items => Set<SoftDelEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplySoftDeleteFilter(_deleted);
            base.OnModelCreating(modelBuilder);
        }
    }

    public class ModelBuilderExtensionsSoftDeleteTest
    {
        private const int DEL = -14; // Statu seed: Code="DEL"

        private static DbContextOptions<T> InMemory<T>() where T : DbContext =>
            new DbContextOptionsBuilder<T>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .EnableSensitiveDataLogging()
                .Options;

        [Fact]
        public async Task DefaultFilter_Excludes_Status_DEL()
        {
            var options = InMemory<SoftDelDbDefault>();
            await using var db = new SoftDelDbDefault(options);

            db.Items.AddRange(
                new SoftDelEntity { Name = "Alive", StatusId = 0 },
                new SoftDelEntity { Name = "Deleted", StatusId = DEL }
            );
            await db.SaveChangesAsync();

            var visible = await db.Items.OrderBy(x => x.Name).ToListAsync();
            var all = await db.Items.IgnoreQueryFilters().OrderBy(x => x.Name).ToListAsync();

            Assert.Single(visible);
            Assert.Equal("Alive", visible[0].Name);
            Assert.Equal(2, all.Count);
            Assert.Contains(all, x => x.Name == "Deleted");
        }

        [Fact]
        public async Task CustomDeletedStatus_IsHonored_ByFilter()
        {
            const int customDel = -999;
            var options = InMemory<SoftDelDbCustom>();
            await using var db = new SoftDelDbCustom(options, customDel);

            db.Items.AddRange(
                new SoftDelEntity { Name = "Keep", StatusId = 1 },
                new SoftDelEntity { Name = "Trash", StatusId = customDel }
            );
            await db.SaveChangesAsync();

            var visible = await db.Items.ToListAsync();
            var all = await db.Items.IgnoreQueryFilters().ToListAsync();

            Assert.Single(visible);
            Assert.Equal("Keep", visible[0].Name);
            Assert.Equal(2, all.Count);
        }
    }
}

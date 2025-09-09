using ArchiX.Library.Context;
using ArchiX.Library.Entities;
using ArchiX.Library.Filtering;
using ArchiX.Library.LanguagePacks;

using Microsoft.EntityFrameworkCore;

using Xunit;

namespace ArchiXTest.ApiWeb.Tests.InfrastructureTests
{
    public sealed class ContextTests
    {
        private AppDbContext CreateInMemoryDb()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // her test için ayrı DB
                .Options;

            var db = new AppDbContext(options);
            db.Database.EnsureCreated(); // seed verilerini yükler
            return db;
        }

        [Fact]
        public void CanInsertAndRetrieveEntity()
        {
            using var db = CreateInMemoryDb();

            var entity = new Statu { Id = 1, Code = "TST", Name = "Test", Description = "Test record" };
            db.Add(entity);
            db.SaveChanges();

            var result = db.Set<Statu>().FirstOrDefault(e => e.Id == 1);

            Assert.NotNull(result);
            Assert.Equal("TST", result!.Code);
        }

        [Fact]
        public void ShouldSeedFilterItems()
        {
            using var db = CreateInMemoryDb();

            var items = db.Set<FilterItem>().ToList();

            Assert.NotEmpty(items);
            Assert.Contains(items, f => f.Code == "Equals");
            Assert.Contains(items, f => f.Code == "NotEquals");
        }

        [Fact]
        public void ShouldSeedLanguagePacks()
        {
            using var db = CreateInMemoryDb();

            var langs = db.Set<LanguagePack>().ToList();

            Assert.NotEmpty(langs);
            Assert.Contains(langs, lp => lp.Code == "Equals" && lp.Culture == "tr-TR");
            Assert.Contains(langs, lp => lp.Code == "Equals" && lp.Culture == "en-US");
        }
    }
}

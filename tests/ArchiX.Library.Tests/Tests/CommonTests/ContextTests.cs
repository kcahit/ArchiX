using ArchiX.Library.Context;
using ArchiX.Library.Entities;

using Microsoft.EntityFrameworkCore;

using Xunit;

namespace ArchiX.Library.Tests.Tests.CommonTests
{
    /// <summary>
    /// InMemory DB ile çalışan, test içinden minimal seed yapan sürüm.
    /// SQLite/SQL Server’a ihtiyaç yoktur.
    /// </summary>
    public sealed class ContextTests
    {
        private static AppDbContext CreateInMemoryDb()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // her test için izole db
                .Options;

            var db = new AppDbContext(options);
            db.Database.EnsureCreated();  // şema/model ayakta

            SeedForTests(db);             // testlerin ihtiyaç duyduğu minimum veri
            return db;
        }

        /// <summary>
        /// Testlerin beklediği FilterItem ve LanguagePack kayıtlarını
        /// (tablolarda yoksa) minimal şekilde ekler.
        /// InMemory provider kullanıldığı için FK/MARS vb. dert yok.
        /// </summary>
        private static void SeedForTests(AppDbContext db)
        {
            // ---- FilterItems (Equals, NotEquals) ----
            if (!db.Set<FilterItem>().Any())
            {
                // “zorunlu” alanlara güvenli default’lar
                var createdBy = 0;
                var lastStatusBy = 0;
                var statusId = 3; // Approved gibi; test açısından numerik değer yeterli

                var items = new[]
                {
                    new FilterItem
                    {
                        ItemType = "Operator",
                        Code = "Equals",
                        CreatedBy = createdBy,
                        LastStatusBy = lastStatusBy,
                        StatusId = statusId
                    },
                    new FilterItem
                    {
                        ItemType = "Operator",
                        Code = "NotEquals",
                        CreatedBy = createdBy,
                        LastStatusBy = lastStatusBy,
                        StatusId = statusId
                    }
                };

                db.Set<FilterItem>().AddRange(items);
                db.SaveChanges();
            }

            // ---- LanguagePacks (Equals için tr-TR ve en-US) ----
            if (!db.Set<LanguagePack>().Any(lp => lp.Code == "Equals"))
            {
                var createdBy = 0;
                var lastStatusBy = 0;
                var statusId = 3;

                var langs = new[]
                {
                    new LanguagePack
                    {
                        ItemType = "Operator",
                        EntityName = "FilterItem",
                        FieldName = "Code",
                        Code = "Equals",
                        Culture = "tr-TR",
                        DisplayName = "Eşittir",
                        Description = "Değer belirtilene eşit olmalı",
                        CreatedBy = createdBy,
                        LastStatusBy = lastStatusBy,
                        StatusId = statusId
                    },
                    new LanguagePack
                    {
                        ItemType = "Operator",
                        EntityName = "FilterItem",
                        FieldName = "Code",
                        Code = "Equals",
                        Culture = "en-US",
                        DisplayName = "Equals",
                        Description = "Value must be equal to the given one",
                        CreatedBy = createdBy,
                        LastStatusBy = lastStatusBy,
                        StatusId = statusId
                    }
                };

                db.Set<LanguagePack>().AddRange(langs);
                db.SaveChanges();
            }
        }

        [Fact]
        public void CanInsertAndRetrieveEntity()
        {
            using var db = CreateInMemoryDb();

            var entity = new Statu
            {
                Id = 1,
                Code = "TST",
                Name = "Test",
                Description = "Test record",
                CreatedBy = 0,
                LastStatusBy = 0,
                StatusId = 3
            };

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

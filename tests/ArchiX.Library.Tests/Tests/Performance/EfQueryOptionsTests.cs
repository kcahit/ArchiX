using ArchiX.Library.Context;
using ArchiX.Library.Entities;
using ArchiX.Library.Infrastructure.EfCore;

using Microsoft.EntityFrameworkCore;

using Xunit;

namespace ArchiX.Library.Tests.Tests.Performance
{
    public class EfQueryOptionsTests
    {
        private static DbContextOptions<T> InMemoryOptions<T>() where T : DbContext
            => new DbContextOptionsBuilder<T>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .EnableSensitiveDataLogging()
                .Options;

        [Fact]
        public async Task GetAllAsync_Should_Be_NoTracking()
        {
            var options = InMemoryOptions<AppDbContext>();
            await using var db = new AppDbContext(options);
            db.Database.EnsureCreated();

            // Seed (Statu 1-6) temizle
            db.Set<Statu>().RemoveRange(db.Set<Statu>());
            await db.SaveChangesAsync();

            // Sadece 2 kayýt ekle
            db.Set<Statu>().Add(new Statu { Code = "T1", Name = "One" });
            db.Set<Statu>().Add(new Statu { Code = "T2", Name = "Two" });
            await db.SaveChangesAsync();

            // clear tracker so we can assert that query itself does not start tracking
            db.ChangeTracker.Clear();

            var repo = new Repository<Statu>(db);

            // act
            var all = (await repo.GetAllAsync()).ToList();

            // assert: change tracker should not contain tracked entries from the query
            Assert.Empty(db.ChangeTracker.Entries());
            Assert.Equal(2, all.Count);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Respect_SoftDelete()
        {
            var options = InMemoryOptions<AppDbContext>();
            await using var db = new AppDbContext(options);
            db.Database.EnsureCreated();

            var alive = new Statu { Code = "A", Name = "Alive", StatusId = BaseEntity.ApprovedStatusId };
            var deleted = new Statu { Code = "D", Name = "Deleted", StatusId = BaseEntity.DeletedStatusId };

            db.Set<Statu>().Add(alive);
            db.Set<Statu>().Add(deleted);
            await db.SaveChangesAsync();

            var repo = new Repository<Statu>(db);

            var aliveFromRepo = await repo.GetByIdAsync(alive.Id);
            var deletedFromRepo = await repo.GetByIdAsync(deleted.Id);

            Assert.NotNull(aliveFromRepo);
            Assert.Equal(alive.Code, aliveFromRepo!.Code);

            Assert.Null(deletedFromRepo);
        }
    }
}

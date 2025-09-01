using System;
using System.Threading.Tasks;
using ArchiX.Library.Context;
using ArchiX.Library.Entities;
using ArchiX.Library.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ArchiXTest.ApiWeb.Tests.InfrastructureTests
{
    public sealed class RepositoryUowTests
    {
        private (Repository<Statu> repo, UnitOfWork uow) CreateInMemory()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var db = new AppDbContext(options);
            db.Database.EnsureCreated();

            var repo = new Repository<Statu>(db);
            var uow = new UnitOfWork(db);
            return (repo, uow);
        }

        [Fact]
        public async Task Add_Then_GetById_ShouldReturnEntity()
        {
            // Arrange
            var (repo, uow) = CreateInMemory();
            var entity = new Statu { Id = 100, Code = "TST", Name = "Test", Description = "Test record" };

            // Act
            await repo.AddAsync(entity, userId: 1);
            await uow.SaveChangesAsync();

            var result = await repo.GetByIdAsync(100);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("TST", result!.Code);
        }

        [Fact]
        public async Task Update_ShouldChangeEntityValues()
        {
            // Arrange
            var (repo, uow) = CreateInMemory();
            var entity = new Statu { Id = 200, Code = "UPD", Name = "Old", Description = "Before update" };
            await repo.AddAsync(entity, 1);
            await uow.SaveChangesAsync();

            // Act
            entity.Name = "New";
            await repo.UpdateAsync(entity, 2);
            await uow.SaveChangesAsync();

            var result = await repo.GetByIdAsync(200);

            // Assert
            Assert.Equal("New", result!.Name);
            Assert.Equal(2, result.UpdatedBy);
        }

        [Fact]
        public async Task Delete_ShouldRemoveEntity()
        {
            // Arrange
            var (repo, uow) = CreateInMemory();
            var entity = new Statu { Id = 300, Code = "DEL", Name = "ToDelete", Description = "Delete test" };
            await repo.AddAsync(entity, 1);
            await uow.SaveChangesAsync();

            // Act
            await repo.DeleteAsync(300, 5);
            await uow.SaveChangesAsync();

            var result = await repo.GetByIdAsync(300);

            // Assert
            Assert.Null(result);
        }
    }
}

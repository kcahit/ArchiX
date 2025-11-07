using System;
using System.Threading.Tasks;
using ArchiX.Library.Context;
using ArchiX.Library.Entities;
using ArchiX.Library.Infrastructure.Caching;
using ArchiX.Library.Infrastructure.EfCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace ArchiX.Library.Tests.Tests.Caching
{
 public class RepositoryCacheDecoratorTests
 {
 private static DbContextOptions<T> InMemoryOptions<T>() where T : DbContext
 => new DbContextOptionsBuilder<T>()
 .UseInMemoryDatabase(Guid.NewGuid().ToString())
 .Options;

 [Fact]
 public async Task Cached_GetById_Returns_Cached_Value_Until_Invalidated()
 {
 var options = InMemoryOptions<AppDbContext>();
 await using var db = new AppDbContext(options);
 db.Database.EnsureCreated();

 var entity = new Statu { Code = "C1", Name = "Original" };
 db.Set<Statu>().Add(entity);
 await db.SaveChangesAsync();

 var repo = new Repository<Statu>(db);
 var mem = new MemoryCache(new MemoryCacheOptions());
 var memSvc = new MemoryCacheService(mem);
 var cached = new RepositoryCacheDecorator<Statu>(repo, memSvc, TimeSpan.FromMinutes(5));

 // warm cache
 var first = await cached.GetByIdAsync(entity.Id);
 Assert.Equal("Original", first!.Name);

 // change DB directly
 var dbEntity = await db.Set<Statu>().FindAsync(entity.Id);
 dbEntity!.Name = "ChangedInDb";
 await db.SaveChangesAsync();

 // because of cache, decorator should still return original value
 var cachedAgain = await cached.GetByIdAsync(entity.Id);
 Assert.Equal("Original", cachedAgain!.Name);

 // now invalidate via decorator.UpdateAsync (which removes cache) then SaveChanges
 dbEntity.Name = "UpdatedViaDecorator";
 await cached.UpdateAsync(dbEntity, userId:1);
 await db.SaveChangesAsync();

 var afterUpdate = await cached.GetByIdAsync(entity.Id);
 Assert.Equal("UpdatedViaDecorator", afterUpdate!.Name);
 }

 [Fact]
 public async Task Delete_Invalidates_Cache()
 {
 var options = InMemoryOptions<AppDbContext>();
 await using var db = new AppDbContext(options);
 db.Database.EnsureCreated();

 var entity = new Statu { Code = "C2", Name = "ToDelete" };
 db.Set<Statu>().Add(entity);
 await db.SaveChangesAsync();

 var repo = new Repository<Statu>(db);
 var mem = new MemoryCache(new MemoryCacheOptions());
 var memSvc = new MemoryCacheService(mem);
 var cached = new RepositoryCacheDecorator<Statu>(repo, memSvc, TimeSpan.FromMinutes(5));

 // warm cache
 var first = await cached.GetByIdAsync(entity.Id);
 Assert.NotNull(first);

 // delete via decorator (which should remove cache)
 await cached.DeleteAsync(entity.Id, userId:2);
 await db.SaveChangesAsync();

 var afterDelete = await cached.GetByIdAsync(entity.Id);
 Assert.Null(afterDelete);
 }
 }
}

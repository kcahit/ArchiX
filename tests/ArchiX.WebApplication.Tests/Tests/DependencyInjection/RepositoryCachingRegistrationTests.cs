using System;
using ArchiX.Library.Abstractions.Persistence;
using ArchiX.Library.Entities;
using ArchiX.Library.Infrastructure.Caching;
using ArchiX.WebApplication;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Microsoft.EntityFrameworkCore;
using ArchiX.Library.Context;

namespace ArchiX.WebApplication.Tests.Tests.DependencyInjection
{
 public class RepositoryCachingRegistrationTests
 {
 [Fact]
 public void AddArchiXWebAppDefaults_Registers_Cache_And_RepositoryDecorator()
 {
 var services = new ServiceCollection();

 // register a simple in-memory AppDbContext so Repository<> can be constructed
 services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("di-test-db"));

 // Act
 services.AddArchiXWebAppDefaults();
 var sp = services.BuildServiceProvider();

 // Assert
 var cache = sp.GetService<ICacheService>();
 Assert.NotNull(cache);

 var repo = sp.GetService<IRepository<Statu>>();
 Assert.NotNull(repo);
 Assert.IsType<RepositoryCacheDecorator<Statu>>(repo);
 }
 }
}

using ArchiX.Library.Context;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Xunit;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ArchiX.Library.Tests.Tests.PersistenceTests;

public sealed class TabbedOptionsSeedTests
{
    [Fact]
    public void TabbedOptions_parameter_seed_is_present_in_model()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var db = new AppDbContext(options);

        var sp = ((IInfrastructure<IServiceProvider>)db).Instance;
        var designModel = sp.GetRequiredService<IDesignTimeModel>().Model;
        var entityType = designModel.FindEntityType(typeof(ArchiX.Library.Entities.Parameter));
        entityType.Should().NotBeNull();

        // Seed data is stored in model metadata; verify at least one seed row has UI/TabbedOptions.
        var seeds = entityType!.GetSeedData();
        seeds.Should().NotBeNull();

        var found = false;
        foreach (var s in seeds)
        {
            if (s.TryGetValue("Group", out var gObj)
                && s.TryGetValue("Key", out var kObj)
                && string.Equals(gObj as string, "UI", StringComparison.Ordinal)
                && string.Equals(kObj as string, "TabbedOptions", StringComparison.Ordinal))
            {
                found = true;
                break;
            }
        }

        found.Should().BeTrue();
    }
}

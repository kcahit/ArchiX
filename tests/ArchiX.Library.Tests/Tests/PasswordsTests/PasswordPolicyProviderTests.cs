using ArchiX.Library.Abstractions.Security;
using ArchiX.Library.Context;              // AppDbContext için

using Microsoft.EntityFrameworkCore;        // UseInMemoryDatabase için

using Xunit;

namespace ArchiX.Library.Tests.Tests.PasswordsTests;

public sealed class PasswordPolicyProviderTests
{
    [Fact]
    public async Task Provider_Loads_Policy_From_Parameters()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddMemoryCache();
        services.AddDbContextFactory<AppDbContext>(o => o.UseInMemoryDatabase("PolicyTests"));

        // Statik uzantý metodu doðrudan sýnýf adýyla çaðrýlýyor
        ArchiX.Library.Runtime.Security.PasswordSecurityServiceCollectionExtensions.AddPasswordSecurity(services);

        var sp = services.BuildServiceProvider();
        var provider = sp.GetRequiredService<IPasswordPolicyProvider>();

        // Act
        var options = await provider.GetAsync(1);

        // Assert
        Assert.True(options.MinLength >= 10);
        Assert.True(options.RequireUpper);
        Assert.True(options.RequireLower);
        Assert.True(options.RequireDigit);
        Assert.True(options.RequireSymbol);

        Assert.True(options.Hash.MemoryKb >= 32768);
        Assert.InRange(options.Hash.Iterations, 2, 5);
        Assert.InRange(options.Hash.Parallelism, 1, 4);
        Assert.InRange(options.Hash.SaltLength, 12, 32);
        Assert.InRange(options.Hash.HashLength, 24, 64);
        Assert.Equal("Argon2id", options.Hash.Algorithm);
    }
}

using ArchiX.Library.Abstractions.Security;
using ArchiX.Library.Context;

using Microsoft.EntityFrameworkCore;
using ArchiX.Library.Entities; // Parameter için

using Xunit;

namespace ArchiX.Library.Tests.Tests.PasswordsTests;

public sealed class PasswordPolicyCachingTests
{
    private static async Task SeedPolicyAsync(ServiceProvider sp, int id)
    {
        var factory = sp.GetRequiredService<IDbContextFactory<AppDbContext>>();
        await using var db = await factory.CreateDbContextAsync(CancellationToken.None);
        if (!await db.Parameters.AnyAsync(p => p.ApplicationId == 1 && p.Group == "Security" && p.Key == "PasswordPolicy"))
        {
            var policyJson = """
            {
              "version": 1,
              "minLength": 12,
              "maxLength": 128,
              "requireUpper": true,
              "requireLower": true,
              "requireDigit": true,
              "requireSymbol": true,
              "allowedSymbols": "!@#$%^&*_-+=:?.,;",
              "minDistinctChars": 5,
              "maxRepeatedSequence": 3,
              "blockList": ["password","123456","qwerty","admin"],
              "historyCount": 10,
              "lockoutThreshold": 5,
              "lockoutSeconds": 900,
              "hash": {
                "algorithm": "Argon2id",
                "memoryKb": 32768,
                "parallelism": 2,
                "iterations": 3,
                "saltLength": 16,
                "hashLength": 32,
                "fallback": { "algorithm": "PBKDF2-SHA512", "iterations": 210000 },
                "pepperEnabled": true
              }
            }
            """;
            db.Parameters.Add(new Parameter
            {
                Id = id,
                ApplicationId = 1,
                Group = "Security",
                Key = "PasswordPolicy",
                ParameterDataTypeId = 15,
                StatusId = 3,
                CreatedBy = 0,
                Description = "Test seed",
                Value = policyJson
            });
            await db.SaveChangesAsync();
        }
    }

    [Fact]
    public async Task Invalidate_Forces_Reload_On_Next_GetAsync()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMemoryCache();
        services.AddDbContextFactory<AppDbContext>(o => o.UseInMemoryDatabase("CacheTests"));
        ArchiX.Library.Runtime.Security.PasswordSecurityServiceCollectionExtensions.AddPasswordSecurity(services);

        var sp = services.BuildServiceProvider();
        await SeedPolicyAsync(sp, 3001);

        var provider = sp.GetRequiredService<IPasswordPolicyProvider>();

        // Ýlk okuma (cache set edilir)
        var p1 = await provider.GetAsync(1, CancellationToken.None);

        // Cache temizle
        provider.Invalidate(1);

        // Sonraki okuma (DB’den tekrar yüklenmeli)
        var p2 = await provider.GetAsync(1, CancellationToken.None);

        // Deðerler yeniden yüklenmiþ olmalý
        Assert.Equal(p1.MinLength, p2.MinLength);
        Assert.Equal(p1.Hash.MemoryKb, p2.Hash.MemoryKb);
        Assert.Equal(p1.Hash.Iterations, p2.Hash.Iterations);
    }
}

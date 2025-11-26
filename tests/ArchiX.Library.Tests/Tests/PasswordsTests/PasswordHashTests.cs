using ArchiX.Library.Abstractions.Security;
using ArchiX.Library.Context;              // AppDbContext
using ArchiX.Library.Entities;

using Microsoft.EntityFrameworkCore;        // UseInMemoryDatabase

using Xunit;

namespace ArchiX.Library.Tests.Tests.PasswordsTests;

public sealed class PasswordHashTests
{
    private static async Task SeedPolicyAsync(ServiceProvider sp, int id)
    {
        var factory = sp.GetRequiredService<IDbContextFactory<AppDbContext>>();
        await using var db = await factory.CreateDbContextAsync(CancellationToken.None);
        if (!await db.Parameters.AnyAsync(p => p.ApplicationId == 1 && p.Group == "Security" && p.Key == "PasswordPolicy"))
        {
            var json = """
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
                "pepperEnabled": false
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
                Value = json
            });
            await db.SaveChangesAsync();
        }
    }

    [Fact]
    public async Task Argon2_RoundTrip_With_Explicit_Invalidate_Succeeds()
    {
        var services = new ServiceCollection();
        services.AddMemoryCache();
        services.AddDbContextFactory<AppDbContext>(o => o.UseInMemoryDatabase("HashRoundTrip"));
        ArchiX.Library.Runtime.Security.PasswordSecurityServiceCollectionExtensions.AddPasswordSecurity(services);

        var sp = services.BuildServiceProvider();

        // 1. Seed
        await SeedPolicyAsync(sp, 9001);

        var provider = sp.GetRequiredService<IPasswordPolicyProvider>();

        // 2. Invalidate (idempotent; seed sonrası cache'i temizler)
        provider.Invalidate(1);

        // 3. Oku (taze policy)
        var policy = await provider.GetAsync(1, CancellationToken.None);

        var hasher = sp.GetRequiredService<IPasswordHasher>();
        var password = "GoodP@ssw0rd!";

        var hash = await hasher.HashAsync(password, policy, CancellationToken.None);
        Assert.True(await hasher.VerifyAsync(password, hash, policy, CancellationToken.None));
    }
}

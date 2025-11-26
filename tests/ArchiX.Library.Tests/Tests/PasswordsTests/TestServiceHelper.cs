using ArchiX.Library.Context;
using ArchiX.Library.Entities;
using ArchiX.Library.Runtime.Security;

using Microsoft.EntityFrameworkCore;

namespace ArchiX.Library.Tests.Tests.PasswordsTests;

public static class TestServiceHelper
{
    public static ServiceProvider BuildServices(string dbName = "PolicyTests")
    {
        var services = new ServiceCollection();

        // MemoryCache
        services.AddMemoryCache();

        // InMemory EF Core factory
        services.AddDbContextFactory<AppDbContext>(o =>
        {
            o.UseInMemoryDatabase(dbName);
        });

        // PasswordSecurity kayýtlarý (provider/hasher vs.)
        services.AddPasswordSecurity();

        // AppDbContext factory'den bir context açýp seed yapacaðýz
        var sp = services.BuildServiceProvider();
        SeedParameters(sp).GetAwaiter().GetResult();
        return sp;
    }

    private static async Task SeedParameters(ServiceProvider sp)
    {
        var factory = sp.GetRequiredService<IDbContextFactory<AppDbContext>>();
        await using var db = await factory.CreateDbContextAsync(CancellationToken.None);

        // Gerekli tablolar InMemory’de otomatik, mevcutsa ekleme
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
                Id = 999, // InMemory için herhangi bir id
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
}

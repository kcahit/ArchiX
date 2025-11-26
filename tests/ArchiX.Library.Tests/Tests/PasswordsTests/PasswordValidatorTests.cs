using ArchiX.Library.Abstractions.Security;
using ArchiX.Library.Context;
using ArchiX.Library.Runtime.Security;

using Microsoft.EntityFrameworkCore;
using ArchiX.Library.Entities; // Parameter için

using Xunit;

namespace ArchiX.Library.Tests.Tests.PasswordsTests;

public sealed class PasswordValidatorTests
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
    public async Task Validate_Returns_Success_For_Compliant_Password()
    {
        var services = new ServiceCollection();
        services.AddMemoryCache();
        services.AddDbContextFactory<AppDbContext>(o => o.UseInMemoryDatabase("ValidatorOk"));
        ArchiX.Library.Runtime.Security.PasswordSecurityServiceCollectionExtensions.AddPasswordSecurity(services);
        var sp = services.BuildServiceProvider();
        await SeedPolicyAsync(sp, 2001);

        var provider = sp.GetRequiredService<IPasswordPolicyProvider>();
        var policy = await provider.GetAsync(1, CancellationToken.None);

        var errors = PasswordPolicyValidator.Validate("GoodP@ssw0rd!", policy);
        Assert.Empty(errors);
    }

    [Fact]
    public async Task Validate_Returns_Errors_For_Too_Short_And_Missing_Categories()
    {
        var services = new ServiceCollection();
        services.AddMemoryCache();
        services.AddDbContextFactory<AppDbContext>(o => o.UseInMemoryDatabase("ValidatorFailShort"));
        ArchiX.Library.Runtime.Security.PasswordSecurityServiceCollectionExtensions.AddPasswordSecurity(services);
        var sp = services.BuildServiceProvider();
        await SeedPolicyAsync(sp, 2002);

        var provider = sp.GetRequiredService<IPasswordPolicyProvider>();
        var policy = await provider.GetAsync(1, CancellationToken.None);

        var errors = PasswordPolicyValidator.Validate("short", policy);

        Assert.Contains(errors, e => e.Equals("MIN_LENGTH", System.StringComparison.OrdinalIgnoreCase));
        Assert.Contains(errors, e =>
            e.Equals("REQ_UPPER", System.StringComparison.OrdinalIgnoreCase) ||
            e.Equals("REQ_LOWER", System.StringComparison.OrdinalIgnoreCase) ||
            e.Equals("REQ_DIGIT", System.StringComparison.OrdinalIgnoreCase) ||
            e.Equals("REQ_SYMBOL", System.StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Validate_Detects_Repeated_Sequences()
    {
        var services = new ServiceCollection();
        services.AddMemoryCache();
        services.AddDbContextFactory<AppDbContext>(o => o.UseInMemoryDatabase("ValidatorRepeat"));
        ArchiX.Library.Runtime.Security.PasswordSecurityServiceCollectionExtensions.AddPasswordSecurity(services);
        var sp = services.BuildServiceProvider();
        await SeedPolicyAsync(sp, 2003);

        var provider = sp.GetRequiredService<IPasswordPolicyProvider>();
        var policy = await provider.GetAsync(1, CancellationToken.None);

        var errors = PasswordPolicyValidator.Validate("GoodP@ssaaaa123", policy);
        Assert.Contains(errors, e => e.Equals("REPEAT_SEQ", System.StringComparison.OrdinalIgnoreCase));
    }
}

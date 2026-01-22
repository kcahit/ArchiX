using ArchiX.Library.Context;
using ArchiX.Library.Entities;
using ArchiX.Library.Runtime.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ArchiX.Library.Tests.Tests.PasswordsTests;

public static class TestServiceHelper
{
    public static ServiceProvider BuildServices(string dbName = "PolicyTests")
    {
        var services = new ServiceCollection();
        services.AddMemoryCache();
        services.AddDbContextFactory<AppDbContext>(o => o.UseInMemoryDatabase(dbName));
        services.AddPasswordSecurity();

        var sp = services.BuildServiceProvider();
        SeedParameters(sp).GetAwaiter().GetResult();
        return sp;
    }

    private static async Task SeedParameters(ServiceProvider sp)
    {
        var factory = sp.GetRequiredService<IDbContextFactory<AppDbContext>>();
        await using var db = await factory.CreateDbContextAsync(CancellationToken.None);

        // Yeni şema: Parameter (tanım) + ParameterApplication (değer)
        var param = await db.Parameters
            .Include(p => p.Applications)
            .FirstOrDefaultAsync(p => p.Group == "Security" && p.Key == "PasswordPolicy");

        if (param == null)
        {
            param = new Parameter
            {
                Id = 999,
                Group = "Security",
                Key = "PasswordPolicy",
                ParameterDataTypeId = 15,
                Description = "Test seed",
                StatusId = 3,
                CreatedBy = 0,
                LastStatusBy = 0,
                IsProtected = false,
                RowId = Guid.NewGuid(),
                CreatedAt = DateTimeOffset.UtcNow
            };
            db.Parameters.Add(param);
            await db.SaveChangesAsync();
        }

        var appValue = param.Applications.FirstOrDefault(a => a.ApplicationId == 1);
        if (appValue == null)
        {
            var policyJson = @"{
  ""version"": 1,
  ""minLength"": 12,
  ""maxLength"": 128,
  ""requireUpper"": true,
  ""requireLower"": true,
  ""requireDigit"": true,
  ""requireSymbol"": true,
  ""allowedSymbols"": ""!@#$%^&*_-+=:?.,;"",
  ""minDistinctChars"": 5,
  ""maxRepeatedSequence"": 3,
  ""blockList"": [""password"",""123456"",""qwerty"",""admin""],
  ""historyCount"": 10,
  ""lockoutThreshold"": 5,
  ""lockoutSeconds"": 900,
  ""hash"": {
    ""algorithm"": ""Argon2id"",
    ""memoryKb"": 32768,
    ""parallelism"": 2,
    ""iterations"": 3,
    ""saltLength"": 16,
    ""hashLength"": 32,
    ""fallback"": { ""algorithm"": ""PBKDF2-SHA512"", ""iterations"": 210000 },
    ""pepperEnabled"": true
  }
}";

            db.ParameterApplications.Add(new ParameterApplication
            {
                Id = 9999,
                ParameterId = param.Id,
                ApplicationId = 1,
                Value = policyJson,
                StatusId = 3,
                CreatedBy = 0,
                LastStatusBy = 0,
                IsProtected = false,
                RowId = Guid.NewGuid(),
                CreatedAt = DateTimeOffset.UtcNow
            });

            await db.SaveChangesAsync();
        }
    }
}

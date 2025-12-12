using System.Text.Json;

using ArchiX.Library.Abstractions.Security;
using ArchiX.Library.Context;
using ArchiX.Library.Entities;
using ArchiX.Library.Runtime.Security;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

using Xunit;

namespace ArchiX.Library.Tests.Tests.SecurityTests
{
    public sealed class PasswordPolicyConcurrencyTests
    {
        private static readonly JsonSerializerOptions WebOpts = new(JsonSerializerDefaults.Web);

        private static ServiceProvider CreateServices()
        {
            var s = new ServiceCollection();
            s.AddMemoryCache();
            s.AddDbContextFactory<AppDbContext>(o => o.UseInMemoryDatabase("pp-concurrency"));
            s.AddSingleton<IPasswordPolicyProvider>(sp =>
            {
                var cache = sp.GetRequiredService<IMemoryCache>();
                var dbf = sp.GetRequiredService<IDbContextFactory<AppDbContext>>();
                return new PasswordPolicyProvider(cache, dbf);
            });
            s.AddSingleton<IPasswordPolicyAdminService, PasswordPolicyAdminService>();
            return s.BuildServiceProvider();
        }

        [Fact]
        public async Task Update_Conflicts_When_RowVersion_Changes()
        {
            var sp = CreateServices();
            var admin = sp.GetRequiredService<IPasswordPolicyAdminService>();
            var provider = sp.GetRequiredService<IPasswordPolicyProvider>();
            var dbf = sp.GetRequiredService<IDbContextFactory<AppDbContext>>();

            // Seed
            await using (var db = await dbf.CreateDbContextAsync())
            {
                db.Parameters.Add(new Parameter
                {
                    ApplicationId = 1,
                    Group = "Security",
                    Key = "PasswordPolicy",
                    ParameterDataTypeId = 15,
                    Value = JsonSerializer.Serialize(new PasswordPolicyOptions(), WebOpts),
                    RowVersion = System.Security.Cryptography.RandomNumberGenerator.GetBytes(8),
                    Description = "seed"
                });
                await db.SaveChangesAsync();
            }

            // Ýstemci A
            var aJson = "{\"version\":1,\"minLength\":12,\"maxLength\":128,\"requireUpper\":true,\"requireLower\":true,\"requireDigit\":true,\"requireSymbol\":true,\"allowedSymbols\":\"!@#$%^&*_-+=:?.,;\",\"minDistinctChars\":5,\"maxRepeatedSequence\":3,\"blockList\":[\"password\"],\"historyCount\":10,\"lockoutThreshold\":5,\"lockoutSeconds\":900,\"hash\":{\"algorithm\":\"Argon2id\",\"memoryKb\":65536,\"parallelism\":2,\"iterations\":3,\"saltLength\":16,\"hashLength\":32,\"fallback\":{\"algorithm\":\"PBKDF2-SHA512\",\"iterations\":210000},\"pepperEnabled\":false}}";
            // Ýstemci B
            var bJson = "{\"version\":1,\"minLength\":16,\"maxLength\":128,\"requireUpper\":true,\"requireLower\":true,\"requireDigit\":true,\"requireSymbol\":true,\"allowedSymbols\":\"!@#$%^&*_-+=:?.,;\",\"minDistinctChars\":6,\"maxRepeatedSequence\":3,\"blockList\":[\"password\"],\"historyCount\":10,\"lockoutThreshold\":5,\"lockoutSeconds\":900,\"hash\":{\"algorithm\":\"Argon2id\",\"memoryKb\":65536,\"parallelism\":2,\"iterations\":3,\"saltLength\":16,\"hashLength\":32,\"fallback\":{\"algorithm\":\"PBKDF2-SHA512\",\"iterations\":210000},\"pepperEnabled\":false}}";

            // Seed sonrasý istemci görüntüsündeki RowVersion'ý al
            byte[] clientRowVersion;
            await using (var db = await dbf.CreateDbContextAsync())
            {
                var seedEntity = await db.Parameters.AsNoTracking()
                    .FirstAsync(x => x.ApplicationId == 1 && x.Group == "Security" && x.Key == "PasswordPolicy");

                // RowVersion: SQL Server'da DB tarafýndan doldurulur; InMemory'de null kalýr.
                if (db.Database.IsRelational())
                    Assert.NotNull(seedEntity.RowVersion);
                // Ýstemci görüntüsündeki versiyon: relational deðilse sahte bir deðer üret.
                clientRowVersion = seedEntity.RowVersion ?? System.Security.Cryptography.RandomNumberGenerator.GetBytes(8);
            }

            // A kaydeder ? RowVersion deðiþir
            await admin.UpdateAsync(aJson, 1);

            // B eski RowVersion ile kaydetmeye çalýþýr ? eþzamanlýlýk hatasý beklenir
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                ((PasswordPolicyAdminService)admin).UpdateAsync(bJson, 1, clientRowVersion));
            Assert.Contains("çakýþma", ex.Message, StringComparison.InvariantCultureIgnoreCase);

            // Provider’ýn gördüðü deðer A olmalý
            var policy = await provider.GetAsync(1);
            Assert.Equal(12, policy.MinLength);
        }
    }
}

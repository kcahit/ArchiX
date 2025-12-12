using System.Text.Json;

using ArchiX.Library.Abstractions.Security;
using ArchiX.Library.Context;
using ArchiX.Library.Entities;
using ArchiX.Library.Runtime.Security;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

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

            s.AddDbContextFactory<AppDbContext>(o =>
            {
                o.UseInMemoryDatabase("pp-concurrency");

                // InMemory provider transaction desteklemediği için bu uyarıyı exception'a çevirmesin
                o.ConfigureWarnings(w =>
                    w.Ignore(InMemoryEventId.TransactionIgnoredWarning));
            });

            s.AddPasswordSecurity();

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

            // İstemci A
            var aJson = "{\"version\":1,\"minLength\":12,\"maxLength\":128,\"requireUpper\":true,\"requireLower\":true,\"requireDigit\":true,\"requireSymbol\":true,\"allowedSymbols\":\"!@#$%^&*_-+=:?.,;\",\"minDistinctChars\":5,\"maxRepeatedSequence\":3,\"blockList\":[\"password\"],\"historyCount\":10,\"lockoutThreshold\":5,\"lockoutSeconds\":900,\"hash\":{\"algorithm\":\"Argon2id\",\"memoryKb\":65536,\"parallelism\":2,\"iterations\":3,\"saltLength\":16,\"hashLength\":32,\"fallback\":{\"algorithm\":\"PBKDF2-SHA512\",\"iterations\":210000},\"pepperEnabled\":false}}";
            // İstemci B
            var bJson = "{\"version\":1,\"minLength\":16,\"maxLength\":128,\"requireUpper\":true,\"requireLower\":true,\"requireDigit\":true,\"requireSymbol\":true,\"allowedSymbols\":\"!@#$%^&*_-+=:?.,;\",\"minDistinctChars\":6,\"maxRepeatedSequence\":3,\"blockList\":[\"password\"],\"historyCount\":10,\"lockoutThreshold\":5,\"lockoutSeconds\":900,\"hash\":{\"algorithm\":\"Argon2id\",\"memoryKb\":65536,\"parallelism\":2,\"iterations\":3,\"saltLength\":16,\"hashLength\":32,\"fallback\":{\"algorithm\":\"PBKDF2-SHA512\",\"iterations\":210000},\"pepperEnabled\":false}}";

            // Seed sonrası istemci görüntüsündeki RowVersion'ı al
            byte[] clientRowVersion;
            await using (var db = await dbf.CreateDbContextAsync())
            {
                var seedEntity = await db.Parameters.AsNoTracking()
                    .FirstAsync(x => x.ApplicationId == 1 && x.Group == "Security" && x.Key == "PasswordPolicy");

                clientRowVersion = seedEntity.RowVersion
                    ?? System.Security.Cryptography.RandomNumberGenerator.GetBytes(8);
            }

            // A kaydeder → RowVersion değişse de değişmese de önemli değil, B'de bilerek yanlış versiyon göndereceğiz
            await admin.UpdateAsync(aJson, 1, null, CancellationToken.None);

            // B: bilinçli olarak yanlış RowVersion ile kaydetmeye çalışsın → eşzamanlılık hatası beklenir
            var wrongRowVersion = (byte[])clientRowVersion.Clone();
            if (wrongRowVersion.Length > 0)
            {
                wrongRowVersion[0] ^= 0xFF; // ilk byte'ı değiştir, kesin farklı olsun
            }

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                ((PasswordPolicyAdminService)admin).UpdateAsync(bJson, 1, wrongRowVersion, CancellationToken.None));

            Assert.Contains("güncellenmiştir", ex.Message, StringComparison.InvariantCultureIgnoreCase);

            // Provider’ın gördüğü değer A olmalı
            var policy = await provider.GetAsync(1, CancellationToken.None);
            Assert.Equal(12, policy.MinLength);
        }
    }
}

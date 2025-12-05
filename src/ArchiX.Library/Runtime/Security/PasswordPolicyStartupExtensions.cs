using ArchiX.Library.Context;
using ArchiX.Library.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ArchiX.Library.Runtime.Security
{
    /// <summary>
    /// PasswordPolicy parametre kaydýnýn startup sýrasýnda idempotent insert edilmesi için extension.
    /// </summary>
    public static class PasswordPolicyStartupExtensions
    {
        private const string DefaultPolicyJson = """
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
  "blockList": ["password", "123456", "qwerty", "admin"],
  "historyCount": 10,
  "lockoutThreshold": 5,
  "lockoutSeconds": 900,
  "hash": {
    "algorithm": "Argon2id",
    "memoryKb": 65536,
    "parallelism": 2,
    "iterations": 3,
    "saltLength": 16,
    "hashLength": 32,
    "fallback": {
      "algorithm": "PBKDF2-SHA512",
      "iterations": 210000
    },
    "pepperEnabled": true
  }
}
""";

        /// <summary>
        /// PasswordPolicy parametresini ApplicationId=1 için kontrol eder; yoksa ekler (idempotent).
        /// Production'da migration dýþýnda da güvenli baþlatma saðlar.
        /// </summary>
        public static async Task EnsurePasswordPolicyParameterAsync(
            this AppDbContext db,
            ILogger logger,
            int applicationId = 1,
            CancellationToken ct = default)
        {
            const string group = "Security";
            const string key = "PasswordPolicy";

            var existing = await db.Parameters
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ApplicationId == applicationId 
                                       && p.Group == group 
                                       && p.Key == key, ct)
                .ConfigureAwait(false);

            if (existing != null)
            {
                logger.LogInformation(
                    "[PasswordPolicy] Parametre kaydý zaten mevcut (AppId={ApplicationId}, Id={Id}).",
                    applicationId, existing.Id);
                return;
            }

            var param = new Parameter
            {
                ApplicationId = applicationId,
                Group = group,
                Key = key,
                ParameterDataTypeId = 15, // Json
                Value = DefaultPolicyJson,
                Description = "Parola politikasý (startup idempotent insert)",
                StatusId = 3, // Approved
                CreatedBy = 0
            };

            db.Parameters.Add(param);
            await db.SaveChangesAsync(ct).ConfigureAwait(false);

            logger.LogInformation(
                "[PasswordPolicy] Parametre kaydý oluþturuldu (AppId={ApplicationId}, Id={Id}).",
                applicationId, param.Id);
        }
    }
}
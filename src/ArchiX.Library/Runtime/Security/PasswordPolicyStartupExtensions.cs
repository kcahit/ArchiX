using ArchiX.Library.Context;
using ArchiX.Library.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ArchiX.Library.Runtime.Security
{
    /// <summary>
    /// PasswordPolicy parametre kaydının startup sırasında idempotent insert edilmesi için extension.
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
        /// Production'da migration dışında da güvenli başlatma sağlar.
        /// </summary>
        public static async Task EnsurePasswordPolicyParameterAsync(
            this AppDbContext db,
            ILogger logger,
            int applicationId = 1,
            CancellationToken ct = default)
        {
            const string group = "Security";
            const string key = "PasswordPolicy";

            var param = await db.Parameters
                .AsNoTracking()
                .Include(p => p.Applications)
                .FirstOrDefaultAsync(p => p.Group == group && p.Key == key, ct)
                .ConfigureAwait(false);

            var appValue = param?.Applications.FirstOrDefault(a => a.ApplicationId == applicationId);

            if (appValue != null)
            {
                logger.LogInformation(
                    "[PasswordPolicy] Parametre değeri zaten mevcut (AppId={ApplicationId}, ParamId={Id}).",
                    applicationId, param!.Id);
                return;
            }

            // Parametre tanımı yoksa oluştur
            if (param == null)
            {
                param = new Parameter
                {
                    Group = group,
                    Key = key,
                    ParameterDataTypeId = 15, // Json
                    Description = "Parola politikası (startup idempotent insert)",
                    StatusId = 3, // Approved
                    CreatedBy = 0,
                    LastStatusBy = 0,
                    IsProtected = true,
                    RowId = Guid.NewGuid(),
                    CreatedAt = DateTimeOffset.UtcNow
                };
                db.Parameters.Add(param);
                await db.SaveChangesAsync(ct).ConfigureAwait(false);
            }

            // Değer ekle
            var paramApp = new ParameterApplication
            {
                ParameterId = param.Id,
                ApplicationId = applicationId,
                Value = DefaultPolicyJson,
                StatusId = 3,
                CreatedBy = 0,
                LastStatusBy = 0,
                IsProtected = true,
                RowId = Guid.NewGuid(),
                CreatedAt = DateTimeOffset.UtcNow
            };

            db.ParameterApplications.Add(paramApp);
            await db.SaveChangesAsync(ct).ConfigureAwait(false);

            logger.LogInformation(
                "[PasswordPolicy] Parametre değeri oluşturuldu (AppId={ApplicationId}, ParamId={Id}).",
                applicationId, param.Id);
        }
    }
}

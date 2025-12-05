using System.Text.Json;

using ArchiX.Library.Abstractions.Security;
using ArchiX.Library.Context;
using ArchiX.Library.Entities;
using ArchiX.Library.Formatting;

using Microsoft.EntityFrameworkCore;
namespace ArchiX.Library.Runtime.Security
{
    internal sealed class PasswordPolicyAdminService : IPasswordPolicyAdminService
    {
        private const string Group = "Security";
        private const string Key = "PasswordPolicy";
        private const int JsonParameterTypeId = 15;
        private readonly IDbContextFactory<AppDbContext> _dbFactory;
        private readonly IPasswordPolicyProvider _provider;
        private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web) { PropertyNameCaseInsensitive = true };

        public PasswordPolicyAdminService(IDbContextFactory<AppDbContext> dbFactory, IPasswordPolicyProvider provider)
        {
            _dbFactory = dbFactory;
            _provider = provider;
        }

        public async Task<string> GetRawJsonAsync(int applicationId = 1, CancellationToken ct = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);
            var entity = await db.Parameters.AsNoTracking()
                .FirstOrDefaultAsync(x => x.ApplicationId == applicationId && x.Group == Group && x.Key == Key, ct)
                .ConfigureAwait(false);

            if (entity is not null && !string.IsNullOrWhiteSpace(entity.Value))
                return entity.Value;

            var options = await _provider.GetAsync(applicationId, ct).ConfigureAwait(false);
            return JsonSerializer.Serialize(options, _jsonOptions);
        }

        // Client RowVersion sağlanmışsa EF Core eşzamanlılık çatışmasını tespit edebilir.
        public async Task UpdateAsync(string json, int applicationId = 1, byte[]? clientRowVersion = null, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(json);

            if (!JsonTextFormatter.TryValidate(json, out var err))
                throw new InvalidOperationException($"PasswordPolicy JSON geçersiz: {err}");

            PasswordPolicySchemaValidator.ValidateOrThrow(json);

            _ = JsonSerializer.Deserialize<PasswordPolicyOptions>(json, _jsonOptions)
                ?? throw new InvalidOperationException("PasswordPolicy tip eşlemesi başarısız.");

            json = JsonTextFormatter.Minify(json);

            await using var db = await _dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);
            Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? tx = null;
            if (db.Database.IsRelational())
                tx = await db.Database.BeginTransactionAsync(ct).ConfigureAwait(false);

            var entity = await db.Parameters
                .FirstOrDefaultAsync(x => x.ApplicationId == applicationId && x.Group == Group && x.Key == Key, ct)
                .ConfigureAwait(false);

            var oldJson = entity?.Value ?? string.Empty;

            if (entity is null)
            {
                entity = new Parameter
                {
                    ApplicationId = applicationId,
                    Group = Group,
                    Key = Key,
                    ParameterDataTypeId = JsonParameterTypeId,
                    Value = json,
                    Description = "Parola politikası (yönetim)",
                    StatusId = 3,
                    CreatedBy = 0
                };
                db.Parameters.Add(entity);
            }
            else
            {
                if (clientRowVersion is not null && clientRowVersion.Length > 0)
                {
                    var entry = db.Entry(entity);
                    entry.Property(nameof(Parameter.RowVersion)).OriginalValue = clientRowVersion;
                }

                entity.ParameterDataTypeId = JsonParameterTypeId;
                entity.Value = json;
                entity.UpdatedAt = DateTimeOffset.UtcNow;

                // For non-relational providers (e.g., InMemory), simulate rowversion changes
                if (!db.Database.IsRelational())
                {
                    entity.RowVersion = System.Security.Cryptography.RandomNumberGenerator.GetBytes(8);
                }
            }

            db.Set<PasswordPolicyAudit>().Add(new PasswordPolicyAudit
            {
                ApplicationId = applicationId,
                UserId = 0,
                OldJson = oldJson,
                NewJson = json,
                StatusId = 3,
                CreatedBy = 0
            });

            try
            {
                await db.SaveChangesAsync(ct).ConfigureAwait(false);
                if (tx != null) await tx.CommitAsync(ct).ConfigureAwait(false);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (tx != null) await tx.RollbackAsync(ct).ConfigureAwait(false);
                throw new InvalidOperationException("Çakışma: kayıt başka bir işlem tarafından değiştirildi. Sayfayı yenileyip tekrar deneyin.");
            }

            _provider.Invalidate(applicationId);
        }

        public Task UpdateAsync(string json, int applicationId = 1, CancellationToken ct = default)
        {
            // Delegate to main overload without client-supplied RowVersion
            return UpdateAsync(json, applicationId, null, ct);
        }
    }
}

using System.Linq;
using System.Text.Json;

using ArchiX.Library.Abstractions.Security;
using ArchiX.Library.Context;
using ArchiX.Library.Entities;
using ArchiX.Library.Formatting;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace ArchiX.Library.Runtime.Security
{
    internal sealed class PasswordPolicyAdminService : IPasswordPolicyAdminService
    {
        private const string Group = "Security";
        private const string Key = "PasswordPolicy";
        private const int JsonParameterTypeId = 15;
        private readonly IDbContextFactory<AppDbContext> _dbFactory;
        private readonly IPasswordPolicyProvider _provider;
        private readonly IMemoryCache _memoryCache;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web) { PropertyNameCaseInsensitive = true };

        public PasswordPolicyAdminService(
            IDbContextFactory<AppDbContext> dbFactory,
            IPasswordPolicyProvider provider,
            IMemoryCache memoryCache,
            IServiceScopeFactory scopeFactory)
        {
            _dbFactory = dbFactory;
            _provider = provider;
            _memoryCache = memoryCache;
            _scopeFactory = scopeFactory;
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

        // Client RowVersion saðlanmýþsa EF Core eþzamanlýlýk çatýþmasýný tespit edebilir.
        public async Task UpdateAsync(string json, int applicationId = 1, byte[]? clientRowVersion = null, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(json);

            if (!JsonTextFormatter.TryValidate(json, out var err))
                throw new InvalidOperationException($"PasswordPolicy JSON geçersiz: {err}");

            PasswordPolicySchemaValidator.ValidateOrThrow(json);

            _ = JsonSerializer.Deserialize<PasswordPolicyOptions>(json, _jsonOptions)
                ?? throw new InvalidOperationException("PasswordPolicy tip eþlemesi baþarýsýz.");

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
                    Description = "Parola politikasý (yönetim)",
                    StatusId = BaseEntity.ApprovedStatusId,
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
                StatusId = BaseEntity.ApprovedStatusId,
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
                throw new InvalidOperationException("Çakýþma: kayýt baþka bir iþlem tarafýndan deðiþtirildi. Sayfayý yenileyip tekrar deneyin.");
            }

            _provider.Invalidate(applicationId);
        }

        public Task UpdateAsync(string json, int applicationId = 1, CancellationToken ct = default)
        {
            // Delegate to main overload without client-supplied RowVersion
            return UpdateAsync(json, applicationId, null, ct);
        }

        public async Task<SecurityDashboardData> GetDashboardDataAsync(int applicationId, CancellationToken ct = default)
        {
            var policy = await _provider.GetAsync(applicationId, ct).ConfigureAwait(false);

            await using var db = await _dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);

            var blacklistTask = db.PasswordBlacklists
                .AsNoTracking()
                .CountAsync(x => x.ApplicationId == applicationId, ct);

            var expiredTask = policy.MaxPasswordAgeDays is > 0
                ? db.Users.AsNoTracking()
                    .CountAsync(
                        u => u.PasswordChangedAtUtc != null &&
                             u.PasswordChangedAtUtc <= DateTimeOffset.UtcNow.AddDays(-policy.MaxPasswordAgeDays.Value),
                        ct)
                : Task.FromResult(0);

            var recentChangesTask = LoadRecentAuditsAsync(db, applicationId, ct);

            await Task.WhenAll(blacklistTask, expiredTask, recentChangesTask).ConfigureAwait(false);

            return new SecurityDashboardData(
                Policy: policy,
                BlacklistWordCount: blacklistTask.Result,
                ExpiredPasswordCount: expiredTask.Result,
                Last30DaysErrors: new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase),
                RecentChanges: recentChangesTask.Result);
        }

        private static async Task<IReadOnlyList<RecentAuditSummary>> LoadRecentAuditsAsync(AppDbContext db, int applicationId, CancellationToken ct)
        {
            var audits = await db.Set<PasswordPolicyAudit>()
                .AsNoTracking()
                .Where(x => x.ApplicationId == applicationId)
                .OrderByDescending(x => x.ChangedAtUtc)
                .Take(5)
                .Select(a => new
                {
                    a.Id,
                    a.ChangedAtUtc,
                    a.UserId,
                    a.OldJson,
                    a.NewJson
                })
                .ToListAsync(ct)
                .ConfigureAwait(false);

            if (audits.Count == 0)
                return Array.Empty<RecentAuditSummary>();

            var userIds = audits.Select(a => a.UserId).Where(id => id > 0).Distinct().ToList();
            var userLookup = userIds.Count == 0
                ? new Dictionary<int, string>()
                : await db.Users
                    .AsNoTracking()
                    .Where(u => userIds.Contains(u.Id))
                    .ToDictionaryAsync(u => u.Id, u => u.DisplayName ?? u.UserName, ct)
                    .ConfigureAwait(false);

            return audits
                .Select(a => new RecentAuditSummary(
                    AuditId: a.Id,
                    ChangedAt: a.ChangedAtUtc,
                    UserDisplayName: userLookup.TryGetValue(a.UserId, out var name) ? name : "System",
                    Summary: BuildAuditSummary(a.OldJson, a.NewJson)))
                .ToArray();
        }

        private static string BuildAuditSummary(string oldJson, string newJson)
        {
            if (string.IsNullOrWhiteSpace(oldJson))
                return "Policy oluþturuldu";

            if (oldJson == newJson)
                return "Deðiþiklik yok";

            return "Policy güncellendi";
        }

        public async Task<IReadOnlyList<PasswordBlacklistWordDto>> GetBlacklistAsync(int applicationId, CancellationToken ct = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);

            var entries = await db.PasswordBlacklists
                .AsNoTracking()
                .Where(x => x.ApplicationId == applicationId)
                .OrderBy(x => x.Word)
                .Select(x => new
                {
                    x.Id,
                    x.Word,
                    x.CreatedBy,
                    x.CreatedAt,
                    x.StatusId
                })
                .ToListAsync(ct)
                .ConfigureAwait(false);

            if (entries.Count == 0)
                return Array.Empty<PasswordBlacklistWordDto>();

            var userIds = entries.Select(e => e.CreatedBy).Where(id => id > 0).Distinct().ToList();
            var userLookup = userIds.Count == 0
                ? new Dictionary<int, string>()
                : await db.Users.AsNoTracking()
                    .Where(u => userIds.Contains(u.Id))
                    .ToDictionaryAsync(u => u.Id, u => u.DisplayName ?? u.UserName, ct)
                    .ConfigureAwait(false);

            return entries
                .Select(e => new PasswordBlacklistWordDto(
                    e.Id,
                    e.Word,
                    userLookup.TryGetValue(e.CreatedBy, out var createdBy) ? createdBy : "System",
                    e.CreatedAt,
                    e.StatusId != BaseEntity.DeletedStatusId))
                .ToArray();
        }

        public async Task<bool> TryAddBlacklistWordAsync(int applicationId, string word, int createdByUserId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(word))
                return false;

            var normalized = word.Trim().ToLowerInvariant();
            if (normalized.Length > 256)
                normalized = normalized[..256];

            await using var db = await _dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);

            var existing = await db.PasswordBlacklists
                .FirstOrDefaultAsync(x => x.ApplicationId == applicationId && x.Word == normalized, ct)
                .ConfigureAwait(false);

            if (existing is not null)
            {
                if (existing.StatusId == BaseEntity.DeletedStatusId)
                {
                    existing.StatusId = BaseEntity.ApprovedStatusId;
                    existing.LastStatusBy = createdByUserId;
                    existing.UpdatedAt = DateTimeOffset.UtcNow;
                    existing.UpdatedBy = createdByUserId;
                    await db.SaveChangesAsync(ct).ConfigureAwait(false);
                    InvalidateBlacklistCache(applicationId);
                    return true;
                }

                return false;
            }

            var entity = new PasswordBlacklist
            {
                ApplicationId = applicationId,
                Word = normalized,
                CreatedBy = createdByUserId,
                StatusId = BaseEntity.ApprovedStatusId,
                LastStatusBy = createdByUserId
            };

            db.PasswordBlacklists.Add(entity);
            await db.SaveChangesAsync(ct).ConfigureAwait(false);
            InvalidateBlacklistCache(applicationId);
            return true;
        }

        public async Task<bool> TryRemoveBlacklistWordAsync(int wordId, int removedByUserId, CancellationToken ct = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);
            var entity = await db.PasswordBlacklists
                .FirstOrDefaultAsync(x => x.Id == wordId && x.StatusId != BaseEntity.DeletedStatusId, ct)
                .ConfigureAwait(false);

            if (entity is null)
                return false;

            entity.SoftDelete(removedByUserId);
            entity.UpdatedAt = DateTimeOffset.UtcNow;
            entity.UpdatedBy = removedByUserId;

            await db.SaveChangesAsync(ct).ConfigureAwait(false);
            InvalidateBlacklistCache(entity.ApplicationId);
            return true;
        }

        public async Task<IReadOnlyList<PasswordPolicyAuditDto>> GetAuditTrailAsync(int applicationId, DateTimeOffset? from, DateTimeOffset? to, CancellationToken ct = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);

            var query = db.Set<PasswordPolicyAudit>()
                .AsNoTracking()
                .Where(x => x.ApplicationId == applicationId);

            if (from.HasValue)
                query = query.Where(x => x.ChangedAtUtc >= from.Value);

            if (to.HasValue)
                query = query.Where(x => x.ChangedAtUtc <= to.Value);

            var audits = await query
                .OrderByDescending(x => x.ChangedAtUtc)
                .Select(a => new
                {
                    a.Id,
                    a.ChangedAtUtc,
                    a.UserId,
                    a.OldJson,
                    a.NewJson
                })
                .ToListAsync(ct)
                .ConfigureAwait(false);

            if (audits.Count == 0)
                return Array.Empty<PasswordPolicyAuditDto>();

            var userIds = audits.Select(a => a.UserId).Where(id => id > 0).Distinct().ToList();
            var userLookup = userIds.Count == 0
                ? new Dictionary<int, string>()
                : await db.Users.AsNoTracking()
                    .Where(u => userIds.Contains(u.Id))
                    .ToDictionaryAsync(u => u.Id, u => u.DisplayName ?? u.UserName, ct)
                    .ConfigureAwait(false);

            return audits
                .Select(a => new PasswordPolicyAuditDto(
                    a.Id,
                    a.ChangedAtUtc,
                    userLookup.TryGetValue(a.UserId, out var name) ? name : "System",
                    BuildAuditSummary(a.OldJson, a.NewJson)))
                .ToArray();
        }

        public async Task<AuditDiffDto?> GetAuditDiffAsync(int auditId, CancellationToken ct = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);
            var audit = await db.Set<PasswordPolicyAudit>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == auditId, ct)
                .ConfigureAwait(false);

            return audit is null
                ? null
                : new AuditDiffDto(audit.Id, audit.OldJson, audit.NewJson);
        }

        public async Task<IReadOnlyList<UserPasswordHistoryEntryDto>> GetUserPasswordHistoryAsync(int userId, int take, CancellationToken ct = default)
        {
            var limit = take <= 0 ? 20 : Math.Min(take, 100);

            await using var db = await _dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);

            var user = await db.Users.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == userId, ct)
                .ConfigureAwait(false);

            if (user is null)
                return Array.Empty<UserPasswordHistoryEntryDto>();

            var histories = await db.UserPasswordHistories
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAtUtc)
                .Take(limit)
                .Select(x => new
                {
                    x.PasswordHash,
                    x.HashAlgorithm,
                    x.CreatedAtUtc
                })
                .ToListAsync(ct)
                .ConfigureAwait(false);

            return histories
               .Select(h => new UserPasswordHistoryEntryDto(
                    user.Id,
                    user.DisplayName ?? user.UserName,
                    MaskHash(h.PasswordHash),
                    h.HashAlgorithm,
                    h.CreatedAtUtc,
                    false))
                .ToArray();
        }

        public async Task<PolicyTestResultDto> ValidatePasswordAsync(string password, int userId, int applicationId, CancellationToken ct = default)
        {
            using var scope = _scopeFactory.CreateScope();
            var validator = scope.ServiceProvider.GetRequiredService<PasswordValidationService>();
            var result = await validator.ValidateAsync(password, userId, applicationId, ct).ConfigureAwait(false);

            var strength = CalculateStrengthScore(password);
            int? historyFlag = result.Errors.Contains("HISTORY", StringComparer.OrdinalIgnoreCase) ? 1 : null;
            int? pwnedFlag = result.Errors.Contains("PWNED", StringComparer.OrdinalIgnoreCase) ? 1 : null;

            return new PolicyTestResultDto(
                result.IsValid,
                result.Errors,
                strength,
                historyFlag,
                pwnedFlag);
        }

        private void InvalidateBlacklistCache(int applicationId)
        {
            var cacheKey = $"blacklist_{applicationId}";
            _memoryCache.Remove(cacheKey);
        }

        private static string MaskHash(string hash)
        {
            if (string.IsNullOrEmpty(hash))
                return string.Empty;

            if (hash.Length <= 6)
                return new string('*', hash.Length);

            return string.Create(hash.Length, hash, static (span, value) =>
            {
                value.AsSpan().CopyTo(span);
                for (int i = 3; i < span.Length - 3; i++)
                {
                    span[i] = '*';
                }
            });
        }

        private static int CalculateStrengthScore(string password)
        {
            if (string.IsNullOrEmpty(password))
                return 0;

            var score = 0;
            if (password.Length >= 12) score += 30;
            if (password.Length >= 16) score += 20;
            if (password.Any(char.IsUpper)) score += 10;
            if (password.Any(char.IsLower)) score += 10;
            if (password.Any(char.IsDigit)) score += 10;
            if (password.Any(ch => !char.IsLetterOrDigit(ch))) score += 10;
            if (password.Distinct().Count() >= 8) score += 10;
            return Math.Min(score, 100);
        }
    }
}

using System.Text.Json;

using ArchiX.Library.Abstractions.Security;
using ArchiX.Library.Context;
using ArchiX.Library.Entities;
using ArchiX.Library.Formatting;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ArchiX.Library.Runtime.Security;

internal sealed class PasswordPolicyAdminService : IPasswordPolicyAdminService
{
    private const string ParameterGroup = "Security";
    private const string ParameterKey = "PasswordPolicy";
    private const int ParameterDataTypeId = 15;
    private const int SystemUserId = 0;

    private static readonly TimeSpan DashboardCacheDuration = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan BlacklistCacheDuration = TimeSpan.FromMinutes(2);
    private static readonly IReadOnlyList<string> EmptyErrors = [];

    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly IPasswordPolicyProvider _policyProvider;
    private readonly IPasswordPolicyVersionUpgrader _versionUpgrader;
    private readonly PasswordPolicyMetrics _metrics;
    private readonly IMemoryCache _memoryCache;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PasswordPolicyAdminService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public PasswordPolicyAdminService(
        IDbContextFactory<AppDbContext> dbFactory,
        IPasswordPolicyProvider policyProvider,
        IPasswordPolicyVersionUpgrader versionUpgrader,
        PasswordPolicyMetrics metrics,
        IMemoryCache memoryCache,
        IServiceScopeFactory scopeFactory,
        ILogger<PasswordPolicyAdminService> logger)
    {
        _dbFactory = dbFactory;
        _policyProvider = policyProvider;
        _versionUpgrader = versionUpgrader;
        _metrics = metrics;
        _memoryCache = memoryCache;
        _scopeFactory = scopeFactory;
        _logger = logger;

        _jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<string> GetRawJsonAsync(int applicationId = 1, CancellationToken ct = default)
    {
        var appId = NormalizeApplicationId(applicationId);

        await using var db = await _dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);
        var parameter = await db.Parameters.AsNoTracking()
            .FirstOrDefaultAsync(p => p.ApplicationId == appId && p.Group == ParameterGroup && p.Key == ParameterKey, ct)
            .ConfigureAwait(false);

        if (parameter is null || string.IsNullOrWhiteSpace(parameter.Value))
            return JsonSerializer.Serialize(new PasswordPolicyOptions(), _jsonOptions);

        if (PasswordPolicyIntegrityChecker.IsEnabled())
        {
            if (string.IsNullOrWhiteSpace(parameter.Template))
            {
                _logger.LogWarning("PasswordPolicy integrity signature missing for AppId {AppId}.", appId);
            }
            else if (!PasswordPolicyIntegrityChecker.VerifySignature(parameter.Value, parameter.Template))
            {
                throw new InvalidOperationException("PasswordPolicy kaydı bütünlük doğrulamasından geçemedi.");
            }
        }

        return JsonTextFormatter.Pretty(parameter.Value);
    }

    public async Task UpdateAsync(string json, int applicationId = 1, byte[]? clientRowVersion = null, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(json))
            throw new ArgumentException("JSON değeri boş olamaz.", nameof(json));

        var appId = NormalizeApplicationId(applicationId);
        var success = false;

        try
        {
            if (!JsonTextFormatter.TryValidate(json, out var jsonError))
                throw new InvalidOperationException($"Geçersiz JSON: {jsonError}");

            var schemaErrors = PasswordPolicySchemaValidator.Validate(json);
            if (schemaErrors.Count > 0)
                throw new InvalidOperationException($"Şema doğrulaması başarısız: {string.Join(", ", schemaErrors)}");

            var upgraded = _versionUpgrader.UpgradeIfNeeded(json);
            ValidatePolicyOptions(upgraded);

            var minified = JsonTextFormatter.Minify(json);
            var signature = PasswordPolicyIntegrityChecker.IsEnabled()
                ? PasswordPolicyIntegrityChecker.ComputeSignature(minified)
                : null;

            await using var db = await _dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);
            await using var transaction = await db.Database.BeginTransactionAsync(ct).ConfigureAwait(false);

            var parameter = await db.Parameters
                .FirstOrDefaultAsync(p => p.ApplicationId == appId && p.Group == ParameterGroup && p.Key == ParameterKey, ct)
                .ConfigureAwait(false);

            var approvedStatusId = ResolveApprovedStatusId(db);
            var oldJson = parameter?.Value ?? string.Empty;
            var utcNow = DateTimeOffset.UtcNow;

            if (parameter is null)
            {
                parameter = new Parameter
                {
                    ApplicationId = appId,
                    Group = ParameterGroup,
                    Key = ParameterKey,
                    ParameterDataTypeId = ParameterDataTypeId,
                    Description = "Security.PasswordPolicy",
                    StatusId = approvedStatusId,
                    CreatedAt = utcNow,
                    CreatedBy = SystemUserId
                };

                db.Parameters.Add(parameter);
            }
            else
            {
                if (clientRowVersion is not null)
                {
                    if (parameter.RowVersion is not null &&
                        !parameter.RowVersion.AsSpan().SequenceEqual(clientRowVersion))
                    {
                        throw new InvalidOperationException("Parola politikası başka bir kullanıcı tarafından güncellenmiştir. Lütfen sayfayı yenileyin.");
                    }

                    db.Entry(parameter)
                        .Property(p => p.RowVersion)
                        .OriginalValue = clientRowVersion;
                }
            }

            parameter.Value = minified;
            parameter.Template = signature;
            parameter.StatusId = approvedStatusId;
            parameter.UpdatedAt = utcNow;
            parameter.UpdatedBy = SystemUserId;

            var audit = new PasswordPolicyAudit
            {
                ApplicationId = appId,
                UserId = SystemUserId,
                OldJson = string.IsNullOrWhiteSpace(oldJson) ? "{}" : oldJson,
                NewJson = minified,
                ChangedAtUtc = utcNow,
                CreatedAt = utcNow,
                CreatedBy = SystemUserId,
                StatusId = approvedStatusId
            };

            db.Set<PasswordPolicyAudit>().Add(audit);

            await db.SaveChangesAsync(ct).ConfigureAwait(false);
            await transaction.CommitAsync(ct).ConfigureAwait(false);

            _policyProvider.Invalidate(appId);
            InvalidateDashboardCache(appId);

            var consistencyWarning = PasswordPolicySymbolsConsistencyChecker.CheckConsistency(upgraded.AllowedSymbols);
            if (consistencyWarning is not null)
                _logger.LogWarning("allowedSymbols tutarsız (AppId={AppId}): {Message}", appId, consistencyWarning);

            _logger.LogInformation("PasswordPolicy güncellendi (AppId={AppId}).", appId);
            success = true;
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new InvalidOperationException("Parola politikası başka bir kullanıcı tarafından güncellenmiştir. Lütfen sayfayı yenileyin.");
        }
        finally
        {
            _metrics.RecordUpdate(appId, success);
        }
    }

    public async Task<SecurityDashboardData> GetDashboardDataAsync(int applicationId, CancellationToken ct = default)
    {
        var appId = NormalizeApplicationId(applicationId);
        var cacheKey = BuildDashboardCacheKey(appId);

        if (_memoryCache.TryGetValue(cacheKey, out SecurityDashboardData? cached) && cached is not null)
            return cached;

        var policy = await _policyProvider.GetAsync(appId, ct).ConfigureAwait(false);
        await using var db = await _dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);

        var approvedStatusId = ResolveApprovedStatusId(db);

        var blacklistCountTask = db.PasswordBlacklists.AsNoTracking()
            .Where(x => x.ApplicationId == appId && x.StatusId == approvedStatusId)
            .CountAsync(ct);

        var expiredTask = CountExpiredPasswordsAsync(db, appId, policy, ct);
        var recentAuditsTask = LoadRecentAuditsAsync(db, appId, ct);

        await Task.WhenAll(blacklistCountTask, expiredTask, recentAuditsTask).ConfigureAwait(false);

        var data = new SecurityDashboardData(
            policy,
            blacklistCountTask.Result,
            expiredTask.Result,
            new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase),
            recentAuditsTask.Result);

        _memoryCache.Set(cacheKey, data, DashboardCacheDuration);

        return data;
    }

    public async Task<IReadOnlyList<PasswordBlacklistWordDto>> GetBlacklistAsync(int applicationId, CancellationToken ct = default)
    {
        var appId = NormalizeApplicationId(applicationId);
        var cacheKey = BuildBlacklistCacheKey(appId);

        if (_memoryCache.TryGetValue(cacheKey, out IReadOnlyList<PasswordBlacklistWordDto>? cached) && cached is not null)
            return cached;

        await using var db = await _dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);
        var approvedStatusId = ResolveApprovedStatusId(db);

        var rawQuery = from word in db.PasswordBlacklists.AsNoTracking()
                       where word.ApplicationId == appId
                       join creator in db.Users.AsNoTracking()
                           on word.CreatedBy equals creator.Id into creatorJoin
                       from creator in creatorJoin.DefaultIfEmpty()
                       orderby word.CreatedAt descending
                       select new { word, creator };

        var rawData = await rawQuery.ToListAsync(ct).ConfigureAwait(false);

        var result = rawData.Select(x => new PasswordBlacklistWordDto(
            x.word.Id,
            x.word.Word,
            BuildUserDisplayName(x.creator, x.word.CreatedBy),
            x.word.CreatedAt,
            x.word.StatusId == approvedStatusId))
            .ToArray();

        _memoryCache.Set(cacheKey, result, BlacklistCacheDuration);

        return result;
    }

    public async Task<bool> TryAddBlacklistWordAsync(int applicationId, string word, int createdByUserId, CancellationToken ct = default)
    {
        var appId = NormalizeApplicationId(applicationId);
        if (string.IsNullOrWhiteSpace(word))
            throw new ArgumentException("Kelime boş olamaz.", nameof(word));

        var trimmed = word.Trim();
        if (trimmed.Length is < 2 or > 256)
            throw new ArgumentException("Kelime uzunluğu 2-256 aralığında olmalıdır.", nameof(word));

        await using var db = await _dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);
        var approvedStatusId = ResolveApprovedStatusId(db);

        var existing = await db.PasswordBlacklists
            .FirstOrDefaultAsync(x => x.ApplicationId == appId && x.Word.Equals(trimmed, StringComparison.OrdinalIgnoreCase), ct)
            .ConfigureAwait(false);

        var utcNow = DateTimeOffset.UtcNow;

        if (existing is not null)
        {
            if (existing.StatusId == approvedStatusId)
                return false;

            existing.StatusId = approvedStatusId;
            existing.UpdatedAt = utcNow;
            existing.UpdatedBy = createdByUserId;
        }
        else
        {
            var entity = new PasswordBlacklist
            {
                ApplicationId = appId,
                Word = trimmed,
                StatusId = approvedStatusId,
                CreatedAt = utcNow,
                CreatedBy = createdByUserId
            };

            db.PasswordBlacklists.Add(entity);
        }

        await db.SaveChangesAsync(ct).ConfigureAwait(false);
        InvalidateBlacklistCache(appId);
        _logger.LogInformation("Blacklist kelimesi eklendi (AppId={AppId}, Kelime={Word}).", appId, trimmed);

        return true;
    }

    public async Task<bool> TryRemoveBlacklistWordAsync(int wordId, int removedByUserId, CancellationToken ct = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(wordId);

        await using var db = await _dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);
        var deletedStatusId = ResolveDeletedStatusId(db);

        var entity = await db.PasswordBlacklists
            .FirstOrDefaultAsync(x => x.Id == wordId, ct)
            .ConfigureAwait(false);

        if (entity is null || entity.StatusId == deletedStatusId)
            return false;

        entity.StatusId = deletedStatusId;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        entity.UpdatedBy = removedByUserId;

        await db.SaveChangesAsync(ct).ConfigureAwait(false);
        InvalidateBlacklistCache(entity.ApplicationId);
        _logger.LogInformation("Blacklist kelimesi silindi (Id={Id}).", wordId);

        return true;
    }

    public async Task<IReadOnlyList<PasswordPolicyAuditDto>> GetAuditTrailAsync(int applicationId, DateTimeOffset? from, DateTimeOffset? to, CancellationToken ct = default)
    {
        var appId = NormalizeApplicationId(applicationId);

        await using var db = await _dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);

        var query = db.Set<PasswordPolicyAudit>().AsNoTracking()
            .Where(x => x.ApplicationId == appId);

        if (from.HasValue)
            query = query.Where(x => x.ChangedAtUtc >= from.Value);

        if (to.HasValue)
            query = query.Where(x => x.ChangedAtUtc <= to.Value);

        var rawQuery =
            from audit in query
            join user in db.Users.AsNoTracking() on audit.UserId equals user.Id into userJoin
            from user in userJoin.DefaultIfEmpty()
            orderby audit.ChangedAtUtc descending
            select new { audit, user };

        var rawData = await rawQuery.ToListAsync(ct).ConfigureAwait(false);

        var result = new List<PasswordPolicyAuditDto>(rawData.Count);

        foreach (var item in rawData)
        {
            result.Add(new PasswordPolicyAuditDto(
                item.audit.Id,
                item.audit.ChangedAtUtc,
                BuildUserDisplayName(item.user, item.audit.UserId),
                BuildAuditSummarySafe(item.audit.OldJson, item.audit.NewJson)));
        }

        return result;
    }

    public async Task<AuditDiffDto?> GetAuditDiffAsync(int auditId, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);

        var audit = await db.Set<PasswordPolicyAudit>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == auditId, ct)
            .ConfigureAwait(false);

        return audit is null
            ? null
            : new AuditDiffDto(
                audit.Id,
                string.IsNullOrWhiteSpace(audit.OldJson) ? "{}" : JsonTextFormatter.Pretty(audit.OldJson),
                JsonTextFormatter.Pretty(audit.NewJson));
    }

    public async Task<IReadOnlyList<UserPasswordHistoryEntryDto>> GetUserPasswordHistoryAsync(int userId, int take, CancellationToken ct = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(userId);

        var takeCount = Math.Clamp(take, 1, 100);

        await using var db = await _dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);

        var applicationId = await db.UserApplications.AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => x.ApplicationId)
            .FirstOrDefaultAsync(ct)
            .ConfigureAwait(false);

        var policy = await _policyProvider.GetAsync(NormalizeApplicationId(applicationId), ct).ConfigureAwait(false);
        var now = DateTimeOffset.UtcNow;

        var query =
            from history in db.UserPasswordHistories.AsNoTracking()
            where history.UserId == userId
            join user in db.Users.AsNoTracking() on history.UserId equals user.Id
            orderby history.CreatedAtUtc descending
            select new { history, user };

        var rows = await query.Take(takeCount).ToListAsync(ct).ConfigureAwait(false);

        var result = new List<UserPasswordHistoryEntryDto>(rows.Count);

        foreach (var row in rows)
        {
            var maxDays = row.user.MaxPasswordAgeDays ?? policy.MaxPasswordAgeDays;
            var isExpired = maxDays.HasValue &&
                            maxDays.Value > 0 &&
                            row.history.CreatedAtUtc.AddDays(maxDays.Value) <= now;

            result.Add(new UserPasswordHistoryEntryDto(
                row.history.UserId,
                BuildUserDisplayName(row.user, row.history.UserId),
                MaskHash(row.history.PasswordHash),
                row.history.HashAlgorithm,
                row.history.CreatedAtUtc,
                isExpired));
        }

        return result;
    }

    public async Task<PolicyTestResultDto> ValidatePasswordAsync(string password, int userId, int applicationId, CancellationToken ct = default)
    {
        var appId = NormalizeApplicationId(applicationId);
        if (string.IsNullOrEmpty(password))
        {
            return new PolicyTestResultDto(
                false,
                new[] { "EMPTY" },
                0,
                null,
                null);
        }

        using var scope = _scopeFactory.CreateScope();
        var validator = scope.ServiceProvider.GetRequiredService<PasswordValidationService>();
        var policy = await _policyProvider.GetAsync(appId, ct).ConfigureAwait(false);

        var result = await validator.ValidateAsync(password, userId, appId, ct).ConfigureAwait(false);
        var strength = CalculateStrengthScore(password);

        int? pwnedCount = null;
        if (result.Errors.Contains("PWNED", StringComparer.OrdinalIgnoreCase))
        {
            var checker = scope.ServiceProvider.GetService<IPasswordPwnedChecker>();
            if (checker is not null)
            {
                try
                {
                    pwnedCount = await checker.GetPwnedCountAsync(password, ct).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "HIBP sorgusu başarısız oldu.");
                }
            }
        }

        int? historyInfo = result.Errors.Contains("HISTORY", StringComparer.OrdinalIgnoreCase)
            ? policy.HistoryCount
            : null;

        return new PolicyTestResultDto(
            result.IsValid,
            result.Errors,
            strength,
            historyInfo,
            pwnedCount);
    }

    private static void ValidatePolicyOptions(PasswordPolicyOptions options)
    {
        if (options.MinLength <= 0)
            throw new InvalidOperationException("MinLength değeri sıfırdan büyük olmalıdır.");

        if (options.MinLength > options.MaxLength)
            throw new InvalidOperationException("MinLength, MaxLength değerinden büyük olamaz.");

        if (options.HistoryCount < 0)
            throw new InvalidOperationException("HistoryCount negatif olamaz.");

        if (options.MaxPasswordAgeDays is < 0)
            throw new InvalidOperationException("MaxPasswordAgeDays negatif olamaz.");
    }

    private static async Task<int> CountExpiredPasswordsAsync(AppDbContext db, int applicationId, PasswordPolicyOptions policy, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        var query =
            from ua in db.UserApplications.AsNoTracking()
            where ua.ApplicationId == applicationId
            join user in db.Users.AsNoTracking() on ua.UserId equals user.Id
            select new { user.Id, user.PasswordChangedAtUtc, user.MaxPasswordAgeDays };

        var rows = await query.ToListAsync(ct).ConfigureAwait(false);
        var count = 0;

        foreach (var row in rows)
        {
            var maxDays = row.MaxPasswordAgeDays ?? policy.MaxPasswordAgeDays;
            if (!maxDays.HasValue || maxDays.Value <= 0)
                continue;

            if (!row.PasswordChangedAtUtc.HasValue)
                continue;

            if (row.PasswordChangedAtUtc.Value.AddDays(maxDays.Value) <= now)
                count++;
        }

        return count;
    }

    private async Task<IReadOnlyList<RecentAuditSummary>> LoadRecentAuditsAsync(AppDbContext db, int applicationId, CancellationToken ct)
    {
        var query =
            from audit in db.Set<PasswordPolicyAudit>().AsNoTracking()
            where audit.ApplicationId == applicationId
            orderby audit.ChangedAtUtc descending
            select audit;

        var rows = await query.Take(5).ToListAsync(ct).ConfigureAwait(false);
        var result = new List<RecentAuditSummary>(rows.Count);

        foreach (var audit in rows)
        {
            var user = await db.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == audit.UserId, ct)
                .ConfigureAwait(false);

            result.Add(new RecentAuditSummary(
                audit.Id,
                audit.ChangedAtUtc,
                BuildUserDisplayName(user, audit.UserId),
                BuildAuditSummarySafe(audit.OldJson, audit.NewJson)));
        }

        return result;
    }

    private string BuildAuditSummarySafe(string? oldJson, string newJson)
    {
        try
        {
            return BuildAuditSummary(oldJson, newJson);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Audit özeti oluşturulamadı.");
            return "Policy updated";
        }
    }

    private string BuildAuditSummary(string? oldJson, string newJson)
    {
        var oldPolicy = string.IsNullOrWhiteSpace(oldJson)
            ? new PasswordPolicyOptions()
            : _versionUpgrader.UpgradeIfNeeded(oldJson);

        var newPolicy = _versionUpgrader.UpgradeIfNeeded(newJson);

        var changes = new List<string>();

        AppendChange(changes, "MinLength", oldPolicy.MinLength, newPolicy.MinLength);
        AppendChange(changes, "MaxLength", oldPolicy.MaxLength, newPolicy.MaxLength);
        AppendChange(changes, "HistoryCount", oldPolicy.HistoryCount, newPolicy.HistoryCount);
        AppendBoolChange(changes, "RequireUpper", oldPolicy.RequireUpper, newPolicy.RequireUpper);
        AppendBoolChange(changes, "RequireDigit", oldPolicy.RequireDigit, newPolicy.RequireDigit);
        AppendChange(changes, "MaxPasswordAgeDays", oldPolicy.MaxPasswordAgeDays, newPolicy.MaxPasswordAgeDays);

        if (changes.Count == 0)
            changes.Add("Policy updated");

        return string.Join(", ", changes.Take(4));

        static void AppendChange<T>(ICollection<string> list, string name, T oldValue, T newValue)
        {
            if (!Equals(oldValue, newValue))
                list.Add($"{name} {oldValue}→{newValue}");
        }

        static void AppendBoolChange(ICollection<string> list, string name, bool oldValue, bool newValue)
        {
            if (oldValue != newValue)
                list.Add($"{name} {(newValue ? "enabled" : "disabled")}");
        }
    }

    private void InvalidateBlacklistCache(int applicationId)
    {
        var cacheKey = BuildBlacklistCacheKey(applicationId);
        _memoryCache.Remove(cacheKey);

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetService<IPasswordBlacklistService>();
            service?.InvalidateCache(applicationId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Blacklist cache invalidate işlemi sırasında hata oluştu.");
        }
    }

    private void InvalidateDashboardCache(int applicationId)
    {
        _memoryCache.Remove(BuildDashboardCacheKey(applicationId));
    }

    private static string BuildUserDisplayName(User? user, int fallbackUserId)
    {
        if (user is null)
            return $"User #{fallbackUserId}";

        return user.DisplayName
            ?? user.UserName
            ?? $"User #{user.Id}";
    }

    private static int ResolveApprovedStatusId(AppDbContext db) => db.ApprovedStatusId == 0 ? 3 : db.ApprovedStatusId;

    private static int ResolveDeletedStatusId(AppDbContext db) => db.DeletedStatusId == 0 ? 5 : db.DeletedStatusId;

    private static string BuildDashboardCacheKey(int applicationId) => $"PasswordPolicy:Dashboard:{applicationId}";

    private static string BuildBlacklistCacheKey(int applicationId) => $"PasswordPolicy:Blacklist:{applicationId}";

    private static int NormalizeApplicationId(int applicationId) => applicationId > 0 ? applicationId : 1;

    private static string MaskHash(string hash)
    {
        if (string.IsNullOrEmpty(hash))
            return string.Empty;

        if (hash.Length <= 8)
            return new string('*', hash.Length);

        return $"{hash[..6]}…{hash[^4..]}";
    }

    private static int CalculateStrengthScore(string password)
    {
        if (string.IsNullOrEmpty(password))
            return 0;

        var score = Math.Min(40, password.Length * 2);

        if (password.Any(char.IsLower))
            score += 15;
        if (password.Any(char.IsUpper))
            score += 15;
        if (password.Any(char.IsDigit))
            score += 15;
        if (password.Any(ch => !char.IsLetterOrDigit(ch)))
            score += 15;

        var distinct = password.Distinct().Count();
        score += Math.Min(10, distinct);

        return Math.Clamp(score, 0, 100);
    }
}

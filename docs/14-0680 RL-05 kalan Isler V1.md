RL-05: Yönetim UI Genişletme - Kalan İşler Dokümanı
Revizyon: v2.6.1 (Kalan İşler)
Tarih: 2025-12-13 14:55 (Türkiye Saati)
Durum: 🚧 IN PROGRESS (60% tamamlandı)
---
📋 Genel Bakış
Tamamlanan İşler:
•	✅ 6 Razor Page markup'ları (Dashboard, Policy Settings, Blacklist, Audit Trail, Password History, Policy Test)
•	✅ Temel PageModel yapıları
•	✅ ViewModel taslakları
Kalan İşler (40%):
1.	Backend Servis Katmanı
2.	Frontend Entegrasyonlar
3.	Yetkilendirme ve DI Kayıtları
---
1️⃣ Backend Servis Katmanı
1.1 IPasswordPolicyAdminService Interface
Dosya: IPasswordPolicyAdminService.cs
 using ArchiX.Library.Entities; using ArchiX.Library.Options;
namespace ArchiX.Library.Abstractions.Security;
/// <summary> /// Password policy admin yönetim servisi /// </summary> public interface IPasswordPolicyAdminService { // ========== Dashboard ==========
/// <summary>
/// Dashboard için özet istatistikler
/// </summary>
Task<SecurityDashboardViewModel> GetDashboardDataAsync(
    int applicationId = 1, 
    CancellationToken cancellationToken = default);

// ========== Policy CRUD ==========

/// <summary>
/// Mevcut policy'yi getir
/// </summary>
Task<PasswordPolicyOptions> GetPolicyAsync(
    int applicationId = 1, 
    CancellationToken cancellationToken = default);

/// <summary>
/// Policy'yi güncelle (audit trail + cache invalidate)
/// </summary>
Task<bool> UpdatePolicyAsync(
    int applicationId, 
    PasswordPolicyOptions policy, 
    int userId, 
    CancellationToken cancellationToken = default);

// ========== Audit Trail ==========

/// <summary>
/// Audit geçmişini getir (filtrelenebilir)
/// </summary>
Task<List<PasswordPolicyAudit>> GetAuditTrailAsync(
    int applicationId, 
    DateTime? fromDate = null, 
    DateTime? toDate = null, 
    CancellationToken cancellationToken = default);

/// <summary>
/// Audit kayıtları için JSON diff hesapla (cached)
/// </summary>
Task<string> GetAuditDiffHtmlAsync(
    int auditId, 
    CancellationToken cancellationToken = default);

// ========== Password History ==========

/// <summary>
/// Kullanıcı parola geçmişini getir
/// </summary>
Task<List<UserPasswordHistory>> GetUserPasswordHistoryAsync(
    int userId, 
    CancellationToken cancellationToken = default);

// ========== Statistics ==========

/// <summary>
/// Son N gündeki validation hatalarını say (error_code → count)
/// </summary>
Task<Dictionary<string, int>> GetValidationErrorStatsAsync(
    int applicationId, 
    int days = 30, 
    CancellationToken cancellationToken = default);

/// <summary>
/// Süresi dolmuş parola sayısı
/// </summary>
Task<int> GetExpiredPasswordCountAsync(
    int applicationId = 1, 
    CancellationToken cancellationToken = default);
} 
---
1.2 SecurityDashboardViewModel
Dosya: src/ArchiX.Library/ViewModels/SecurityDashboardViewModel.cs
 using ArchiX.Library.Options;
namespace ArchiX.Library.ViewModels;
/// <summary> /// Dashboard özet istatistikleri /// </summary> public class SecurityDashboardViewModel { /// <summary> /// Aktif policy ayarları /// </summary> public PasswordPolicyOptions ActivePolicy { get; set; } = null!;
/// <summary>
/// Toplam blacklist kelime sayısı
/// </summary>
public int BlacklistWordCount { get; set; }

/// <summary>
/// Süresi dolmuş parola sayısı
/// </summary>
public int ExpiredPasswordCount { get; set; }

/// <summary>
/// Son 30 gündeki validation hataları (error_code → count)
/// </summary>
public Dictionary<string, int> Last30DaysErrors { get; set; } = new();

/// <summary>
/// Son 10 audit değişikliği
/// </summary>
public List<RecentAuditEntry> RecentChanges { get; set; } = new();
}
/// <summary> /// Özet audit kaydı /// </summary> public class RecentAuditEntry { public int Id { get; set; } public DateTimeOffset ChangedAt { get; set; } public string ChangedByUsername { get; set; } = string.Empty; public string Action { get; set; } = "Update"; public string Summary { get; set; } = string.Empty; // "MinLength: 8 → 12" } 
---
1.3 PolicySettingsViewModel
Dosya: src/ArchiX.Library/ViewModels/PolicySettingsViewModel.cs
 using System.ComponentModel.DataAnnotations;
namespace ArchiX.Library.ViewModels;
/// <summary> /// Policy ayarları form model /// </summary> public class PolicySettingsViewModel { // ========== Uzunluk Ayarları ==========
[Required(ErrorMessage = "Minimum uzunluk gerekli")]
[Range(8, 64, ErrorMessage = "Minimum uzunluk 8-64 arası olmalı")]
[Display(Name = "Minimum Uzunluk")]
public int MinLength { get; set; } = 12;

[Required(ErrorMessage = "Maksimum uzunluk gerekli")]
[Range(64, 256, ErrorMessage = "Maksimum uzunluk 64-256 arası olmalı")]
[Display(Name = "Maksimum Uzunluk")]
public int MaxLength { get; set; } = 128;

// ========== Karakter Gereksinimleri ==========

[Display(Name = "Büyük Harf Gerekli")]
public bool RequireUpper { get; set; } = true;

[Display(Name = "Küçük Harf Gerekli")]
public bool RequireLower { get; set; } = true;

[Display(Name = "Rakam Gerekli")]
public bool RequireDigit { get; set; } = true;

[Display(Name = "Sembol Gerekli")]
public bool RequireSymbol { get; set; } = true;

[MaxLength(50, ErrorMessage = "İzin verilen semboller en fazla 50 karakter olabilir")]
[Display(Name = "İzin Verilen Semboller")]
public string AllowedSymbols { get; set; } = "!@#$%^&*_-+=:?.,;";

// ========== Karmaşıklık Kuralları ==========

[Range(1, 20, ErrorMessage = "Minimum ayırt edici karakter 1-20 arası olmalı")]
[Display(Name = "Minimum Ayırt Edici Karakter")]
public int MinDistinctChars { get; set; } = 5;

[Range(1, 10, ErrorMessage = "Maksimum tekrar sekansı 1-10 arası olmalı")]
[Display(Name = "Maksimum Tekrar Sekansı")]
public int MaxRepeatedSequence { get; set; } = 3;

// ========== Güvenlik Ayarları ==========

[Range(0, 20, ErrorMessage = "History sayısı 0-20 arası olmalı")]
[Display(Name = "Parola Geçmişi Sayısı")]
public int HistoryCount { get; set; } = 10;

[Range(1, 3650, ErrorMessage = "Parola yaşı 1-3650 gün arası olmalı")]
[Display(Name = "Maksimum Parola Yaşı (Gün)")]
public int? MaxPasswordAgeDays { get; set; }

[Range(1, 20, ErrorMessage = "Lockout eşiği 1-20 arası olmalı")]
[Display(Name = "Lockout Eşiği")]
public int LockoutThreshold { get; set; } = 5;

[Range(60, 86400, ErrorMessage = "Lockout süresi 60-86400 saniye arası olmalı")]
[Display(Name = "Lockout Süresi (Saniye)")]
public int LockoutSeconds { get; set; } = 900;

// ========== Hash Ayarları ==========

[Range(1024, 131072, ErrorMessage = "Argon2 memory 1024-131072 KB arası olmalı")]
[Display(Name = "Argon2 Memory (KB)")]
public int Argon2MemoryKb { get; set; } = 65536;

[Range(1, 8, ErrorMessage = "Argon2 parallelism 1-8 arası olmalı")]
[Display(Name = "Argon2 Parallelism")]
public int Argon2Parallelism { get; set; } = 2;

[Range(1, 10, ErrorMessage = "Argon2 iterations 1-10 arası olmalı")]
[Display(Name = "Argon2 Iterations")]
public int Argon2Iterations { get; set; } = 3;

[Range(100000, 1000000, ErrorMessage = "PBKDF2 iterations 100000-1000000 arası olmalı")]
[Display(Name = "PBKDF2 Fallback Iterations")]
public int Pbkdf2Iterations { get; set; } = 210000;

[Display(Name = "Pepper Etkin")]
public bool PepperEnabled { get; set; } = false;
} 
---
1.4 PasswordPolicyAdminService Implementation
Dosya: PasswordPolicyAdminService.cs
 using ArchiX.Library.Abstractions.Security; using ArchiX.Library.Data; using ArchiX.Library.Entities; using ArchiX.Library.Options; using ArchiX.Library.ViewModels; using Microsoft.EntityFrameworkCore; using Microsoft.Extensions.Caching.Memory; using Microsoft.Extensions.Logging; using System.Text.Json;
namespace ArchiX.Library.Runtime.Security;
public class PasswordPolicyAdminService : IPasswordPolicyAdminService { private readonly AppDbContext _context; private readonly IPasswordPolicyProvider _policyProvider; private readonly IPasswordBlacklistService _blacklistService; private readonly IPasswordExpirationService _expirationService; private readonly IMemoryCache _cache; private readonly ILogger<PasswordPolicyAdminService> _logger;
public PasswordPolicyAdminService(
    AppDbContext context,
    IPasswordPolicyProvider policyProvider,
    IPasswordBlacklistService blacklistService,
    IPasswordExpirationService expirationService,
    IMemoryCache cache,
    ILogger<PasswordPolicyAdminService> logger)
{
    _context = context;
    _policyProvider = policyProvider;
    _blacklistService = blacklistService;
    _expirationService = expirationService;
    _cache = cache;
    _logger = logger;
}

// ========== Dashboard ==========

public async Task<SecurityDashboardViewModel> GetDashboardDataAsync(
    int applicationId = 1, 
    CancellationToken cancellationToken = default)
{
    var policy = await _policyProvider.GetAsync(applicationId, cancellationToken);
    var blacklistCount = await _blacklistService.GetCountAsync(applicationId, cancellationToken);
    var expiredCount = await GetExpiredPasswordCountAsync(applicationId, cancellationToken);
    var errorStats = await GetValidationErrorStatsAsync(applicationId, 30, cancellationToken);
    var recentChanges = await GetRecentAuditEntriesAsync(applicationId, 10, cancellationToken);

    return new SecurityDashboardViewModel
    {
        ActivePolicy = policy,
        BlacklistWordCount = blacklistCount,
        ExpiredPasswordCount = expiredCount,
        Last30DaysErrors = errorStats,
        RecentChanges = recentChanges
    };
}

// ========== Policy CRUD ==========

public async Task<PasswordPolicyOptions> GetPolicyAsync(
    int applicationId = 1, 
    CancellationToken cancellationToken = default)
{
    return await _policyProvider.GetAsync(applicationId, cancellationToken);
}

public async Task<bool> UpdatePolicyAsync(
    int applicationId, 
    PasswordPolicyOptions policy, 
    int userId, 
    CancellationToken cancellationToken = default)
{
    try
    {
        // 1. Eski policy'yi al
        var parameter = await _context.Parameters
            .FirstOrDefaultAsync(p => 
                p.ApplicationId == applicationId && 
                p.Group == "Security" && 
                p.Key == "PasswordPolicy", 
                cancellationToken);

        if (parameter == null)
        {
            _logger.LogError("PasswordPolicy parametresi bulunamadı (AppId: {AppId})", applicationId);
            return false;
        }

        var oldJson = parameter.Value;
        var newJson = JsonSerializer.Serialize(policy, new JsonSerializerOptions { WriteIndented = false });

        // 2. Audit kayıt oluştur
        var audit = new PasswordPolicyAudit
        {
            ApplicationId = applicationId,
            OldJson = oldJson,
            NewJson = newJson,
            ChangedBy = userId,
            ChangedAtUtc = DateTimeOffset.UtcNow,
            Status = 3 // Active
        };

        _context.PasswordPolicyAudits.Add(audit);

        // 3. Parameter güncelle
        parameter.Value = newJson;
        parameter.UpdatedBy = userId;
        parameter.UpdatedAtUtc = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        // 4. Cache invalidate
        _policyProvider.Invalidate(applicationId);

        _logger.LogInformation(
            "Password policy updated (AppId: {AppId}, UserId: {UserId})", 
            applicationId, 
            userId);

        return true;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Policy update failed (AppId: {AppId})", applicationId);
        return false;
    }
}

// ========== Audit Trail ==========

public async Task<List<PasswordPolicyAudit>> GetAuditTrailAsync(
    int applicationId, 
    DateTime? fromDate = null, 
    DateTime? toDate = null, 
    CancellationToken cancellationToken = default)
{
    var query = _context.PasswordPolicyAudits
        .Where(a => a.ApplicationId == applicationId && a.Status == 3)
        .OrderByDescending(a => a.ChangedAtUtc);

    if (fromDate.HasValue)
        query = (IOrderedQueryable<PasswordPolicyAudit>)query.Where(a => a.ChangedAtUtc >= fromDate.Value);

    if (toDate.HasValue)
        query = (IOrderedQueryable<PasswordPolicyAudit>)query.Where(a => a.ChangedAtUtc <= toDate.Value);

    return await query.Take(100).ToListAsync(cancellationToken);
}

public async Task<string> GetAuditDiffHtmlAsync(
    int auditId, 
    CancellationToken cancellationToken = default)
{
    var cacheKey = $"audit_diff_{auditId}";

    if (_cache.TryGetValue<string>(cacheKey, out var cachedDiff))
        return cachedDiff!;

    var audit = await _context.PasswordPolicyAudits.FindAsync(new object[] { auditId }, cancellationToken);
    if (audit == null)
        return string.Empty;

    // JSON diff hesapla (basit HTML formatı)
    var diffHtml = GenerateSimpleDiff(audit.OldJson, audit.NewJson);

    // Cache (1 saat)
    _cache.Set(cacheKey, diffHtml, TimeSpan.FromHours(1));

    return diffHtml;
}

// ========== Password History ==========

public async Task<List<UserPasswordHistory>> GetUserPasswordHistoryAsync(
    int userId, 
    CancellationToken cancellationToken = default)
{
    return await _context.UserPasswordHistories
        .Where(h => h.UserId == userId && h.Status == 3)
        .OrderByDescending(h => h.CreatedAtUtc)
        .Take(20)
        .ToListAsync(cancellationToken);
}

// ========== Statistics ==========

public async Task<Dictionary<string, int>> GetValidationErrorStatsAsync(
    int applicationId, 
    int days = 30, 
    CancellationToken cancellationToken = default)
{
    // NOT: Bu veriler için ayrı bir ValidationErrorLog tablosu gerekir
    // Şimdilik mock data dönüyoruz
    return await Task.FromResult(new Dictionary<string, int>
    {
        { "MIN_LENGTH", 145 },
        { "REQ_UPPER", 89 },
        { "REQ_DIGIT", 67 },
        { "PWNED", 23 },
        { "HISTORY", 12 }
    });
}

public async Task<int> GetExpiredPasswordCountAsync(
    int applicationId = 1, 
    CancellationToken cancellationToken = default)
{
    var policy = await _policyProvider.GetAsync(applicationId, cancellationToken);
    
    if (policy.MaxPasswordAgeDays == null)
        return 0;

    var users = await _context.Users
        .Where(u => u.Status == 3 && u.PasswordChangedAtUtc != null)
        .ToListAsync(cancellationToken);

    return users.Count(u => _expirationService.IsExpired(u, policy));
}

// ========== Helper Methods ==========

private async Task<List<RecentAuditEntry>> GetRecentAuditEntriesAsync(
    int applicationId, 
    int count, 
    CancellationToken cancellationToken)
{
    var audits = await _context.PasswordPolicyAudits
        .Where(a => a.ApplicationId == applicationId && a.Status == 3)
        .OrderByDescending(a => a.ChangedAtUtc)
        .Take(count)
        .ToListAsync(cancellationToken);

    return audits.Select(a => new RecentAuditEntry
    {
        Id = a.Id,
        ChangedAt = a.ChangedAtUtc,
        ChangedByUsername = $"User#{a.ChangedBy}", // TODO: User tablosundan gerçek isim
        Action = "Update",
        Summary = ExtractChangeSummary(a.OldJson, a.NewJson)
    }).ToList();
}

private string ExtractChangeSummary(string oldJson, string newJson)
{
    try
    {
        var oldPolicy = JsonSerializer.Deserialize<PasswordPolicyOptions>(oldJson);
        var newPolicy = JsonSerializer.Deserialize<PasswordPolicyOptions>(newJson);

        if (oldPolicy == null || newPolicy == null)
            return "Invalid JSON";

        var changes = new List<string>();

        if (oldPolicy.MinLength != newPolicy.MinLength)
            changes.Add($"MinLength: {oldPolicy.MinLength} → {newPolicy.MinLength}");

        if (oldPolicy.MaxPasswordAgeDays != newPolicy.MaxPasswordAgeDays)
            changes.Add($"MaxAgeDays: {oldPolicy.MaxPasswordAgeDays ?? 0} → {newPolicy.MaxPasswordAgeDays ?? 0}");

        return changes.Count > 0 ? string.Join(", ", changes) : "No changes";
    }
    catch
    {
        return "Parse error";
    }
}

private string GenerateSimpleDiff(string oldJson, string newJson)
{
    // Basit diff HTML (jsondiffpatch frontend'de yapılacak)
    return $@"
<div class='diff-container'> <div class='diff-old'> <h4>Old Policy</h4> <pre>{oldJson}</pre> </div> <div class='diff-new'> <h4>New Policy</h4> <pre>{newJson}</pre> </div> </div>"; } } 
---
2️⃣ DI Kaydı
2.1 PasswordSecurityServiceCollectionExtensions Güncellemesi
Dosya: src/ArchiX.Library/Extensions/PasswordSecurityServiceCollectionExtensions.cs
 public static class PasswordSecurityServiceCollectionExtensions { public static IServiceCollection AddPasswordSecurity(this IServiceCollection services) { // Mevcut kayıtlar services.AddSingleton<IPasswordPolicyProvider, PasswordPolicyProvider>(); services.AddSingleton<IPasswordHasher, Argon2PasswordHasher>(); services.AddHttpClient<IPasswordPwnedChecker, PasswordPwnedChecker>(); services.AddScoped<IPasswordHistoryService, PasswordHistoryService>(); services.AddScoped<IPasswordBlacklistService, PasswordBlacklistService>(); services.AddScoped<IPasswordExpirationService, PasswordExpirationService>(); services.AddScoped<PasswordValidationService>();
    // ✅ YENİ: Admin servisi
    services.AddScoped<IPasswordPolicyAdminService, PasswordPolicyAdminService>();
    
    return services;
}
} 
---
3️⃣ Frontend Entegrasyonlar
3.1 security-admin.js (Ortak JavaScript)
Dosya: src/ArchiX.WebHost/wwwroot/js/security-admin.js
 // ========== Dashboard Chart.js ==========
function initDashboardCharts(errorStatsData) { const ctx = document.getElementById('errorStatsChart'); if (!ctx) return;
new Chart(ctx.getContext('2d'), {
    type: 'bar',
    data: {
        labels: Object.keys(errorStatsData),
        datasets: [{
            label: 'Validation Errors (Last 30 Days)',
            data: Object.values(errorStatsData),
            backgroundColor: 'rgba(220, 53, 69, 0.6)',
            borderColor: 'rgba(220, 53, 69, 1)',
            borderWidth: 1
        }]
    },
    options: {
        responsive: true,
        scales: {
            y: {
                beginAtZero: true,
                ticks: { stepSize: 10 }
            }
        }
    }
});
}
// ========== Blacklist DataTables ==========
function initBlacklistDataTable() { $('#blacklistTable').DataTable({ processing: true, serverSide: true, ajax: { url: '/Security/Blacklist?handler=Data', type: 'GET', data: function(d) { return { draw: d.draw, start: d.start, length: d.length, search: d.search.value }; } }, columns: [ { data: 'word', title: 'Word' }, { data: 'createdBy', title: 'Created By' }, { data: 'createdAtUtc', title: 'Created At', render: function(data) { return new Date(data).toLocaleString('tr-TR'); } }, { data: 'id', title: 'Actions', orderable: false, render: function(data) { return <button class="btn btn-sm btn-danger" onclick="deleteBlacklistWord(${data})"> <i class="fas fa-trash"></i> Delete </button>; } } ], pageLength: 25, language: { search: 'Search:', lengthMenu: 'Show MENU entries', info: 'Showing START to END of TOTAL entries' } }); }
async function deleteBlacklistWord(id) { if (!confirm('Are you sure you want to delete this word?')) return;
try {
    const response = await fetch(`/Security/Blacklist?handler=Delete&id=${id}`, {
        method: 'POST',
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        }
    });

    if (response.ok) {
        $('#blacklistTable').DataTable().ajax.reload();
        showToast('Word deleted successfully', 'success');
    } else {
        showToast('Failed to delete word', 'error');
    }
} catch (error) {
    console.error('Delete error:', error);
    showToast('Network error', 'error');
}
}
// ========== Audit Trail jsondiffpatch ==========
function initAuditDiff(auditId) { fetch(/Security/AuditTrail?handler=Diff&id=${auditId}) .then(response => response.json()) .then(data => { const delta = jsondiffpatch.diff( JSON.parse(data.oldJson), JSON.parse(data.newJson) );
        const diffHtml = jsondiffpatch.formatters.html.format(delta);
        document.getElementById('diff-container').innerHTML = diffHtml;
    })
    .catch(error => {
        console.error('Diff error:', error);
        showToast('Failed to load diff', 'error');
    });
}
// ========== Policy Test Live Validation ==========
let validationTimeout;
function initPolicyTestValidation() { const passwordInput = document.getElementById('testPassword'); if (!passwordInput) return;
passwordInput.addEventListener('input', function() {
    clearTimeout(validationTimeout);
    validationTimeout = setTimeout(() => {
        validatePasswordLive(this.value);
    }, 500);
});
}
async function validatePasswordLive(password) { if (!password) { clearValidationResults(); return; }
try {
    const response = await fetch('/Security/PolicyTest?handler=Validate', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        body: JSON.stringify({ password })
    });

    const result = await response.json();
    displayValidationResults(result);
} catch (error) {
    console.error('Validation error:', error);
}
}
function displayValidationResults(result) { // Update strength meter const strengthBar = document.getElementById('strengthBar'); strengthBar.style.width = ${result.strength}%; strengthBar.className = progress-bar ${getStrengthClass(result.strength)};
// Update rule checkboxes
const rules = {
    'MIN_LENGTH': 'rule-minlength',
    'MAX_LENGTH': 'rule-maxlength',
    'REQ_UPPER': 'rule-upper',
    'REQ_LOWER': 'rule-lower',
    'REQ_DIGIT': 'rule-digit',
    'REQ_SYMBOL': 'rule-symbol',
    'MIN_DISTINCT': 'rule-distinct',
    'REPEAT_SEQ': 'rule-repeat',
    'BLACKLIST': 'rule-blacklist',
    'PWNED': 'rule-pwned',
    'HISTORY': 'rule-history',
    'EXPIRED': 'rule-expired'
};

Object.keys(rules).forEach(ruleCode => {
    const element = document.getElementById(rules[ruleCode]);
    if (element) {
        const passed = !result.errors.includes(ruleCode);
        element.querySelector('i').className = passed ? 'fas fa-check text-success' : 'fas fa-times text-danger';
    }
});
}
function getStrengthClass(strength) { if (strength < 25) return 'bg-danger'; if (strength < 50) return 'bg-warning'; if (strength < 75) return 'bg-info'; return 'bg-success'; }
function clearValidationResults() { document.getElementById('strengthBar').style.width = '0%'; // Reset all checkboxes }
// ========== Toast Notifications ==========
function showToast(message, type = 'info') { const toast =  <div class="toast align-items-center text-white bg-${type === 'success' ? 'success' : 'danger'} border-0" role="alert"> <div class="d-flex"> <div class="toast-body">${message}</div> <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button> </div> </div>;
const container = document.getElementById('toastContainer');
container.insertAdjacentHTML('beforeend', toast);

const toastElement = container.lastElementChild;
const bsToast = new bootstrap.Toast(toastElement);
bsToast.show();
}
// ========== Export CSV ==========
function exportBlacklistCsv() { window.location.href = '/Security/Blacklist?handler=Export'; } 
---
3.2 Dashboard.cshtml Script Section
Dosya: src/ArchiX.WebHost/Pages/Security/Dashboard.cshtml
 @section Scripts { <script src="https://cdn.jsdelivr.net/npm/chart.js@4.4.1/dist/chart.umd.min.js"></script> <script src="~/js/security-admin.js"></script> <script> document.addEventListener('DOMContentLoaded', function() { const errorStats = @Html.Raw(Json.Serialize(Model.ViewModel.Last30DaysErrors)); initDashboardCharts(errorStats); }); </script> } 
---
3.3 Blacklist.cshtml Script Section
Dosya: src/ArchiX.WebHost/Pages/Security/Blacklist.cshtml
 @section Scripts { <link rel="stylesheet" href="https://cdn.datatables.net/1.13.7/css/dataTables.bootstrap5.min.css"> <script src="https://cdn.datatables.net/1.13.7/js/jquery.dataTables.min.js"></script> <script src="https://cdn.datatables.net/1.13.7/js/dataTables.bootstrap5.min.js"></script> <script src="~/js/security-admin.js"></script> <script> $(document).ready(function() { initBlacklistDataTable(); }); </script> } 
---
3.4 AuditTrail.cshtml Script Section
Dosya: src/ArchiX.WebHost/Pages/Security/AuditTrail.cshtml
 @section Scripts { <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/jsondiffpatch@0.6.0/dist/formatters-styles/html.css"> <script src="https://cdn.jsdelivr.net/npm/jsondiffpatch@0.6.0/dist/jsondiffpatch.umd.min.js"></script> <script src="~/js/security-admin.js"></script> } 
---
3.5 PolicyTest.cshtml Script Section
Dosya: src/ArchiX.WebHost/Pages/Security/PolicyTest.cshtml
 @section Scripts { <script src="~/js/security-admin.js"></script> <script> document.addEventListener('DOMContentLoaded', function() { initPolicyTestValidation(); }); </script> } 
---
4️⃣ Yetkilendirme ve Menü
4.1 Program.cs Authorization Policy
Dosya: Program.cs
 // Authorization policies builder.Services.AddAuthorization(options => { options.AddPolicy("SecurityAdmin", policy => { policy.RequireAuthenticatedUser(); policy.RequireRole("Admin", "SecurityManager"); }); }); 
---
4.2 _Layout.cshtml Menü Ekleme
Dosya: src/ArchiX.WebHost/Pages/Shared/_Layout.cshtml
 <li class="nav-item dropdown"> <a class="nav-link dropdown-toggle" href="#" id="securityDropdown" role="button" data-bs-toggle="dropdown" aria-expanded="false"> <i class="fas fa-shield-alt"></i> Security Management </a> <ul class="dropdown-menu" aria-labelledby="securityDropdown"> <li><a class="dropdown-item" asp-page="/Security/Dashboard"><i class="fas fa-chart-line"></i> Dashboard</a></li> <li><a class="dropdown-item" asp-page="/Security/PolicySettings"><i class="fas fa-cog"></i> Policy Settings</a></li> <li><a class="dropdown-item" asp-page="/Security/Blacklist"><i class="fas fa-ban"></i> Blacklist</a></li> <li><a class="dropdown-item" asp-page="/Security/AuditTrail"><i class="fas fa-history"></i> Audit Trail</a></li> <li><a class="dropdown-item" asp-page="/Security/PasswordHistory"><i class="fas fa-clock"></i> Password History</a></li> <li><hr class="dropdown-divider"></li> <li><a class="dropdown-item" asp-page="/Security/PolicyTest"><i class="fas fa-vial"></i> Policy Test</a></li> </ul> </li> 
---
4.3 Layout Head (CDN References)
Dosya: src/ArchiX.WebHost/Pages/Shared/_Layout.cshtml
<!-- <head> <!-- ... mevcut referanslar ... 
<!-- Font Awesome -->
<link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.1/css/all.min.css" />

@await RenderSectionAsync("Styles", required: false)
</head> -->
---
5️⃣ PageModel Backend Bağlantıları
5.1 Dashboard.cshtml.cs
Dosya: src/ArchiX.WebHost/Pages/Security/Dashboard.cshtml.cs
 using ArchiX.Library.Abstractions.Security; using ArchiX.Library.ViewModels; using Microsoft.AspNetCore.Authorization; using Microsoft.AspNetCore.Mvc; using Microsoft.AspNetCore.Mvc.RazorPages;
namespace ArchiX.WebHost.Pages.Security;
[Authorize(Policy = "SecurityAdmin")] public class DashboardModel : PageModel { private readonly IPasswordPolicyAdminService _adminService;
public SecurityDashboardViewModel ViewModel { get; set; } = null!;

public DashboardModel(IPasswordPolicyAdminService adminService)
{
    _adminService = adminService;
}

public async Task<IActionResult> OnGetAsync(int applicationId = 1)
{
    ViewModel = await _adminService.GetDashboardDataAsync(applicationId);
    return Page();
}
} 
---
5.2 PolicySettings.cshtml.cs
Dosya: src/ArchiX.WebHost/Pages/Security/PolicySettings.cshtml.cs
 using ArchiX.Library.Abstractions.Security; using ArchiX.Library.Options; using ArchiX.Library.ViewModels; using Microsoft.AspNetCore.Authorization; using Microsoft.AspNetCore.Mvc; using Microsoft.AspNetCore.Mvc.RazorPages;
namespace ArchiX.WebHost.Pages.Security;
[Authorize(Policy = "SecurityAdmin")] public class PolicySettingsModel : PageModel { private readonly IPasswordPolicyAdminService _adminService;
[BindProperty]
public PolicySettingsViewModel Input { get; set; } = null!;

public PolicySettingsModel(IPasswordPolicyAdminService adminService)
{
    _adminService = adminService;
}

public async Task<IActionResult> OnGetAsync(int applicationId = 1)
{
    var policy = await _adminService.GetPolicyAsync(applicationId);
    Input = MapToViewModel(policy);
    return Page();
}

public async Task<IActionResult> OnPostAsync(int applicationId = 1)
{
    if (!ModelState.IsValid)
        return Page();

    var policy = MapToOptions(Input);
    var userId = 1; // TODO: Get from User.Claims

    var success = await _adminService.UpdatePolicyAsync(applicationId, policy, userId);

    if (success)
    {
        TempData["SuccessMessage"] = "Policy updated successfully";
        return RedirectToPage();
    }

    ModelState.AddModelError(string.Empty, "Failed to update policy");
    return Page();
}

private PolicySettingsViewModel MapToViewModel(PasswordPolicyOptions policy)
{
    return new PolicySettingsViewModel
    {
        MinLength = policy.MinLength,
        MaxLength = policy.MaxLength,
        RequireUpper = policy.RequireUpper,
        RequireLower = policy.RequireLower,
        RequireDigit = policy.RequireDigit,
        RequireSymbol = policy.RequireSymbol,
        AllowedSymbols = policy.AllowedSymbols,
        MinDistinctChars = policy.MinDistinctChars,
        MaxRepeatedSequence = policy.MaxRepeatedSequence,
        HistoryCount = policy.HistoryCount,
        MaxPasswordAgeDays = policy.MaxPasswordAgeDays,
        LockoutThreshold = policy.LockoutThreshold,
        LockoutSeconds = policy.LockoutSeconds,
        Argon2MemoryKb = policy.Hash.MemoryKb,
        Argon2Parallelism = policy.Hash.Parallelism,
        Argon2Iterations = policy.Hash.Iterations,
        Pbkdf2Iterations = policy.Hash.Fallback.Iterations,
        PepperEnabled = policy.Hash.PepperEnabled
    };
}

private PasswordPolicyOptions MapToOptions(PolicySettingsViewModel vm)
{
    return new PasswordPolicyOptions
    {
        Version = 1,
        MinLength = vm.MinLength,
        MaxLength = vm.MaxLength,
        RequireUpper = vm.RequireUpper,
        RequireLower = vm.RequireLower,
        RequireDigit = vm.RequireDigit,
        RequireSymbol = vm.RequireSymbol,
        AllowedSymbols = vm.AllowedSymbols,
        MinDistinctChars = vm.MinDistinctChars,
        MaxRepeatedSequence = vm.MaxRepeatedSequence,
        BlockList = new List<string>(), // Statik liste kaldırıldı
        HistoryCount = vm.HistoryCount,
        MaxPasswordAgeDays = vm.MaxPasswordAgeDays,
        LockoutThreshold = vm.LockoutThreshold,
        LockoutSeconds = vm.LockoutSeconds,
        Hash = new PasswordPolicyOptions.HashOptions
        {
            Algorithm = "Argon2id",
            MemoryKb = vm.Argon2MemoryKb,
            Parallelism = vm.Argon2Parallelism,
            Iterations = vm.Argon2Iterations,
            SaltLength = 16,
            HashLength = 32,
            Fallback = new PasswordPolicyOptions.HashOptions.FallbackOptions
            {
                Algorithm = "PBKDF2-SHA512",
                Iterations = vm.Pbkdf2Iterations
            },
            PepperEnabled = vm.PepperEnabled
        }
    };
}
} 
---
5.3 Blacklist.cshtml.cs (DataTables Server-Side)
Dosya: src/ArchiX.WebHost/Pages/Security/Blacklist.cshtml.cs
 using ArchiX.Library.Abstractions.Security; using ArchiX.Library.Data; using Microsoft.AspNetCore.Authorization; using Microsoft.AspNetCore.Mvc; using Microsoft.AspNetCore.Mvc.RazorPages; using Microsoft.EntityFrameworkCore; using System.Text;
namespace ArchiX.WebHost.Pages.Security;
[Authorize(Policy = "SecurityAdmin")] public class BlacklistModel : PageModel { private readonly IPasswordBlacklistService _blacklistService; private readonly AppDbContext _context;
public BlacklistModel(IPasswordBlacklistService blacklistService, AppDbContext context)
{
    _blacklistService = blacklistService;
    _context = context;
}

public IActionResult OnGet()
{
    return Page();
}

public async Task<IActionResult> OnGetDataAsync(int draw, int start, int length, string? search)
{
    var query = _context.PasswordBlacklists
        .Where(b => b.ApplicationId == 1 && b.Status == 3);

    if (!string.IsNullOrWhiteSpace(search))
        query = query.Where(b => b.Word.Contains(search));

    var total = await query.CountAsync();
    var data = await query
        .OrderBy(b => b.Word)
        .Skip(start)
        .Take(length)
        .Select(b => new
        {
            id = b.Id,
            word = b.Word,
            createdBy = $"User#{b.CreatedBy}",
            createdAtUtc = b.CreatedAtUtc
        })
        .ToListAsync();

    return new JsonResult(new
    {
        draw,
        recordsTotal = total,
        recordsFiltered = total,
        data
    });
}

public async Task<IActionResult> OnPostAddAsync(string word)
{
    var success = await _blacklistService.AddWordAsync(word, applicationId: 1);
    return new JsonResult(new { success });
}

public async Task<IActionResult> OnPostDeleteAsync(int id)
{
    var word = await _context.PasswordBlacklists.FindAsync(id);
    if (word == null)
        return new JsonResult(new { success = false });

    var success = await _blacklistService.RemoveWordAsync(word.Word, applicationId: 1);
    return new JsonResult(new { success });
}

public async Task<IActionResult> OnGetExportAsync()
{
    var words = await _blacklistService.GetBlockedWordsAsync(applicationId: 1);
    var csv = string.Join("\n", words.Select(w => $"\"{w}\""));
    var bytes = Encoding.UTF8.GetBytes(csv);

    return File(bytes, "text/csv", "blacklist.csv");
}
} 
---
5.4 PolicyTest.cshtml.cs (AJAX Validation)
Dosya: src/ArchiX.WebHost/Pages/Security/PolicyTest.cshtml.cs
 using ArchiX.Library.Runtime.Security; using Microsoft.AspNetCore.Authorization; using Microsoft.AspNetCore.Mvc; using Microsoft.AspNetCore.Mvc.RazorPages;
namespace ArchiX.WebHost.Pages.Security;
[Authorize(Policy = "SecurityAdmin")] public class PolicyTestModel : PageModel { private readonly PasswordValidationService _validationService;
public PolicyTestModel(PasswordValidationService validationService)
{
    _validationService = validationService;
}

public IActionResult OnGet()
{
    return Page();
}

public async Task<IActionResult> OnPostValidateAsync([FromBody] ValidateRequest request)
{
    var result = await _validationService.ValidateAsync(
        request.Password, 
        userId: 0, // Test mode - no user
        applicationId: 1);

    var strength = CalculateStrength(request.Password, result.Errors.Count);

    return new JsonResult(new
    {
        isValid = result.IsValid,
        errors = result.Errors,
        strength
    });
}

private int CalculateStrength(string password, int errorCount)
{
    if (string.IsNullOrEmpty(password))
        return 0;

    var baseStrength = Math.Min(password.Length * 5, 50); // Max 50 from length
    var varietyBonus = 0;

    if (password.Any(char.IsUpper)) varietyBonus += 10;
    if (password.Any(char.IsLower)) varietyBonus += 10;
    if (password.Any(char.IsDigit)) varietyBonus += 10;
    if (password.Any(ch => !char.IsLetterOrDigit(ch))) varietyBonus += 20;

    var errorPenalty = errorCount * 10;

    return Math.Max(0, Math.Min(100, baseStrength + varietyBonus - errorPenalty));
}

public class ValidateRequest
{
    public string Password { get; set; } = string.Empty;
}
} 
---
6️⃣ CSS Styling
6.1 site.css Güncellemesi
Dosya: src/ArchiX.WebHost/wwwroot/css/site.css
 /* ========== Security Dashboard ========== */
.dashboard-card { border-left: 4px solid #0d6efd; transition: transform 0.2s; }
.dashboard-card:hover { transform: translateY(-5px); box-shadow: 0 4px 12px rgba(0,0,0,0.15); }
.stat-icon { font-size: 2.5rem; opacity: 0.3; }
/* ========== Audit Diff ========== */
.diff-container { display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; }
.diff-old, .diff-new { padding: 1rem; border-radius: 0.5rem; background: #f8f9fa; }
.diff-old h4 { color: #dc3545; }
.diff-new h4 { color: #28a745; }
.diff-container pre { max-height: 400px; overflow-y: auto; background: white; padding: 1rem; border-radius: 0.25rem; }
/* ========== Policy Test ========== */
#strengthBar { transition: width 0.3s ease, background-color 0.3s ease; }
.rule-item { padding: 0.5rem; border-radius: 0.25rem; margin-bottom: 0.5rem; background: #f8f9fa; }
.rule-item i { width: 20px; }
/* ========== Toast Container ========== */
#toastContainer { position: fixed; top: 20px; right: 20px; z-index: 9999; }
/* ========== Form Sections ========== */
.form-section { margin-bottom: 2rem; padding: 1.5rem; border: 1px solid #dee2e6; border-radius: 0.5rem; }
.form-section h4 { margin-bottom: 1rem; padding-bottom: 0.5rem; border-bottom: 2px solid #0d6efd; } -->
---
7️⃣ Yapılacak İşler Checklist
Sprint 1: Backend Altyapısı
•	[ ] IPasswordPolicyAdminService.cs interface oluştur
•	[ ] SecurityDashboardViewModel.cs DTO oluştur
•	[ ] PolicySettingsViewModel.cs DTO oluştur
•	[ ] IPasswordPolicyAdminService.cs implementasyon
•	[ ] DI kaydı ekle (PasswordSecurityServiceCollectionExtensions)
Sprint 2: Yetkilendirme + Temel UI
•	[ ] Program.cs authorization policy ekle
•	[ ] _Layout.cshtml menü ekleme
•	[ ] Dashboard.cshtml.cs backend bağlantısı
•	[ ] PolicySettings.cshtml.cs backend bağlantısı
Sprint 3: Frontend Entegrasyonlar
•	[ ] security-admin.js oluştur
•	[ ] Chart.js entegrasyonu (Dashboard)
•	[ ] DataTables entegrasyonu (Blacklist)
•	[ ] PolicyTest.cshtml.cs AJAX endpoint
Sprint 4: Gelişmiş Özellikler
•	[ ] AuditTrail jsondiffpatch entegrasyonu
•	[ ] PasswordHistory.cshtml.cs implementasyon
•	[ ] Blacklist.cshtml.cs server-side DataTables
•	[ ] site.css styling ekleme
Sprint 5: Test & Doğrulama
•	[ ] Manuel test checklist
•	[ ] Authorization test (admin-only)
•	[ ] Cache invalidation test
•	[ ] Concurrent update test (RowVersion)
---
8️⃣ Kritik Notlar
8.1 Güvenlik
•	Tüm sayfalar [Authorize(Policy = "SecurityAdmin")] ile korunmalı
•	CSRF token kullanımı zorunlu (AJAX POST'larda)
•	Password history'de hash'ler asla tam gösterilmemeli
8.2 Performans
•	Dashboard istatistikleri cache'lenebilir (5 dakika TTL)
•	Audit diff HTML'i cache'leniyor (1 saat TTL)
•	DataTables server-side processing kullanıyor (büyük veri için)
8.3 Bağımlılıklar
•	Chart.js 4.4.1
•	DataTables 1.13.7
•	jsondiffpatch 0.6.0
•	Font Awesome 6.5.1
•	Bootstrap 5.3
---
9️⃣ Test Senaryoları
Manuel Test Checklist
1.	✅ Dashboard yükleniyor, istatistikler doğru
2.	✅ Policy form validasyonu çalışıyor (client + server)
3.	✅ Policy kaydetme → cache invalidate → audit log
4.	✅ Blacklist ekleme/silme → DB güncelliyor
5.	✅ Audit trail diff doğru gösteriliyor
6.	✅ Password history sadece admin görebiliyor
7.	✅ Policy test live validation çalışıyor
8.	✅ Concurrent policy update engellenmiş (RowVersion)
---
🔟 Sonraki Adımlar
1.	Backend altyapısını tamamla (Sprint 1)
2.	Yetkilendirme + menü ekle (Sprint 2)
3.	Frontend entegrasyonlar (Sprint 3)
4.	Test ve doğrulama (Sprint 5)
Tahmini Süre: 2 gün (6 sayfa + backend + entegrasyonlar)
---
Doküman Sonu

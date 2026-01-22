using System.Text.Json;
using ArchiX.Library.Abstractions.Security;
using ArchiX.Library.Context;
using ArchiX.Library.Entities;
using ArchiX.Library.Formatting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ArchiX.Library.Runtime.Security;

/// <summary>
/// Birden fazla ApplicationId için başlangıç PasswordPolicy seed stratejisi (PK-01).
/// </summary>
public static class PasswordPolicyMultiAppSeed
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web) { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// Belirtilen ApplicationId listesi için varsayılan PasswordPolicy parametrelerini oluşturur (idempotent).
    /// </summary>
    public static async Task EnsureForApplicationsAsync(
        AppDbContext db,
        ILogger logger,
        IEnumerable<int> applicationIds,
        CancellationToken ct = default)
    {
        foreach (var appId in applicationIds)
        {
            var param = await db.Parameters
                .AsNoTracking()
                .Include(p => p.Applications)
                .FirstOrDefaultAsync(p => p.Group == "Security" && p.Key == "PasswordPolicy", ct)
                .ConfigureAwait(false);

            var appValue = param?.Applications.FirstOrDefault(a => a.ApplicationId == appId);

            if (appValue != null)
            {
                logger.LogInformation(
                    "[PasswordPolicy] AppId={AppId} için parametre zaten mevcut (ParamId={Id}).", appId, param!.Id);
                continue;
            }

            var defaultModel = new PasswordPolicyOptions();
            var raw = JsonSerializer.Serialize(defaultModel, JsonOpts);
            var minified = JsonTextFormatter.Minify(raw);

            // Parametre tanımı yoksa oluştur
            if (param == null)
            {
                param = new Parameter
                {
                    Group = "Security",
                    Key = "PasswordPolicy",
                    ParameterDataTypeId = 15,
                    Description = "Varsayılan parola politikası (multi-app seed)",
                    StatusId = 3,
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
                ApplicationId = appId,
                Value = minified,
                StatusId = 3,
                CreatedBy = 0,
                LastStatusBy = 0,
                IsProtected = false,
                RowId = Guid.NewGuid(),
                CreatedAt = DateTimeOffset.UtcNow
            };
            db.ParameterApplications.Add(paramApp);
            await db.SaveChangesAsync(ct).ConfigureAwait(false);

            logger.LogInformation(
                "[PasswordPolicy] AppId={AppId} için parametre oluşturuldu (ParamId={Id}).", appId, param.Id);
        }
    }
}

using System.Text.Json;
using ArchiX.Library.Abstractions.Security;
using ArchiX.Library.Context;
using ArchiX.Library.Entities;
using ArchiX.Library.Formatting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ArchiX.Library.Runtime.Security;

/// <summary>
/// Birden fazla ApplicationId için baþlangýç PasswordPolicy seed stratejisi (PK-01).
/// </summary>
public static class PasswordPolicyMultiAppSeed
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web) { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// Belirtilen ApplicationId listesi için varsayýlan PasswordPolicy parametrelerini oluþturur (idempotent).
    /// </summary>
    public static async Task EnsureForApplicationsAsync(
        AppDbContext db,
        ILogger logger,
        IEnumerable<int> applicationIds,
        CancellationToken ct = default)
    {
        foreach (var appId in applicationIds)
        {
            var existing = await db.Parameters
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ApplicationId == appId 
                                       && p.Group == "Security" 
                                       && p.Key == "PasswordPolicy", ct)
                .ConfigureAwait(false);

            if (existing != null)
            {
                logger.LogInformation(
                    "[PasswordPolicy] AppId={AppId} için parametre zaten mevcut (Id={Id}).", appId, existing.Id);
                continue;
            }

            var defaultModel = new PasswordPolicyOptions();
            var raw = JsonSerializer.Serialize(defaultModel, JsonOpts);
            var minified = JsonTextFormatter.Minify(raw);

            var param = new Parameter
            {
                ApplicationId = appId,
                Group = "Security",
                Key = "PasswordPolicy",
                ParameterDataTypeId = 15,
                Value = minified,
                Description = $"Varsayýlan parola politikasý (multi-app seed, AppId={appId})",
                StatusId = 3,
                CreatedBy = 0
            };

            db.Parameters.Add(param);
            await db.SaveChangesAsync(ct).ConfigureAwait(false);

            logger.LogInformation(
                "[PasswordPolicy] AppId={AppId} için parametre oluþturuldu (Id={Id}).", appId, param.Id);
        }
    }
}

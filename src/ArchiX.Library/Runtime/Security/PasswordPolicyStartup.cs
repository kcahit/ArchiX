using System.Text.Json;

using ArchiX.Library.Abstractions.Security;
using ArchiX.Library.Context;
using ArchiX.Library.Entities;
using ArchiX.Library.Formatting;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ArchiX.Library.Runtime.Security;

/// <summary>
/// Uygulama açýlýþýnda parola politikasý parametresini yoksa ekler (PK-02) ve pepper uyarýsýný loglar (PK-08).
/// </summary>
public static class PasswordPolicyStartup
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web) { PropertyNameCaseInsensitive = true };

    public static async Task EnsureSeedAndWarningsAsync(IServiceProvider services, int applicationId = 1, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var logger = scope.ServiceProvider.GetService<ILoggerFactory>()?.CreateLogger("PasswordPolicyStartup");
        var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
        await using var db = await dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);

        var entity = await db.Parameters.AsNoTracking()
            .FirstOrDefaultAsync(x => x.ApplicationId == applicationId && x.Group == "Security" && x.Key == "PasswordPolicy", ct)
            .ConfigureAwait(false);

        if (entity is null)
        {
            // Varsayýlan modelden JSON üret.
            var defaultModel = new PasswordPolicyOptions();
            var raw = JsonSerializer.Serialize(defaultModel, JsonOpts);
            var minified = JsonTextFormatter.Minify(raw);

            var param = new Parameter
            {
                ApplicationId = applicationId,
                Group = "Security",
                Key = "PasswordPolicy",
                ParameterDataTypeId = 15,
                Value = minified,
                Description = "Varsayýlan parola politikasý (startup seed)",
                StatusId = 3,
                CreatedBy = 0
            };
            db.Parameters.Add(param);
            await db.SaveChangesAsync(ct).ConfigureAwait(false);
            logger?.LogInformation("PasswordPolicy parametresi eksikti, varsayýlan oluþturuldu (AppId={AppId}).", applicationId);
        }

        // PepperEnabled uyarýsý (PK-08)
        var provider = scope.ServiceProvider.GetService<IPasswordPolicyProvider>();
        if (provider is not null)
        {
            var policy = await provider.GetAsync(applicationId, ct).ConfigureAwait(false);
            if (policy.Hash.PepperEnabled && string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ARCHIX_PEPPER")))
            {
                logger?.LogWarning("PasswordPolicy 'pepperEnabled=true' ancak 'ARCHIX_PEPPER' ortam deðiþkeni tanýmlý deðil. Güvenlik için bir pepper deðeri ayarlayýn.");
            }
        }
    }
}

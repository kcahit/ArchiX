using System.Text.Json;
using ArchiX.Library.Context;
using ArchiX.Library.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ArchiX.Library.Runtime.Connections;

public static class ConnectionStringsStartup
{
    private const int GlobalApplicationId = 1;
    private const string Group = "ConnectionStrings";
    private const string Key = "ConnectionStrings";

    private static readonly JsonSerializerOptions JsonIndented = new() { WriteIndented = true };

    public static async Task EnsureSeedAsync(IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var logger = scope.ServiceProvider.GetService<ILoggerFactory>()?.CreateLogger("ConnectionStringsStartup");

        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
               ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
               ?? "Production";

        if (!env.Equals("Development", StringComparison.OrdinalIgnoreCase))
        {
            logger?.LogInformation("Skipping ConnectionStrings seed (env={Env}).", env);
            return;
        }

        var shouldRun = Environment.GetEnvironmentVariable("ARCHIX_DB_ENABLE_CONNECTIONSTRINGS_SEED");
        if (!string.Equals(shouldRun, "true", StringComparison.OrdinalIgnoreCase))
        {
            logger?.LogInformation("Skipping ConnectionStrings seed. Set ARCHIX_DB_ENABLE_CONNECTIONSTRINGS_SEED=true to enable.");
            return;
        }

        await using var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var param = await db.Parameters
            .Include(p => p.Applications)
            .FirstOrDefaultAsync(p => p.Group == Group && p.Key == Key, ct)
            .ConfigureAwait(false);

        var appValue = param?.Applications.FirstOrDefault(a => a.ApplicationId == GlobalApplicationId);

        if (appValue != null)
        {
            logger?.LogInformation("ConnectionStrings parameter already exists.");
            return;
        }

        var json = JsonSerializer.Serialize(
            new
            {
                Demo = new
                {
                    Provider = "SqlServer",
                    Server = "(local)",
                    Database = "master",
                    Auth = "SqlLogin",
                    User = "sa",
                    PasswordRef = "ENV:ARCHIX_DB_DEMO_PASSWORD",
                    Encrypt = true,
                    TrustServerCertificate = true
                }
            },
            JsonIndented);

        // Parametre tanımı yoksa oluştur
        if (param == null)
        {
            param = new Parameter
            {
                Group = Group,
                Key = Key,
                ParameterDataTypeId = 15,
                Description = "Tenant DB connection profiles (alias -> profile JSON)",
                Value = json,
                StatusId = BaseEntity.ApprovedStatusId,
                CreatedBy = 0,
                LastStatusBy = 0,
                IsProtected = false,
                RowId = Guid.NewGuid(),
                CreatedAt = DateTimeOffset.UtcNow
            };
            db.Parameters.Add(param);
            await db.SaveChangesAsync(ct).ConfigureAwait(false);
            
            // SaveChanges sonrası Id oluştu, ama EF tracking'de olan entity'yi kullanmak daha güvenli
            // Eğer tracking'de değilse tekrar yükleyelim
            if (param.Id == 0)
            {
                param = await db.Parameters
                    .FirstOrDefaultAsync(p => p.Group == Group && p.Key == Key, ct)
                    .ConfigureAwait(false);
            }
        }

        // Değer ekle
        db.ParameterApplications.Add(new ParameterApplication
        {
            ParameterId = param!.Id,
            ApplicationId = GlobalApplicationId,
            Value = json,
            StatusId = BaseEntity.ApprovedStatusId,
            CreatedBy = 0,
            LastStatusBy = 0,
            IsProtected = false,
            RowId = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow
        });

        await db.SaveChangesAsync(ct).ConfigureAwait(false);
        logger?.LogWarning("Seeded ConnectionStrings parameter (Development only). Review and update before use.");
    }
}

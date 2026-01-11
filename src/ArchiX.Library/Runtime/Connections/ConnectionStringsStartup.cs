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

        var exists = await db.Parameters
            .AnyAsync(p => p.ApplicationId == GlobalApplicationId && p.Group == Group && p.Key == Key, ct)
            .ConfigureAwait(false);

        if (exists)
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

        db.Parameters.Add(new Parameter
        {
            ApplicationId = GlobalApplicationId,
            Group = Group,
            Key = Key,
            ParameterDataTypeId = 15,
            Description = "Tenant DB connection profiles (alias -> profile JSON)",
            Value = json,
            Template = json,
            StatusId = BaseEntity.ApprovedStatusId,
            CreatedBy = 0,
            LastStatusBy = 0,
            IsProtected = false
        });

        await db.SaveChangesAsync(ct).ConfigureAwait(false);
        logger?.LogWarning("Seeded ConnectionStrings parameter (Development only). Review and update before use.");
    }
}

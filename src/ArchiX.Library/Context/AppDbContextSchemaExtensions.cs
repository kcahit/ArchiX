using Microsoft.Extensions.DependencyInjection;

namespace ArchiX.Library.Context
{
    public static class AppDbContextSchemaExtensions
    {
        // Call this once at startup to create tables from the model (no migrations).
        public static async Task EnsureSchemaCreatedAsync(this IServiceProvider services, CancellationToken ct = default)
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Creates the database and schema if they do not exist, based on the current model.
            await db.Database.EnsureCreatedAsync(ct);

            // Seed core data (statuses etc.) and bind IDs.
            await db.EnsureCoreSeedsAndBindAsync(ct);
        }
    }
}
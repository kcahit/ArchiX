#pragma warning disable CS1591
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ArchiX.Library.Context
{
    public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var connection =
                GetArg(args, "--connection")
                ?? LoadFromConfiguration();

            if (string.IsNullOrWhiteSpace(connection))
                throw new InvalidOperationException(
                    "Bağlantı dizesi yok. '--connection' verin veya ConnectionStrings:ArchiXDb/Default sağlayın.");

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(connection, o => o.EnableRetryOnFailure())
                .Options;

            return new AppDbContext(options);
        }

        private static string? LoadFromConfiguration()
        {
            var cfg = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddJsonFile("appsettings.Production.json", optional: true, reloadOnChange: false)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            return cfg.GetConnectionString("ArchiXDb") ?? cfg.GetConnectionString("Default");
        }

        private static string? GetArg(string[] args, string key)
        {
            if (args == null || args.Length == 0) return null;
            for (int i = 0; i < args.Length - 1; i++)
                if (string.Equals(args[i], key, StringComparison.OrdinalIgnoreCase))
                    return args[i + 1];
            return null;
        }
    }
}

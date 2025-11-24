using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
namespace ArchiX.Library.Context
{ /// <summary> /// Design-time EF Core context factory. /// Öncelik: --connection arg > ARCHIX_DB_CONNECTION env > appsettings (yukarı arama / Web/Host). /// </summary>
    public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var connection = GetArg(args, "--connection") ?? Environment.GetEnvironmentVariable("ARCHIX_DB_CONNECTION") ?? LoadFromConfiguration();
            if (string.IsNullOrWhiteSpace(connection))
                throw new InvalidOperationException("Bağlantı dizesi yok. '--connection' verin, ARCHIX_DB_CONNECTION ortam değişkenini ayarlayın veya appsettings.* içine ConnectionStrings:ArchiXDb ekleyin.");

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(connection, o => o.EnableRetryOnFailure())
                .Options;

            return new AppDbContext(options);
        }

        private static string? LoadFromConfiguration()
        {
            var basePath = ResolveConfigBasePath();
            if (basePath == null) return null;

            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                      ?? Environment.GetEnvironmentVariable("DOTNET_ENV")
                      ?? "Development";

            var cfg = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            var cs = cfg.GetConnectionString("ArchiXDb") ?? cfg.GetConnectionString("Default");
            return string.IsNullOrWhiteSpace(cs) ? null : cs.Trim();
        }

        private static string? ResolveConfigBasePath()
        {
            var cwd = Directory.GetCurrentDirectory();
            if (HasAnySettings(cwd)) return cwd;

            var dir = new DirectoryInfo(cwd);
            while (dir != null && !HasAnySettings(dir.FullName))
                dir = dir.Parent;
            if (dir != null && HasAnySettings(dir.FullName)) return dir.FullName;

            var slnRoot = FindSolutionRoot(cwd);
            if (slnRoot != null)
            {
                var webPath = Path.Combine(slnRoot, "src", "ArchiX.Library.Web");
                if (Directory.Exists(webPath) && HasAnySettings(webPath)) return webPath;

                var hostPath = Path.Combine(slnRoot, "src", "ArchiX.WebHost");
                if (Directory.Exists(hostPath) && HasAnySettings(hostPath)) return hostPath;
            }
            return null;
        }

        private static bool HasAnySettings(string path)
        {
            try
            {
                if (File.Exists(Path.Combine(path, "appsettings.json"))) return true;
                return Directory.GetFiles(path, "appsettings.*.json").Length > 0;
            }
            catch { return false; }
        }

        private static string? FindSolutionRoot(string start)
        {
            var dir = new DirectoryInfo(start);
            while (dir != null)
            {
                if (Directory.GetFiles(dir.FullName, "*.sln").Length > 0)
                    return dir.FullName;
                dir = dir.Parent;
            }
            return null;
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

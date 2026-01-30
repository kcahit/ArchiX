using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ArchiX.WebHostDLL.Data
{
    public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var conn = config.GetConnectionString("ApplicationDb") ?? "Server=(localdb)\\MSSQLLocalDB;Database=ArchiXWebHostDLL;Trusted_Connection=True;TrustServerCertificate=True";

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(conn, sql => sql.MigrationsAssembly(typeof(ApplicationDbContextFactory).Assembly.FullName));

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}

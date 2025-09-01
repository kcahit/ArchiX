using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ArchiX.Library.Context
{
    /// <summary>
    /// EF Core için tasarım zamanında DbContext oluşturmayı sağlayan fabrika sınıfı.
    /// Migration ve design-time işlemler için kullanılır.
    /// </summary>
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        /// <summary>
        /// Tasarım zamanında DbContext oluşturur.
        /// </summary>
        /// <param name="args">Komut satırı argümanları.</param>
        /// <returns>Yeni AppDbContext örneği döner.</returns>
        public AppDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var connectionString = configuration.GetConnectionString("ArchiXDb");

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}

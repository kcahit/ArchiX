using ArchiX.Library.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArchiX.WebHostDLL.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<Menu> Menus => Set<Menu>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.UseCollation("Latin1_General_100_CI_AS_SC_UTF8");

            modelBuilder.Entity<Menu>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Title).IsRequired().HasMaxLength(200);
                e.Property(x => x.Url).HasMaxLength(300);
                e.Property(x => x.Icon).HasMaxLength(100);
                e.HasIndex(x => new { x.ApplicationId, x.SortOrder });
            });

            Seed(modelBuilder);
        }

        private static void Seed(ModelBuilder modelBuilder)
        {
            // Minimal demo seed for ApplicationId=2
            modelBuilder.Entity<Menu>().HasData(
                new Menu { Id = 1, ApplicationId = 2, Title = "Dashboard", Url = "/Dashboard", SortOrder = 1, StatusId = BaseEntity.ApprovedStatusId },
                new Menu { Id = 2, ApplicationId = 2, Title = "Tanımlar", Url = "/Definitions", SortOrder = 2, StatusId = BaseEntity.ApprovedStatusId }
            );
        }
    }
}

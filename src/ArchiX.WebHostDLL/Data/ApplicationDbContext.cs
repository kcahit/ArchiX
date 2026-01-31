using ArchiX.Library.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArchiX.WebHostDLL.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<Application> Applications => Set<Application>();
        public DbSet<Menu> Menus => Set<Menu>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.UseCollation("Latin1_General_100_CI_AS_SC_UTF8");

            // Applications ve Menus tabloları Library tarafından yönetiliyor
            // Bu context sadece ApplicationId=2 için seed data ekleyecek
            modelBuilder.Entity<Application>(e =>
            {
                e.ToTable("Applications", t => t.ExcludeFromMigrations());
            });

            modelBuilder.Entity<Menu>(e =>
            {
                e.ToTable("Menus", t => t.ExcludeFromMigrations());
            });

            Seed(modelBuilder);
        }

        private static void Seed(ModelBuilder modelBuilder)
        {
            // Application seed: ID=2 (WebHostDLL) - ID=1 zaten Library tarafından eklendi
            modelBuilder.Entity<Application>().HasData(
                new Application
                {
                    Id = 2,
                    Code = "WebHostDLL",
                    Name = "WebHostDLL",
                    Description = "WebHostDLL customer application",
                    DefaultCulture = "tr-TR",
                    TimeZoneId = "Europe/Istanbul",
                    ConfigVersion = 1,
                    StatusId = BaseEntity.ApprovedStatusId
                }
            );

            // Menu seeds for ApplicationId=2
            modelBuilder.Entity<Menu>().HasData(
                new Menu { Id = 1, ApplicationId = 2, Title = "Dashboard", Url = "/Dashboard", SortOrder = 1, StatusId = BaseEntity.ApprovedStatusId },
                new Menu { Id = 2, ApplicationId = 2, Title = "Tanımlar", Url = "/Definitions", SortOrder = 2, StatusId = BaseEntity.ApprovedStatusId }
            );
        }
    }
}

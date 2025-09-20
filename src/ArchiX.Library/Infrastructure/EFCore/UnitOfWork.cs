using ArchiX.Library.Context;



namespace ArchiX.Library.Infrastructure.EfCore
{
    /// <summary>
    /// Unit of Work implementasyonu.
    /// Birden fazla repository işlemini tek transaction altında birleştirir
    /// ve değişiklikleri veritabanına kaydeder.
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        /// <summary>
        /// UnitOfWork kurucu metodu.
        /// </summary>
        /// <param name="context">EF Core AppDbContext örneği.</param>
        public UnitOfWork(AppDbContext context) => _context = context;

        /// <summary>
        /// Yapılan tüm değişiklikleri veritabanına asenkron olarak kaydeder.
        /// </summary>
        /// <returns>Kaydedilen kayıt sayısı.</returns>
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}

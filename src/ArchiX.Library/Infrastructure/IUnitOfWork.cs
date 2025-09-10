namespace ArchiX.Library.Infrastructure
{
    /// <summary>
    /// Unit of Work desenini temsil eden arayüz.
    /// Repository işlemlerini tek transaction altında birleştirmeyi sağlar.
    /// </summary>
    public interface IUnitOfWork
    {
        /// <summary>
        /// Yapılan tüm değişiklikleri veritabanına asenkron olarak kaydeder.
        /// </summary>
        /// <returns>Kaydedilen kayıt sayısı.</returns>
        Task<int> SaveChangesAsync();
    }
}

// File: src/ArchiX.WebApplication/Abstractions/Persistence/IUnitOfWork.cs
namespace ArchiX.WebApplication.Abstractions
{
    /// <summary>
    /// Uygulama kapsamında işlem birimini temsil eder ve değişiklikleri kalıcılaştırır.
    /// </summary>
    public interface IUnitOfWork
    {
        /// <summary>
        /// Bellekteki değişiklikleri kalıcı depoya yazar.
        /// </summary>
        /// <param name="cancellationToken">İptal belirteci.</param>
        /// <returns>Yazılan kayıt sayısı.</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}

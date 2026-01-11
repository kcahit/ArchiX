namespace ArchiX.Library.Abstractions.Persistence
{
    /// <summary>
    /// Uygulama genelinde işlem birimi sözleşmesi (persist).
    /// </summary>
    public interface IUnitOfWork
    {
        /// <summary>Bellekteki değişiklikleri kalıcı depoya yazar.</summary>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}

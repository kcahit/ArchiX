using ArchiX.Library.Entities;

namespace ArchiX.Library.Abstractions.Persistence
{
    /// <summary>
    /// Generic repository arayüzü. Tüm entity’ler için temel CRUD işlemlerini tanımlar.
    /// </summary>
    /// <typeparam name="T">Entity tipi.</typeparam>
    public interface IRepository<T> where T : BaseEntity
    {
        /// <summary>
        /// Tüm kayıtları asenkron olarak getirir.
        /// </summary>
        /// <returns>Kayıt listesi.</returns>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// Belirtilen kimliğe sahip kaydı asenkron olarak getirir.
        /// </summary>
        /// <param name="id">Aranacak entity kimliği.</param>
        /// <returns>Entity veya null.</returns>
        Task<T?> GetByIdAsync(int id);

        /// <summary>
        /// Yeni bir kayıt ekler.
        /// </summary>
        /// <param name="entity">Eklenecek entity.</param>
        /// <param name="userId">İşlemi yapan kullanıcı kimliği.</param>
        Task AddAsync(T entity, int userId);

        /// <summary>
        /// Var olan bir kaydı günceller.
        /// </summary>
        /// <param name="entity">Güncellenecek entity.</param>
        /// <param name="userId">İşlemi yapan kullanıcı kimliği.</param>
        Task UpdateAsync(T entity, int userId);

        /// <summary>
        /// Belirtilen kimliğe sahip kaydı siler (soft-delete politikası implementasyona bağlı).
        /// </summary>
        /// <param name="id">Silinecek entity kimliği.</param>
        /// <param name="userId">İşlemi yapan kullanıcı kimliği.</param>
        Task DeleteAsync(int id, int userId);
    }
}

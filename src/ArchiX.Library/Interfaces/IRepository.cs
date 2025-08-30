namespace  ArchiX.Library.Interfaces
{
    public interface IRepository<T> where T : class, IEntity
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task AddAsync(T entity, int userId);
        Task UpdateAsync(T entity, int userId);
        Task DeleteAsync(int id, int userId);
    }
}

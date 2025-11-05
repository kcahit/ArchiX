using ArchiX.Library.Abstractions.Persistence;
using ArchiX.Library.Context;
using ArchiX.Library.Entities;

using Microsoft.EntityFrameworkCore;

namespace ArchiX.Library.Infrastructure.EfCore
{
    /// <summary>
    /// Generic repository implementasyonu.
    /// EF Core üzerinden temel CRUD (Create, Read, Update, Delete) işlemlerini gerçekleştirir.
    /// </summary>
    /// <typeparam name="T">Entity tipi (IEntity implementasyonu olmalı).</typeparam>
    public class Repository<T> : IRepository<T> where T : class, IEntity
    {
        private readonly AppDbContext _context;
        private readonly DbSet<T> _dbSet;

        /// <summary>
        /// Repository kurucu metodu.
        /// </summary>
        /// <param name="context">EF Core DbContext örneği.</param>
        public Repository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        /// <summary>
        /// Tüm kayıtları asenkron olarak getirir.
        /// </summary>
        /// <returns>Kayıt listesi.</returns>
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        /// <summary>
        /// Belirtilen kimliğe sahip kaydı asenkron olarak getirir.
        /// Soft-delete edilmiş kayıtlar (BaseEntity.StatusId = 6) dışlanır.
        /// </summary>
        /// <param name="id">Aranacak entity kimliği.</param>
        /// <returns>Entity veya null.</returns>
        public async Task<T?> GetByIdAsync(int id)
        {
            IQueryable<T> query = _dbSet;

            // BaseEntity türevlerinde soft-delete’i açıkça hariç tut
            if (typeof(BaseEntity).IsAssignableFrom(typeof(T)))
            {
                query = query.Where(e =>
                    EF.Property<int>(e, nameof(BaseEntity.StatusId)) != BaseEntity.DeletedStatusId);
            }

            // Id üzerinden filtrele (LINQ → HasQueryFilter devrede kalır)
            return await query.FirstOrDefaultAsync(e =>
                EF.Property<int>(e, nameof(BaseEntity.Id)) == id);
        }

        /// <summary>
        /// Yeni bir kayıt ekler ve BaseEntity özelliklerini günceller.
        /// </summary>
        /// <param name="entity">Eklenecek entity.</param>
        /// <param name="userId">İşlemi yapan kullanıcı kimliği.</param>
        public async Task AddAsync(T entity, int userId)
        {
            if (entity is BaseEntity be)
                be.MarkCreated(userId);

            await _dbSet.AddAsync(entity);
        }

        /// <summary>
        /// Var olan bir kaydı günceller ve BaseEntity özelliklerini günceller.
        /// </summary>
        /// <param name="entity">Güncellenecek entity.</param>
        /// <param name="userId">İşlemi yapan kullanıcı kimliği.</param>
        public async Task UpdateAsync(T entity, int userId)
        {
            if (entity is BaseEntity be)
                be.MarkUpdated(userId);

            _dbSet.Update(entity);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Belirtilen kimliğe sahip kaydı siler.
        /// BaseEntity ise soft-delete uygular; değilse fiziksel siler.
        /// </summary>
        /// <param name="id">Silinecek entity kimliği.</param>
        /// <param name="userId">İşlemi yapan kullanıcı kimliği.</param>
        public async Task DeleteAsync(int id, int userId)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity == null) return;

            if (entity is BaseEntity be)
            {
                // Soft-delete (DEL=6), fiziksel silme yok
                be.SoftDelete(userId);
                _dbSet.Update(entity);
            }
            else
            {
                _dbSet.Remove(entity);
            }
        }
    }
}

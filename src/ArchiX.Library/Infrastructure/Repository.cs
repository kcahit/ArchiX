using ArchiX.Library.Context;
using ArchiX.Library.Entities;
using ArchiX.Library.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace ArchiX.Library.Infrastructure
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
        /// </summary>
        /// <param name="id">Aranacak entity kimliği.</param>
        /// <returns>Entity veya null.</returns>
        public async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        /// <summary>
        /// Yeni bir kayıt ekler ve BaseEntity özelliklerini günceller.
        /// </summary>
        /// <param name="entity">Eklenecek entity.</param>
        /// <param name="userId">İşlemi yapan kullanıcı kimliği.</param>
        public async Task AddAsync(T entity, int userId)
        {
            if (entity is BaseEntity baseEntity)
            {
                baseEntity.CreatedAt = DateTimeOffset.UtcNow;
                baseEntity.CreatedBy = userId;
            }

            await _dbSet.AddAsync(entity);
        }

        /// <summary>
        /// Var olan bir kaydı günceller ve BaseEntity özelliklerini günceller.
        /// </summary>
        /// <param name="entity">Güncellenecek entity.</param>
        /// <param name="userId">İşlemi yapan kullanıcı kimliği.</param>
        public async Task UpdateAsync(T entity, int userId)
        {
            if (entity is BaseEntity baseEntity)
            {
                baseEntity.UpdatedAt = DateTimeOffset.UtcNow;
                baseEntity.UpdatedBy = userId;
            }

            _dbSet.Update(entity);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Belirtilen kimliğe sahip kaydı siler.
        /// Eğer entity BaseEntity türevi ise, durum alanlarını da günceller.
        /// </summary>
        /// <param name="id">Silinecek entity kimliği.</param>
        /// <param name="userId">İşlemi yapan kullanıcı kimliği.</param>
        public async Task DeleteAsync(int id, int userId)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity == null) return;

            if (entity is BaseEntity baseEntity)
            {
                baseEntity.LastStatusAt = DateTimeOffset.UtcNow;
                baseEntity.LastStatusBy = userId;
                baseEntity.StatusId = -14; // Deleted
            }

            _dbSet.Remove(entity);
        }
    }
}

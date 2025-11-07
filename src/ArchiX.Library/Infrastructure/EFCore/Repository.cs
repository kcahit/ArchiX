using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using ArchiX.Library.Abstractions.Persistence;
using ArchiX.Library.Entities;
using ArchiX.Library.Context;

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
        private readonly bool _isBaseEntity;

        // Compiled queries are created per-context to avoid cross-model execution errors
        private readonly Func<AppDbContext, int, T?> _compiledGetById_NoSoftDelete;
        private readonly Func<AppDbContext, int, T?> _compiledGetById_WithSoftDelete;

        /// <summary>
        /// Repository kurucu metodu.
        /// </summary>
        /// <param name="context">EF Core DbContext örneği.</param>
        public Repository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = context.Set<T>();
            _isBaseEntity = typeof(BaseEntity).IsAssignableFrom(typeof(T));

            // Compile queries for this context's model
            _compiledGetById_NoSoftDelete = EF.CompileQuery((AppDbContext ctx, int id) =>
                ctx.Set<T>().AsNoTracking().FirstOrDefault(e => EF.Property<int>(e, nameof(BaseEntity.Id)) == id));

            _compiledGetById_WithSoftDelete = EF.CompileQuery((AppDbContext ctx, int id) =>
                ctx.Set<T>().AsNoTracking().FirstOrDefault(e =>
                    EF.Property<int>(e, nameof(BaseEntity.Id)) == id &&
                    EF.Property<int>(e, nameof(BaseEntity.StatusId)) != BaseEntity.DeletedStatusId));
        }

        /// <summary>
        /// Tüm kayıtları asenkron olarak getirir.
        /// </summary>
        /// <returns>Kayıt listesi.</returns>
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            // Apply recommended read-time optimizations: AsNoTracking by default
            return await _dbSet.ApplyDefaultReadOptions().ToListAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Sayfalandırılmış ve projeksiyonlu sorgu.
        /// </summary>
        public async Task<List<TResult>> GetPageAsync<TResult>(System.Linq.Expressions.Expression<Func<T, TResult>> selector, int pageNumber, int pageSize, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(selector);
            if (pageNumber <1) throw new ArgumentOutOfRangeException(nameof(pageNumber));
            if (pageSize <1) throw new ArgumentOutOfRangeException(nameof(pageSize));

            var q = _dbSet
                .ApplyDefaultReadOptions()
                .Select(selector)
                .Skip((pageNumber -1) * pageSize)
                .Take(pageSize);

            return await q.ToListAsync(ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Belirtilen kimliğe sahip kaydı asenkron olarak getirir.
        /// Soft-delete edilmiş kayıtlar (BaseEntity.StatusId =6) dışlanır.
        /// </summary>
        /// <param name="id">Aranacak entity kimliği.</param>
        /// <returns>Entity veya null.</returns>
        public Task<T?> GetByIdAsync(int id)
        {
            // Use compiled sync query for hot path and return as completed task
            T? result = _isBaseEntity
                ? _compiledGetById_WithSoftDelete(_context, id)
                : _compiledGetById_NoSoftDelete(_context, id);

            return Task.FromResult(result);
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

            await _dbSet.AddAsync(entity).ConfigureAwait(false);
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
            await Task.CompletedTask.ConfigureAwait(false);
        }

        /// <summary>
        /// Belirtilen kimliğe sahip kaydı siler.
        /// BaseEntity ise soft-delete uygular; değilse fiziksel siler.
        /// </summary>
        /// <param name="id">Silinecek entity kimliği.</param>
        /// <param name="userId">İşlemi yapan kullanıcı kimliği.</param>
        public async Task DeleteAsync(int id, int userId)
        {
            var entity = await _dbSet.FindAsync(id).ConfigureAwait(false);
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

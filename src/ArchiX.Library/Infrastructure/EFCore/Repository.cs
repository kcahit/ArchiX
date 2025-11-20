using ArchiX.Library.Abstractions.Persistence;
using ArchiX.Library.Context;
using ArchiX.Library.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArchiX.Library.Infrastructure.EfCore
{
    /// <summary>
    /// Generic repository implementasyonu.
    /// EF Core üzerinden temel CRUD işlemlerini gerçekleştirir.
    /// </summary>
    /// <typeparam name="T">Entity tipi (BaseEntity türevi).</typeparam>
    public class Repository<T> : IRepository<T> where T : BaseEntity
    {
        private readonly AppDbContext _context;
        private readonly DbSet<T> _dbSet;

        // Compiled queries (per context model)
        private readonly Func<AppDbContext, int, T?> _compiledGetById_NoSoftDelete;
        private readonly Func<AppDbContext, int, T?> _compiledGetById_WithSoftDelete;

        public Repository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = context.Set<T>();

            _compiledGetById_NoSoftDelete = EF.CompileQuery(
                (AppDbContext ctx, int id) =>
                    ctx.Set<T>().AsNoTracking()
                       .FirstOrDefault(e => EF.Property<int>(e, nameof(BaseEntity.Id)) == id));

            _compiledGetById_WithSoftDelete = EF.CompileQuery(
                (AppDbContext ctx, int id) =>
                    ctx.Set<T>().AsNoTracking()
                       .FirstOrDefault(e =>
                           EF.Property<int>(e, nameof(BaseEntity.Id)) == id &&
                           EF.Property<int>(e, nameof(BaseEntity.StatusId)) != BaseEntity.DeletedStatusId));
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet
                .ApplyDefaultReadOptions()
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public async Task<List<TResult>> GetPageAsync<TResult>(
            System.Linq.Expressions.Expression<Func<T, TResult>> selector,
            int pageNumber,
            int pageSize,
            CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(selector);
            ArgumentOutOfRangeException.ThrowIfLessThan(pageNumber, 1, nameof(pageNumber));
            ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1, nameof(pageSize));

            var q = _dbSet
                .ApplyDefaultReadOptions()
                .Select(selector)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            return await q.ToListAsync(ct).ConfigureAwait(false);
        }

        public Task<T?> GetByIdAsync(int id)
            => Task.FromResult(_compiledGetById_WithSoftDelete(_context, id));

        public async Task AddAsync(T entity, int userId)
        {
            entity.MarkCreated(userId);
            await _dbSet.AddAsync(entity).ConfigureAwait(false);
        }

        public Task UpdateAsync(T entity, int userId)
        {
            entity.MarkUpdated(userId);
            _dbSet.Update(entity);
            return Task.CompletedTask;
        }

        public async Task DeleteAsync(int id, int userId)
        {
            var entity = await _dbSet.FindAsync(id).ConfigureAwait(false);
            if (entity == null) return;
            entity.SoftDelete(userId);
            _dbSet.Update(entity);
        }
    }
}

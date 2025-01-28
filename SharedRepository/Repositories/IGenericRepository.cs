using Microsoft.EntityFrameworkCore.Query;
using SharedRepository.Audit;
using System.Linq.Expressions;

namespace SharedRepository.Repositories
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        Task<TEntity> GetByIdAsync(object id);
        Task<TEntity> GetByIdAsync(Guid id);
        Task<IEnumerable<TEntity>> GetAllAsync();

        Task<IEnumerable<TEntity>> GetAllAsync(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null);

        Task<TEntity> GetByIdAsync(Guid id, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null);

        Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);
        
        Task AddAsync(TEntity entity);
        Task AddRangeAsync(IEnumerable<TEntity> entities);
        void Update(TEntity entity);
        void Remove(TEntity entity);
        void RemoveRange(IEnumerable<TEntity> entities);

        Task<int> SaveChangesAsync();

        Task<bool> CustomerExistsAsync(Guid customerId);
        public Task<(decimal Price, decimal TaxPercentage)> GetProductDetailsAsync(int productId);

        Task<IEnumerable<object>> GetTransformedAuditsAsync();

    }
}

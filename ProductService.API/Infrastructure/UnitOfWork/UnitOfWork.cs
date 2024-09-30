using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProductService.API.Infrastructure.DBContext;
using SharedRepository.Repositories;

namespace ProductService.API.Infrastructure.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ProductDbContext _context;
        private readonly Dictionary<Type, object> _repositories;

        public UnitOfWork(ProductDbContext context)
        {
            _context = context;
            _repositories = new Dictionary<Type, object>();
        }

        public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class
        {
            if (_repositories.ContainsKey(typeof(TEntity)))
            {
                return (IGenericRepository<TEntity>)_repositories[typeof(TEntity)];
            }

            var repository = new GenericRepository<TEntity>(_context);
            _repositories.Add(typeof(TEntity), repository);
            return repository;
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}

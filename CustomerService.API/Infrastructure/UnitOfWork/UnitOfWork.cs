using System;
using System.Threading.Tasks;
using CustomerService.API.Infrastructure.DBContext;
using CustomerService.API.Infrastructure.Entities;
using CustomerService.API.Infrastructure.UnitOfWork;
using SharedRepository.Repositories;

namespace CustomerService.API.Infrastructure.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly CustomerDbContext _context;
        private readonly Dictionary<Type, object> _repositories;
        public UnitOfWork(CustomerDbContext context)
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

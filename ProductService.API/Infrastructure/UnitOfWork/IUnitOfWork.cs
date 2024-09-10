using System;
using System.Threading.Tasks;
using CustomerService.API.Infrastructure.Repositories;

namespace CustomerService.API.Infrastructure.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class;
        Task<int> CompleteAsync();
    }
}

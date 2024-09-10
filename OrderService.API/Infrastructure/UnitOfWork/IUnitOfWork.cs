using System;
using System.Threading.Tasks;
using OrderService.API.Infrastructure.Repositories;

namespace OrderService.API.Infrastructure.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class;
        Task<int> CompleteAsync();
    }
}

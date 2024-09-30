using SharedRepository.Repositories;
using System;
using System.Threading.Tasks;

namespace OrderService.API.Infrastructure.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        //IGenericRepository<T> Repository<T>() where T : class;
        IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class;
        Task<int> CompleteAsync();
    }
}

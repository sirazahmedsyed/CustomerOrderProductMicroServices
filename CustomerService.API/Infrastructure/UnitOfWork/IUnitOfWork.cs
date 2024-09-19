using ProductService.API.Infrastructure.Entities;
using ProductService.API.Infrastructure.Repositories;

namespace ProductService.API.Infrastructure.UnitOfWork
{
    public interface IUnitOfWork 
    {
       // IGenericRepository<T> Repository<T>() where T : class;
        IGenericRepository<Customer> Customers { get; }
        Task<int> CompleteAsync();
        void Dispose();
    }
}

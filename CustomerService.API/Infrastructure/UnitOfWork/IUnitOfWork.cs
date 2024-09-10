using CustomerService.API.Infrastructure.Entities;
using CustomerService.API.Infrastructure.Repositories;

namespace CustomerService.API.Infrastructure.UnitOfWork
{
    public interface IUnitOfWork 
    {
       // IGenericRepository<T> Repository<T>() where T : class;
        IGenericRepository<Customer> Customers { get; }
        Task<int> CompleteAsync();
        void Dispose();
    }
}

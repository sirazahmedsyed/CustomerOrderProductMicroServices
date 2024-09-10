using CustomerService.API.Infrastructure.Repositories;
using System;
using System.Threading.Tasks;
using CustomerService.API.Infrastructure.DBContext;
using CustomerService.API.Infrastructure.Entities;
using CustomerService.API.Infrastructure.UnitOfWork;

namespace CustomerService.API.Infrastructure.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly CustomerDbContext _context;
        private IGenericRepository<Customer> _customers;
        public UnitOfWork(CustomerDbContext context)
        {
            _context = context;
        }

        public IGenericRepository<Customer> Customers => _customers ??= new GenericRepository<Customer>(_context);
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

using CustomerService.API.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.API.Infrastructure.DBContext
{
    public class CustomerDbContext : DbContext
    {
        public CustomerDbContext(DbContextOptions<CustomerDbContext> options) : base(options) { }

        public DbSet<Customer> Customers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}

using Microsoft.EntityFrameworkCore;
using ProductService.API.Infrastructure.Entities;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace ProductService.API.Infrastructure.DBContext
{
    public class ProductDbContext : DbContext
    {
        public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options) { }
        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
           
        }
    }
}

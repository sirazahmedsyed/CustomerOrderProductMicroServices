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
            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("products");
                entity.Property(e => e.ProductId).HasColumnName("product_id");
                entity.Property(e => e.Name).HasColumnName("name");
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.Price).HasColumnName("price");
                entity.Property(e => e.Stock).HasColumnName("stock");
                entity.Property(e => e.TaxPercentage).HasColumnName("tax_percentage");
            });
        }
    }
}

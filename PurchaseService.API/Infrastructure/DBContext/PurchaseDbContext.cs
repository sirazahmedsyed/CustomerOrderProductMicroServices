using Microsoft.EntityFrameworkCore;
using PurchaseService.API.Infrastructure.Entities;

namespace PurchaseService.API.Infrastructure.DBContext
{
    public class PurchaseDbContext : DbContext
    {
        public PurchaseDbContext(DbContextOptions<PurchaseDbContext> options) : base(options) { }

        public DbSet<Purchase> Purchases { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Purchase>(entity =>
            {
                entity.ToTable("purchases");
                entity.Property(e => e.PurchaseId).HasColumnName("purchase_id");
                entity.Property(e => e.PurchaseOrderNo).HasColumnName("purchase_order_no");
                entity.Property(e => e.ProductId).HasColumnName("product_id");
                entity.Property(e => e.Quantity).HasColumnName("quantity");
                entity.Property(e => e.PurchaseDate).HasColumnName("purchase_date");
                entity.Property(e => e.Supplier).HasColumnName("supplier");

            });
        }
    }
}

using Microsoft.EntityFrameworkCore;
using OrderService.API.Infrastructure.Entities;
using System.Collections.Generic;

namespace OrderService.API.Infrastructure.DBContext
{
    public class OrderDbContext : DbContext
    {
        public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Defining the relationship between Order and OrderItem
            modelBuilder.Entity<Order>()
                .HasMany(o => o.OrderItems) // One Order has many OrderItems
                .WithOne(oi => oi.Order) // Each OrderItem has one Order
                .HasForeignKey(oi => oi.OrderId) // Foreign key in OrderItem is OrderId
                .OnDelete(DeleteBehavior.Cascade); // If Order is deleted, delete its OrderItems
            
            // Order mapping
            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("orders");
                entity.Property(e => e.OrderId).HasColumnName("order_id");
                entity.Property(e => e.CustomerId).HasColumnName("customer_id");
                entity.Property(e => e.OrderDate).HasColumnName("order_date");
                entity.Property(e => e.TotalAmount).HasColumnName("total_amount");
                entity.Property(e => e.DiscountPercentage).HasColumnName("discount_percentage");
                entity.Property(e => e.DiscountedTotal).HasColumnName("discounted_total");
            });
     
            // OrderItem mapping
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.ToTable("order_items");
                entity.Property(e => e.OrderItemId).HasColumnName("order_item_id");
                entity.Property(e => e.OrderId).HasColumnName("order_id");
                entity.Property(e => e.ProductId).HasColumnName("product_id");
                entity.Property(e => e.Quantity).HasColumnName("quantity");
                entity.Property(e => e.UnitPrice).HasColumnName("unit_price");
            });
        }
    }
}

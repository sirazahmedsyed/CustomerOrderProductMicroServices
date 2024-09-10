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
        }
    }
}

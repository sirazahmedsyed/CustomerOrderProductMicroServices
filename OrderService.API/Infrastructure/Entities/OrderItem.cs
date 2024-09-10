using System;
using System.ComponentModel.DataAnnotations;

namespace OrderService.API.Infrastructure.Entities
{

    public class OrderItem
    {
        [Key]
        public Guid OrderItemId { get; set; }

        [Required]
        public Guid OrderId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        public Order Order { get; set; }
    }
}


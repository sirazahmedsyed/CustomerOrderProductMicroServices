using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace OrderService.API.Infrastructure.Entities
{
    public class Order
    {
        [Key]
        public Guid OrderId { get; set; }

        [Required]
        public Guid CustomerId { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        [Range(0, double.MaxValue)]
        public decimal TotalAmount { get; set; }

        [Range(0, 100)]
        public decimal DiscountPercentage { get; set; }

        [Range(0, double.MaxValue)]
        public decimal DiscountedTotal { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; }
    }
}

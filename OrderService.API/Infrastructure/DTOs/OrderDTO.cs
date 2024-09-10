using System.ComponentModel.DataAnnotations;

namespace OrderService.API.Infrastructure.DTOs
{
    public class OrderDTO
    {
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

        public ICollection<OrderItemDTO> OrderItems { get; set; }
    }

}


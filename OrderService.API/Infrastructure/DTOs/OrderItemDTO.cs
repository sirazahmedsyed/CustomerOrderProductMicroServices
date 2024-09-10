using System;
using System.ComponentModel.DataAnnotations;

namespace OrderService.API.Infrastructure.DTOs
{
    public class OrderItemDTO
    {
        public Guid OrderItemId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }

}

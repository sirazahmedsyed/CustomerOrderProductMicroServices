using System;
using System.ComponentModel.DataAnnotations;

namespace ProductService.API.Infrastructure.Entities
{
    public class Product
    {
        public int ProductId { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 3)]
        public string Name { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be positive.")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative.")]
        public int Stock { get; set; }

        [Range(0, 100)]
        public decimal TaxPercentage { get; set; }
    }
}

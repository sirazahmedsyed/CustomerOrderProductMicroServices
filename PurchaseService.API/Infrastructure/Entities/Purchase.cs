using System.ComponentModel.DataAnnotations;
namespace PurchaseService.API.Infrastructure.Entities
{
    public class Purchase
    {
        [Key]
        public int PurchaseId { get; set; }

        [StringLength(50)]
        public string PurchaseOrderNo { get; set; } = string.Empty;

        [Required]
        public int ProductId { get; set; }  

        [Required]
        public int Quantity { get; set; }  

        [Required]
        public DateTime PurchaseDate { get; set; }  

        [StringLength(200)]
        public string Supplier { get; set; }  

    }
}

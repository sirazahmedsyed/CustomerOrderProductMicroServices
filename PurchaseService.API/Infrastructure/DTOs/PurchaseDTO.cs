namespace PurchaseService.API.Infrastructure.DTOs
{
    public class PurchaseDTO
    {
        public int PurchaseId { get; set; }
        public string PurchaseOrderNo { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string Supplier { get; set; }
    }
}

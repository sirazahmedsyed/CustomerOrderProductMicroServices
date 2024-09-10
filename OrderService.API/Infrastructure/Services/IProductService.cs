namespace OrderService.API.Infrastructure.Services
{
    public interface IProductService
    {
        Task<bool> ProductExistsAsync(int productId);
        Task<(decimal Price, decimal TaxPercentage)> GetProductDetailsAsync(int productId);
    }
}

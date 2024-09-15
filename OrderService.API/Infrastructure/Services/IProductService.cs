namespace OrderService.API.Infrastructure.Services
{
    public interface IProductService
    {
        Task<bool> ProductExistsAsync(int productId, string bearerToken);
        Task<(decimal Price, decimal TaxPercentage)> GetProductDetailsAsync(int productId, string bearerToken);
    }
}

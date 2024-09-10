using OrderService.API.Infrastructure.Entities;
using System.Text.Json;

namespace OrderService.API.Infrastructure.Services
{
    public class ProductService : IProductService
    {
        private readonly HttpClient _httpClient;
        private const string ProductApiBaseUrl = "https://api.productservice.com/"; 

        public ProductService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> ProductExistsAsync(int productId)
        {
            var response = await _httpClient.GetAsync($"{ProductApiBaseUrl}products/{productId}");
            return response.IsSuccessStatusCode;
        }

        public async Task<(decimal Price, decimal TaxPercentage)> GetProductDetailsAsync(int productId)
        {
            var response = await _httpClient.GetAsync($"{ProductApiBaseUrl}products/{productId}");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var productDetails = JsonSerializer.Deserialize<ProductDetails>(content);
            return (productDetails.Price, productDetails.TaxPercentage);
        }
    }
}
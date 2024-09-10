using OrderService.API.Infrastructure.Entities;
using System.Text.Json;

namespace OrderService.API.Infrastructure.Services
{
    public class ProductService : IProductService
    {
        private readonly HttpClient _httpClient;
        
        private const string ProductApiBaseUrl = "https://localhost:7219/api/Products";
        
        public ProductService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> ProductExistsAsync(int productId)
        {
            var response = await _httpClient.GetAsync($"{ProductApiBaseUrl}/?Id={productId}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
            response.EnsureSuccessStatusCode();
            return true;
        }

        public async Task<(decimal Price, decimal TaxPercentage)> GetProductDetailsAsync(int productId)
        {
            var response = await _httpClient.GetAsync($"{ProductApiBaseUrl}/?Id={productId}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            using (JsonDocument document = JsonDocument.Parse(content))
            {
                JsonElement root = document.RootElement;

                foreach (JsonElement productElement in root.EnumerateArray())
                {
                    if (productElement.GetProperty("productId").GetInt32() == productId)
                    {
                        var price = productElement.GetProperty("price").GetDecimal();
                        var taxPercentage = productElement.GetProperty("taxPercentage").GetDecimal();

                        return (price, taxPercentage); 
                    }
                }
            }
            throw new Exception($"Product with ID {productId} not found.");
        }
    }
}
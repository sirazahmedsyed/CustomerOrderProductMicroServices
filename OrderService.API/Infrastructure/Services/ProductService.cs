using OrderService.API.Infrastructure.Entities;
using System.Net.Http.Headers;
using System.Text.Json;

namespace OrderService.API.Infrastructure.Services
{
    public class ProductService : IProductService
    {
        private readonly HttpClient _httpClient;
        
        private const string ProductApiBaseUrl = "http://localhost:7219/api/Products";
        
        public ProductService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> ProductExistsAsync(int productId, string bearerToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{ProductApiBaseUrl}/?Id={productId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            // Log the request details
            Console.WriteLine($"Request URL: {request.RequestUri}");
            var response = await _httpClient.SendAsync(request);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
            response.EnsureSuccessStatusCode();
            return true;
        }

        public async Task<(decimal Price, decimal TaxPercentage)> GetProductDetailsAsync(int productId, string bearerToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{ProductApiBaseUrl}/?Id={productId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            var response = await _httpClient.SendAsync(request);
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
            throw new ProductNotFoundException(productId);
        }
    }
}
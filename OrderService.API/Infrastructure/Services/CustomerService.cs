using Newtonsoft.Json;
using OrderService.API.Infrastructure.Entities;
using System.Net.Http.Headers;
using System.Text.Json;

namespace OrderService.API.Infrastructure.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly HttpClient _httpClient;
        private const string CustomerApiBaseUrl = " http://localhost:7229/api/Customers";
        public CustomerService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<bool> CustomerExistsAsync(Guid customerId, string bearerToken)
        {
            // Create the request URL with the customerId parameter
            var requestUrl = $"{CustomerApiBaseUrl}/?Id={customerId}";
            // Create the HttpRequestMessage
            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            // Add the Authorization header with the Bearer token
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            // Send the request
            var response = await _httpClient.SendAsync(request);
            // Check if the customer does not exist
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
            // Ensure the response was successful, otherwise throw an exception
            response.EnsureSuccessStatusCode();
            // Read the response content (this is important to avoid null content)
            var content = await response.Content.ReadAsStringAsync();
            // Optional: You can check or log the content here if needed
            Console.WriteLine($"Response content: {content}");
            // Parse the content (assuming the response is JSON and has a property called "Id")

            // Parse the content using JsonDocument
            using (JsonDocument document = JsonDocument.Parse(content))
            {
                JsonElement root = document.RootElement;

                foreach (JsonElement customerElement in root.EnumerateArray())
                {
                    if (customerElement.GetProperty("customerId").GetGuid() == customerId)
                    {
                       return true;
                    }
                }
            return false;
            }

        }
    }
}

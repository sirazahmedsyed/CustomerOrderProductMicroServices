using System.Net.Http.Headers;

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
            // Create the HttpRequestMessage
            var request = new HttpRequestMessage(HttpMethod.Get, $"{CustomerApiBaseUrl}/?Id={customerId}");

            // Add the Authorization header with the Bearer token
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

            // Send the request
            var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }

            response.EnsureSuccessStatusCode();
            return true;
        }
    }
}

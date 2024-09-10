namespace OrderService.API.Infrastructure.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly HttpClient _httpClient;
        private const string CustomerApiBaseUrl = " https://localhost:7229/api/Customers";
       
        public CustomerService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> CustomerExistsAsync(Guid customerId)
        {
            var response = await _httpClient.GetAsync($"{CustomerApiBaseUrl}/?Id={customerId}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
            response.EnsureSuccessStatusCode();
            return true;
        }
    }
}

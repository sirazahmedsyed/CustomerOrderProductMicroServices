namespace OrderService.API.Infrastructure.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly HttpClient _httpClient;
        //private const string CustomerApiBaseUrl = "https://api.customerservice.com/"; // Replace with actual URL
        private const string CustomerApiBaseUrl = "https://localhost:5001/";
        
        public CustomerService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> CustomerExistsAsync(Guid customerId)
        {
            var response = await _httpClient.GetAsync($"{CustomerApiBaseUrl}customers/{customerId}");
            return response.IsSuccessStatusCode;
        }
    }

}

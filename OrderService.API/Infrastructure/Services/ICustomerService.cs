namespace OrderService.API.Infrastructure.Services
{
    public interface ICustomerService
    {
       // Task<bool> CustomersExistsAsync(Guid customerId);
        Task<bool> CustomerExistsAsync(Guid customerId, string bearerToken);
       // Task CustomerExistsAsync(Guid customerId, string bearerToken);
    }
}

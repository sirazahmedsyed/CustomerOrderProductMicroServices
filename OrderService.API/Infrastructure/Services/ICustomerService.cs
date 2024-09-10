namespace OrderService.API.Infrastructure.Services
{
    public interface ICustomerService
    {
        Task<bool> CustomerExistsAsync(Guid customerId);
    }
}

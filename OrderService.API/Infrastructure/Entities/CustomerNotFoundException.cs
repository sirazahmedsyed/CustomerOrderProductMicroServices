namespace OrderService.API.Infrastructure.Entities
{
    public class CustomerNotFoundException : Exception
    {
        public CustomerNotFoundException(Guid customerId)
        : base($"Customer with ID {customerId} does not exist.") { }
    }
}

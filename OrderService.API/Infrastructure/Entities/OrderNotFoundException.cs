namespace OrderService.API.Infrastructure.Entities
{
    public class OrderNotFoundException : Exception
    {
        public OrderNotFoundException(Guid OrderId)
        : base($"Customer with ID {OrderId} does not exist.") { }
    }
}

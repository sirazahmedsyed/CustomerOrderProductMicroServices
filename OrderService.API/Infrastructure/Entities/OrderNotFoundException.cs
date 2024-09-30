namespace OrderService.API.Infrastructure.Entities
{
    public class OrderNotFoundException : Exception
    {
        public OrderNotFoundException(Guid OrderId)
        : base($"Order with ID {OrderId} does not exist.") { }
    }
}

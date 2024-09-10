namespace OrderService.API.Infrastructure.Entities
{
    public class DuplicateOrderException : Exception
    {
        public DuplicateOrderException(string message) : base(message) { }
    }
}

namespace OrderService.API.Infrastructure.Entities
{
    public class ProductNotFoundException : Exception
    {
        public ProductNotFoundException(int productId)
          : base($"Product with ID {productId} does not exist.") { }
    }
}

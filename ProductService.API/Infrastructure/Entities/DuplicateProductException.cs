namespace ProductService.API.Infrastructure.Entities
{
    public class DuplicateProductException : Exception
    {
        public DuplicateProductException(string message) : base(message) { }
    }
}

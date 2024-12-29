using ProductService.API.Infrastructure.Entities;

namespace InventoryConsumer.RabbitMQMessageBroker
{
    public class ProductMessageHandler : IMessageHandler<Product>
    {
        public async Task HandleMessage(Product message)
        {
            // Implement product processing logic
            Console.WriteLine($"Processing productId: {message.ProductId}");
            Console.WriteLine($" product Name: {message.Name}");
            Console.WriteLine($" product Description: {message.Description}");
            Console.WriteLine($" product Stock: {message.Stock}");
            Console.WriteLine($" product TaxPercentage: {message.TaxPercentage}");
            await Task.CompletedTask;
        }
    }
}

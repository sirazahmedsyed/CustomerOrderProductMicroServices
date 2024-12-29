using PurchaseService.API.Infrastructure.Entities;

namespace InventoryConsumer.RabbitMQMessageBroker
{
    public class PurchaseMessageHandler : IMessageHandler<Purchase>
    {
        public async Task HandleMessage(Purchase message)
        {
            // Implement product processing logic
            Console.WriteLine($"Processing product: {message.PurchaseId}");
            Console.WriteLine($" Product Id: {message.ProductId}");
            Console.WriteLine($" Purchase OrderNo: {message.PurchaseOrderNo}");
            Console.WriteLine($" Purchase Date: {message.PurchaseDate}");
            Console.WriteLine($" Purchase Quantity: {message.Quantity}");
            Console.WriteLine($" Purchase Supplier: {message.Supplier}");
            await Task.CompletedTask;
        }
    }
}

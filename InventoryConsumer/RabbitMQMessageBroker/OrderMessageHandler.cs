using OrderService.API.Infrastructure.Entities;

namespace InventoryConsumer.RabbitMQMessageBroker
{
    public class OrderMessageHandler : IMessageHandler<Order>
    {
        public async Task HandleMessage(Order message)
        {
            // Implement order processing logic
            Console.WriteLine($"Processing orderId: {message.OrderId}");
            Console.WriteLine($"   Customer ID: {message.CustomerId}");
            Console.WriteLine($"   Order Date: {message.OrderDate}");
            Console.WriteLine($"   Total Amount: {message.TotalAmount}");
            Console.WriteLine($"   Discount Percentage: {message.DiscountPercentage}");
            Console.WriteLine($"   Discounted Total: {message.DiscountedTotal}");
            await Task.CompletedTask;
        }
    }
}

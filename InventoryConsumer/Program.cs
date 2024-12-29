using InventoryConsumer.RabbitMQMessageBroker;
using Microsoft.Extensions.Options;
using SharedRepository.RabbitMQMessageBroker.Settings;


public class Program
{
    public static async Task Main(string[] args)
    {
        var settings = new RabbitMQSettings(); // Load from configuration

        // Approach 1: Specialized consumers for each domain like Orderservice queue
        //using var orderConsumer = new OrderConsumer( Options.Create(settings), new OrderMessageHandler() );

        // Approach 2: Single consumer for multiple queues
        using var multiConsumer = new MultiQueueConsumer(Options.Create(settings));

        Console.WriteLine("Consumers started..");
        // Register handlers for different queue types
        multiConsumer.RegisterHandler(settings.Queues.ProductCreated, new ProductMessageHandler());
        multiConsumer.RegisterHandler(settings.Queues.PurchaseCreated, new PurchaseMessageHandler());

        Console.WriteLine("Consumers started. Press any key to exit.");
        Console.ReadKey();
    }
}










//var factory = new ConnectionFactory()
//{
//    HostName = "localhost",
//    Port = 5672,
//    UserName = "guest",
//    Password = "guest"
//};

//using var connection = factory.CreateConnection();
//using var channel = connection.CreateModel();

////channel.QueueDeclare("order_created_queue", durable: true, exclusive: false);
//try
//{
//    channel.QueueDeclarePassive(queue: "order-updated-queue");
//}
//catch (Exception ex) when (ex is RabbitMQ.Client.Exceptions.BrokerUnreachableException ||
//                            ex is System.IO.IOException)
//{
//    channel.QueueDeclare(
//    queue: "order-updated-queue", durable: true, exclusive: false, autoDelete: false, arguments: null
//    );
//}

//Console.WriteLine("Waiting for messages...");

//var consumer = new EventingBasicConsumer(channel);

//consumer.Received += (model, ea) =>
//{
//    try
//    {
//        var body = ea.Body.ToArray();
//        var message = Encoding.UTF8.GetString(body);

//        var order = JsonSerializer.Deserialize<Order>(message);

//        if (order == null)
//        {
//            Console.WriteLine($"[x] Error: Unable to deserialize message: {message}");
//            channel.BasicReject(ea.DeliveryTag, false);
//            return;
//        }

//        Console.WriteLine($"   Received Order:");
//        Console.WriteLine($"   Order ID: {order.OrderId}");
//        Console.WriteLine($"   Customer ID: {order.CustomerId}");
//        Console.WriteLine($"   Order Date: {order.OrderDate}");
//        Console.WriteLine($"   Total Amount: {order.TotalAmount}");
//        Console.WriteLine($"   Discount Percentage: {order.DiscountPercentage}");
//        Console.WriteLine($"   Discounted Total: {order.DiscountedTotal}");

//        channel.BasicAck(ea.DeliveryTag, false);
//    }
//    catch (Exception ex)
//    {
//        if (ex is JsonException)
//        {
//            Console.WriteLine($"[x] JSON deserialization error: {ex.Message}");
//            channel.BasicReject(ea.DeliveryTag, false);
//        }
//        else
//        {
//            Console.WriteLine($"[x] An error occurred while processing the message: {ex.Message}");
//            channel.BasicNack(ea.DeliveryTag, false, true);
//        }
//    }
//};

//channel.BasicConsume(queue: "order-updated-queue", autoAck: false, consumer: consumer);
//Console.WriteLine(" Press [enter] to exit.");
//Console.ReadLine(); 

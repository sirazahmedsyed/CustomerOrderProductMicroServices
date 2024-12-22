using OrderService.API.Infrastructure.Entities;
using OrderService.API.Infrastructure.RabbitMQMessageBroker;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;


var factory = new ConnectionFactory()
{
    HostName = "localhost", 
    Port = 5672,
    UserName = "guest",
    Password = "guest"
};

using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

channel.QueueDeclare("order-created-queue", durable: true, exclusive: false);

//channel.QueueDeclare(queue: "order-created-queue",
//                                     durable: true,
//                                     exclusive: false,
//                                     autoDelete: false,
//                                     arguments: null);

Console.WriteLine("Waiting for messages...");

var consumer = new EventingBasicConsumer(channel);

consumer.Received += (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);

    // Deserialize the JSON message into an Order object
    var order = JsonSerializer.Deserialize<Order>(message);

    // Print specific fields from the deserialized object
    Console.WriteLine($" [x] Received Order ID: {order.OrderId}");

    // Acknowledge the message (important for autoAck: false)
    channel.BasicAck(ea.DeliveryTag, false);
};

channel.BasicConsume(queue: "order-created-queue",
                     autoAck: false, // Set to false for manual acknowledgment
                     consumer: consumer);
Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine(); 





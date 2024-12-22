using RabbitMQ.Client;
using System.Text;

var factory = new ConnectionFactory()
{
    //HostName = "localhost", // Replace with your RabbitMQ host
    HostName = "host.docker.internal",
    Port = 5672,
    UserName = "guest",
    Password = "guest",
    VirtualHost = "/",
    AutomaticRecoveryEnabled = true,
    NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
};
var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

    channel.QueueDeclare(queue: "orderQueue",
                         durable: false,
                         exclusive: false,
                         autoDelete: false,
                         arguments: null);
Console.WriteLine("Producer started. Sending messages slowly...");
// Send 100 messages in a loop
for (int i = 1; i <= 100000; i++)
{
    string message = $"Order Created: OrderId {i}";
    var body = Encoding.UTF8.GetBytes(message);

    channel.BasicPublish(exchange: "",
                         routingKey: "orderQueue",
                         basicProperties: null,
                         body: body);

    Console.WriteLine($"Sent: {message}");
    // Asynchronous delay (non-blocking)
    await Task.Delay(2000); // 2-second delay
}
Console.WriteLine("All messages sent. Press any key to exit...");
Console.ReadKey();



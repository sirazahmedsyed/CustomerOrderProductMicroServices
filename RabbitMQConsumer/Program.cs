using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;


var factory = new ConnectionFactory()
{
    HostName = "localhost", // Replace with your RabbitMQ host
    Port = 5672,
    UserName = "guest",
    Password = "guest"
};

using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

channel.QueueDeclare(queue: "orderQueue",
                     durable: false,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);
Console.WriteLine("Waiting for messages...");
var consumer = new EventingBasicConsumer(channel);
int messageCount = 0;

consumer.Received += (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    Console.WriteLine($" [x] Received {message}");

    messageCount++;
    if (messageCount >= 100000)
    {
        Console.WriteLine("Received all 10000 messages. Exiting...");
        Environment.Exit(0); // Exit after 100 messages
    }
};

channel.BasicConsume(queue: "orderQueue",
                     autoAck: true,
                     consumer: consumer);

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();



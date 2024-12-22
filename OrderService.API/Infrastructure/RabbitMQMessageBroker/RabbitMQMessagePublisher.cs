using RabbitMQ.Client;

namespace OrderService.API.Infrastructure.RabbitMQMessageBroker
{
    public class RabbitMQMessagePublisher<T> : IMessagePublisher<T>, IDisposable
    {
        private readonly IRabbitMQConnection _rabbitMQConnection;
        private readonly ILogger<RabbitMQMessagePublisher<T>> _logger;

        public RabbitMQMessagePublisher(
            IRabbitMQConnection rabbitMQConnection,
            ILogger<RabbitMQMessagePublisher<T>> logger)
        {
            _rabbitMQConnection = rabbitMQConnection;
            _logger = logger;
        }

        public async Task PublishAsync(T message, string queueName)
        {
            try
            {
                using (var channel = _rabbitMQConnection.CreateChannel())
                { 
                // Declare the queue
                channel.QueueDeclare(
                    queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false
                );

                // Serialize message
                var body = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(message);

                // Create properties
                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;
                properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

                // Publish message
                channel.BasicPublish(
                    exchange: "",
                    routingKey: queueName,
                    basicProperties: properties,
                    body: body
                );

                _logger.LogInformation($"Message published to queue {queueName}");
            }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error publishing message to queue {queueName}");
                throw;
            }
        }

        public void Dispose()
        {
            //_rabbitMQConnection?.Dispose();
        }
    }
}

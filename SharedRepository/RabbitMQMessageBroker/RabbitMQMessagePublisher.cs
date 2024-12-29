using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using SharedRepository.RabbitMQMessageBroker.Interfaces;

namespace SharedRepository.RabbitMQMessageBroker
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
                using var channel = _rabbitMQConnection.CreateChannel();

                channel.QueueDeclare(
                    queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false
                );

                var body = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(message);
                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;
                properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

                channel.BasicPublish(
                    exchange: "",
                    routingKey: queueName,
                    basicProperties: properties,
                    body: body
                );

                _logger.LogInformation($"Message published to queue {queueName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error publishing message to queue {queueName}");
                throw;
            }
        }

        public void Dispose()
        {
            // Connection disposal is handled by DI container so not implementyed otherwise it throwing the excecptions
        }
    }
}

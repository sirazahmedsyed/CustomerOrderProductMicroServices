using Confluent.Kafka;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace OrderService.API.Infrastructure.KafkaMessageBroker
{
    public class KafkaMessagePublisher<T> : IKafkaMessagePublisher<T> where T : class
    {
        private readonly IProducer<string, string> _producer;
        private readonly ILogger<KafkaMessagePublisher<T>> _logger;

        public KafkaMessagePublisher(IOptions<KafkaSettings> kafkaSettings, ILogger<KafkaMessagePublisher<T>> logger)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = kafkaSettings.Value.BootstrapServers,
                ClientId = "OrderService"
            };

            _producer = new ProducerBuilder<string, string>(config).Build();
            _logger = logger;
        }

        public async Task PublishAsync(T message, string topic)
        {
            try
            {
                var jsonMessage = JsonSerializer.Serialize(message);
                var kafkaMessage = new Message<string, string>
                {
                    Key = Guid.NewGuid().ToString(),
                    Value = jsonMessage
                };

                var deliveryResult = await _producer.ProduceAsync(topic, kafkaMessage);
                _logger.LogInformation($"Message delivered to topic {topic} at partition {deliveryResult.Partition} with offset {deliveryResult.Offset}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error publishing message to Kafka topic {topic}");
                throw;
            }
        }
    }
}

using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SharedRepository.RabbitMQMessageBroker.Settings;
using System.Text;
using System.Text.Json;

namespace InventoryConsumer.RabbitMQMessageBroker
{
    // Generic consumer that can handle multiple queue types
    public class MultiQueueConsumer : BaseRabbitMQConsumer
    {
        private readonly Dictionary<string, Func<string, Task>> _messageHandlers;

        public MultiQueueConsumer(IOptions<RabbitMQSettings> settings) : base(settings)
        {
            _messageHandlers = new Dictionary<string, Func<string, Task>>();
        }

        public void RegisterHandler<T>(string queueName, IMessageHandler<T> handler)
        {
            _messageHandlers[queueName] = async (message) =>
            {
                var deserializedMessage = JsonSerializer.Deserialize<T>(message);
                await handler.HandleMessage(deserializedMessage);
            };

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);

                    await _messageHandlers[ea.RoutingKey](message);

                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    // Log error and potentially retry or move to dead letter queue
                    _channel.BasicNack(ea.DeliveryTag, false, true);
                }
            };

            _channel.BasicConsume(queueName, false, consumer);
        }
    }


}

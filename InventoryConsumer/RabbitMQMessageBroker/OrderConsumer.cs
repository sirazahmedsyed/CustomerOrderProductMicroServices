using Microsoft.Extensions.Options;
using OrderService.API.Infrastructure.Entities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SharedRepository.RabbitMQMessageBroker.Settings;
using System.Text;
using System.Text.Json;

namespace InventoryConsumer.RabbitMQMessageBroker
{
    // Specialized consumer for Order-related messages
    public class OrderConsumer : BaseRabbitMQConsumer
    {
        private readonly IMessageHandler<Order> _orderHandler;

        public OrderConsumer(IOptions<RabbitMQSettings> settings, IMessageHandler<Order> orderHandler) : base(settings)
        {
            _orderHandler = orderHandler;
            SetupOrderQueues();
        }

        private void SetupOrderQueues()
        {
            // Setup consumer for OrderCreated queue
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var order = JsonSerializer.Deserialize<Order>(message);

                    await _orderHandler.HandleMessage(order);

                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    // Log error and potentially retry or move to dead letter queue
                    _channel.BasicNack(ea.DeliveryTag, false, true);
                }
            };

            // Start consuming from all order-related queues
            _channel.BasicConsume(_settings.Queues.OrderCreated, false, consumer);
            _channel.BasicConsume(_settings.Queues.OrderUpdated, false, consumer);
            _channel.BasicConsume(_settings.Queues.OrderDeleted, false, consumer);
        }
    }
}

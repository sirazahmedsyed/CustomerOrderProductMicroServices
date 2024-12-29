using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using SharedRepository.RabbitMQMessageBroker.Settings;

namespace InventoryConsumer.RabbitMQMessageBroker
{
    public abstract class BaseRabbitMQConsumer : IDisposable
    {
        protected readonly IConnection _connection;
        protected readonly IModel _channel;
        protected readonly RabbitMQSettings _settings;

        protected BaseRabbitMQConsumer(IOptions<RabbitMQSettings> settings)
        {
            _settings = settings.Value;
            var factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                UserName = _settings.UserName,
                Password = _settings.Password,
                VirtualHost = _settings.VirtualHost,
                Port = _settings.Port
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}

using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using SharedRepository.RabbitMQMessageBroker.Interfaces;
using SharedRepository.RabbitMQMessageBroker.Settings;

namespace SharedRepository.RabbitMQMessageBroker
{
    public class RabbitMQConnection : IRabbitMQConnection
    {
        private readonly IConnection _connection;
        private readonly ILogger<RabbitMQConnection> _logger;

        public RabbitMQConnection(RabbitMQSettings settings, ILogger<RabbitMQConnection> logger)
        {
            _logger = logger;

            var factory = new ConnectionFactory
            {
                HostName = settings.HostName,
                UserName = settings.UserName,
                Password = settings.Password,
                VirtualHost = settings.VirtualHost,
                Port = settings.Port,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            try
            {
                _connection = factory.CreateConnection();
                _logger.LogInformation("RabbitMQ connection established successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create RabbitMQ connection");
                throw;
            }
        }

        public IModel CreateChannel()
        {
            return _connection.CreateModel();
        }

        public void Dispose()
        {
            _connection?.Close();
            _connection?.Dispose();
        }
    }
}

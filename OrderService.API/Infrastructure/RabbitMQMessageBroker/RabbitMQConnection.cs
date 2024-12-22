using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace OrderService.API.Infrastructure.RabbitMQMessageBroker
{
    public class RabbitMQConnection : IRabbitMQConnection
    {
        private readonly IConnection _connection;
        private readonly ILogger<RabbitMQConnection> _logger;
        private readonly RabbitMQSettings _settings;

        public RabbitMQConnection(
            IOptions<RabbitMQSettings> options,
            ILogger<RabbitMQConnection> logger)
        {
            _settings = options.Value;
            _logger = logger;

            var factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                UserName = _settings.UserName,
                Password = _settings.Password,
                VirtualHost = _settings.VirtualHost,
                Port = _settings.Port,
                AutomaticRecoveryEnabled = true, // Enable automatic recovery
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10) // Recovery interval
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

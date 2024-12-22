using RabbitMQ.Client;

namespace OrderService.API.Infrastructure.RabbitMQMessageBroker
{
    public interface IRabbitMQConnection : IDisposable
    {
        IModel CreateChannel();
    }
}

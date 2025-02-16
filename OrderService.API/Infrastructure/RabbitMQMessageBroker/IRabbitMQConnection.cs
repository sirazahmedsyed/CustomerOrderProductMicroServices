
namespace OrderService.API.Infrastructure.RabbitMQMessageBroker
{
    public interface IRabbitMQConnection : IDisposable
    {
        RabbitMQ.Client.IModel CreateChannel();
    }
}

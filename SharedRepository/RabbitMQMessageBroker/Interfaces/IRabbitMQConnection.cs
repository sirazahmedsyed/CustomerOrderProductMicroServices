using RabbitMQ.Client;

namespace SharedRepository.RabbitMQMessageBroker.Interfaces
{
    public interface IRabbitMQConnection : IDisposable
    {
        IModel CreateChannel();
    }
}

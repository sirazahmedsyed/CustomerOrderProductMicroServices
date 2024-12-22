namespace OrderService.API.Infrastructure.RabbitMQMessageBroker
{
    public interface IMessagePublisher<T>
    {
        Task PublishAsync(T message, string queueName);
    }
}

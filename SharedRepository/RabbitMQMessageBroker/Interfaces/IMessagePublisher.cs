namespace SharedRepository.RabbitMQMessageBroker.Interfaces
{
    public interface IMessagePublisher<T>
    {
        Task PublishAsync(T message, string queueName);
    }
}

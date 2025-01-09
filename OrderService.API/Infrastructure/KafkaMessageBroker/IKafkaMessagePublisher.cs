namespace OrderService.API.Infrastructure.KafkaMessageBroker
{
    public interface IKafkaMessagePublisher<T> where T : class
    {
        Task PublishAsync(T message, string topic);
    }
}

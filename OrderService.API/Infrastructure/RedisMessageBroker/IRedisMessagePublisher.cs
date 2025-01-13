namespace OrderService.API.Infrastructure.RedisMessageBroker
{
    public interface IRedisMessagePublisher<T>
    {
        Task PublishAsync(T message, string channelName);
    }
}

using Microsoft.Extensions.Options;
using OrderService.API.Infrastructure;
using StackExchange.Redis;
using System.Text.Json;

namespace OrderService.API.Infrastructure.RedisMessageBroker
{
    public class RedisMessagePublisher<T> : IRedisMessagePublisher<T>
    {
        private readonly IConnectionMultiplexer _redisConnection;
        private readonly ILogger<RedisMessagePublisher<T>> _logger;
        public RedisMessagePublisher(IConnectionMultiplexer redisConnection, ILogger<RedisMessagePublisher<T>> logger)
        {
            _redisConnection = redisConnection;
            _logger = logger;
        }
        public async Task PublishAsync(T message, string channelName)
        {
            try
            {
                var db = _redisConnection.GetDatabase();
                var jsonMessage = JsonSerializer.Serialize(message);
                await db.PublishAsync(channelName, jsonMessage);
                _logger.LogInformation($"Message published to Redis channel {channelName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error publishing to redis channel {channelName}");
            }

        }
    }
}

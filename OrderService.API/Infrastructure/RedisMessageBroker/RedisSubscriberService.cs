using Microsoft.Extensions.Options;
using OrderService.API.Infrastructure.DTOs;
using StackExchange.Redis;
using System.Text.Json;

namespace OrderService.API.Infrastructure.RedisMessageBroker
{
    public class RedisSubscriberService : BackgroundService
    {
        private readonly IConnectionMultiplexer _redisConnection;
        private readonly ILogger<RedisSubscriberService> _logger;
        private readonly RedisChannelSettings _redisChannelSettings;

        public RedisSubscriberService(IConnectionMultiplexer redisConnection, ILogger<RedisSubscriberService> logger, IOptions<RedisChannelSettings> redisChannelSettings)
        {
            _redisConnection = redisConnection;
            _logger = logger;
            _redisChannelSettings = redisChannelSettings.Value;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var subscriber = _redisConnection.GetSubscriber();

            await subscriber.SubscribeAsync(_redisChannelSettings.OrderCreatedChannel, (channel, message) =>
            {
                HandleMessage(_redisChannelSettings.OrderCreatedChannel, message);
            });

            await subscriber.SubscribeAsync(_redisChannelSettings.OrderUpdatedChannel, (channel, message) =>
            {
                HandleMessage(_redisChannelSettings.OrderUpdatedChannel, message);
            });

            await subscriber.SubscribeAsync(_redisChannelSettings.OrderDeletedChannel, (channel, message) =>
            {
                HandleMessage(_redisChannelSettings.OrderDeletedChannel, message);
            });
            _logger.LogInformation($"Subscribed to Redis channels: {_redisChannelSettings.OrderCreatedChannel}, {_redisChannelSettings.OrderUpdatedChannel}, {_redisChannelSettings.OrderDeletedChannel}");
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
        private void HandleMessage(string channel, RedisValue message)
        {
            try
            {
                var orderDto = JsonSerializer.Deserialize<OrderDTO>(message.ToString());
                if (orderDto != null)
                {
                    switch (channel)
                    {
                        case var t when t == _redisChannelSettings.OrderCreatedChannel:
                            _logger.LogInformation($"Order created event received with OrderId: {orderDto.OrderId}");
                            break;
                        case var t when t == _redisChannelSettings.OrderUpdatedChannel:
                            _logger.LogInformation($"Order updated event received with OrderId: {orderDto.OrderId}");
                            break;
                        case var t when t == _redisChannelSettings.OrderDeletedChannel:
                            _logger.LogInformation($"Order deleted event received with OrderId: {orderDto.OrderId}");
                            break;
                    }
                }
                else
                {
                    _logger.LogWarning($"Invalid message received on channel {channel}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error handling message from Redis channel {channel}");
            }

        }
    }
}

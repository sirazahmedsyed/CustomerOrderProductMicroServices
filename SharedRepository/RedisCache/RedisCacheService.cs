using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Text.Json;

namespace SharedRepository.RedisCache
{
    public class RedisCacheService : ICacheService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly ILogger<RedisCacheService> _logger;
        private readonly IConfiguration _configuration;
        private readonly int _defaultCacheTimeInMinutes;

        public RedisCacheService(
            IConnectionMultiplexer redis,
            ILogger<RedisCacheService> logger,
            IConfiguration configuration)
        {
            _redis = redis;
            _logger = logger;
            _configuration = configuration;
            _defaultCacheTimeInMinutes = _configuration.GetValue<int>("CacheSettings:DefaultCacheTimeInMinutes", 30);
        }

        public async Task<T> GetAsync<T>(string key)
        {
            try
            {
                var db = _redis.GetDatabase();
                var value = await db.StringGetAsync(key);

                if (!value.HasValue)
                    return default;

                return JsonSerializer.Deserialize<T>(value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting value for key: {key}");
                return default;
            }
        }

        public async Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            try
            {
                var db = _redis.GetDatabase();
                var serializedValue = JsonSerializer.Serialize(value);

                return await db.StringSetAsync(
                    key,
                    serializedValue,
                    expiry ?? TimeSpan.FromMinutes(_defaultCacheTimeInMinutes)
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error setting value for key: {key}");
                return false;
            }
        }

        public async Task<bool> RemoveAsync(string key)
        {
            try
            {
                var db = _redis.GetDatabase();
                return await db.KeyDeleteAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing key: {key}");
                return false;
            }
        }

        public async Task<bool> KeyExistsAsync(string key)
        {
            try
            {
                var db = _redis.GetDatabase();
                return await db.KeyExistsAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking key existence: {key}");
                return false;
            }
        }

        public async Task<bool> RemoveByPatternAsync(string pattern)
        {
            try
            {
                var server = _redis.GetServer(_redis.GetEndPoints().First());
                var keys = server.Keys(pattern: pattern);
                var db = _redis.GetDatabase();

                foreach (var key in keys)
                {
                    await db.KeyDeleteAsync(key);
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing keys by pattern: {pattern}");
                return false;
            }
        }
    }
}

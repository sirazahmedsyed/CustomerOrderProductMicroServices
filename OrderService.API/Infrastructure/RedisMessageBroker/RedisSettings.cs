namespace OrderService.API.Infrastructure.RedisMessageBroker
{
    public class RedisSettings
    {
        public string ConnectionString { get; set; } = "localhost:6379,abortConnect=false";
    }
}

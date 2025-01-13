namespace OrderService.API.Infrastructure.RedisMessageBroker
{
    public class RedisChannelSettings
    {
        public string OrderCreatedChannel { get; set; } = "order-created-channel";
        public string OrderUpdatedChannel { get; set; } = "order-updated-channel";
        public string OrderDeletedChannel { get; set; } = "order-deleted-channel";
    }
}

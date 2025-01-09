namespace OrderService.API.Infrastructure.KafkaMessageBroker
{
    public class Topics
    {
        public string OrderCreated { get; set; } = "order-created";
        public string OrderUpdated { get; set; } = "order-updated";
        public string OrderDeleted { get; set; } = "order-deleted";
    }
}

namespace OrderService.API.Infrastructure.RabbitMQMessageBroker
{
    public class QueueSettings
    {
        public string OrderCreated { get; set; }
        public string OrderUpdated { get; set; }
        public string OrderDeleted { get; set; }
    }
}

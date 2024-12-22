namespace OrderService.API.Infrastructure.RabbitMQMessageBroker
{
    public class OrderDomainEvent
    {
        public Guid OrderId { get; set; }
        public string EventType { get; set; }
        public DateTime Timestamp { get; set; }
        public object Payload { get; set; }

        public static OrderDomainEvent Create(Guid orderId, string eventType, object payload)
        {
            return new OrderDomainEvent
            {
                OrderId = orderId,
                EventType = eventType,
                Timestamp = DateTime.UtcNow,
                Payload = payload
            };
        }
    }
}

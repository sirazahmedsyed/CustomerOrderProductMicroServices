namespace OrderService.API.Infrastructure.KafkaMessageBroker
{
    public class KafkaSettings
    {
        public string BootstrapServers { get; set; } = "localhost:9092";
        public Topics Topics { get; set; } = new Topics();
    }
}

namespace OrderService.API.Infrastructure.RabbitMQMessageBroker
{
    public class RabbitMQSettings
    {
        public string HostName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string VirtualHost { get; set; }
        public int Port { get; set; }
        public QueueSettings Queues { get; set; }
    }
}

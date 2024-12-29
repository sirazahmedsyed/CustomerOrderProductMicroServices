namespace SharedRepository.RabbitMQMessageBroker.Settings
{
    public class RabbitMQSettings
    {
        public string HostName { get; set; } = "localhost";
        public string UserName { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string VirtualHost { get; set; } = "/";
        public int Port { get; set; } = 5672;
        public QueueSettings Queues { get; set; } = new();
    }

    public class QueueSettings
    {
        public string OrderCreated { get; set; } = "order-created-queue";
        public string OrderUpdated { get; set; } = "order-updated-queue";
        public string OrderDeleted { get; set; } = "order-deleted-queue";

        public string PurchaseCreated { get; set; } = "purchase-created-queue";
        public string PurchaseUpdated { get; set; } = "purchase-updated-queue";
        public string PurchaseDeleted { get; set; } = "purchase-deleted-queue";

        public string ProductCreated { get; set; } = "product-created-queue";
        public string ProductUpdated { get; set; } = "product-updated-queue";
        public string ProductDeleted { get; set; } = "product-deleted-queue";
    }
}

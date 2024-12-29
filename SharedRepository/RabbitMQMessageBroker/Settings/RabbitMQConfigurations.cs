namespace SharedRepository.RabbitMQMessageBroker.Settings
{
    public static class RabbitMQConfigurations
    {
        public static RabbitMQSettings DefaultSettings => new()
        {
            HostName = "localhost",
            UserName = "guest",
            Password = "guest",
            VirtualHost = "/",
            Port = 5672,
            Queues = new QueueSettings
            {
                OrderCreated = "order-created-queue",
                OrderUpdated = "order-updated-queue",
                OrderDeleted = "order-deleted-queue",

                PurchaseCreated = "purchase-created-queue",
                PurchaseUpdated = "purchase-updated-queue",
                PurchaseDeleted = "purchase-deleted-queue",

                ProductCreated = "product-created-queue",
                ProductUpdated = "product-updated-queue",
                ProductDeleted = "product-deleted-queue"
            }
        };
    }
}

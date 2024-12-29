namespace InventoryConsumer.RabbitMQMessageBroker
{
    public interface IMessageHandler<T>
    {
        Task HandleMessage(T message);
    }
}

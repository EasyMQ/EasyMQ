namespace EasyMQ.Abstractions.Consumer;

public interface IEventConsumer
{
    ConsumerQueueAndExchangeConfiguration GetQueueAndExchangeConfiguration();
    Task Consume(ReceiverContext context);
}
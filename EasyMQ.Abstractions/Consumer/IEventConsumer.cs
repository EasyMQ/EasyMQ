namespace EasyMQ.Abstractions.Consumer;

/// <summary>
/// A general purpose interface for implementing a queue consumer
/// </summary>
public interface IEventConsumer
{
    ConsumerQueueAndExchangeConfiguration GetQueueAndExchangeConfiguration();
    Task Consume(ReceiverContext context);
}
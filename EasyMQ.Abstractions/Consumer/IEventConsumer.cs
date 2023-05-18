namespace EasyMQ.Abstractions.Consumer;

/// <summary>
/// A general purpose interface for implementing a queue consumer
/// </summary>
public interface IEventConsumer
{
    ConsumerConfiguration GetQueueAndExchangeConfiguration();
    Task ConsumeAsync(ReceiverContext context);
}
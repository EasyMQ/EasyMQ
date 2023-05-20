namespace EasyMQ.Abstractions.Consumer;

/// <summary>
/// A general purpose interface for implementing a queue consumer
/// </summary>
public interface IEventConsumer
{
    ConsumerConfiguration GetConsumerConfiguration();
    Task ConsumeAsync(ReceiverContext context);
}
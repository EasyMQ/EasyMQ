namespace EasyMQ.Abstractions.Producer;

/// <summary>
/// Client facing interface for only publishing events of type <see cref="TEvent"/>>
/// </summary>
/// <typeparam name="TEvent"></typeparam>
public interface IEventPublisher<in TEvent> where TEvent : IEvent
{
    public Task Publish(TEvent @event, ProducerContext context);
}
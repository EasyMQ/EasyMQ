namespace EasyMQ.Abstractions.Consumer;

public interface IEventHandler<in TEvent> where TEvent: IEvent
{
    Task BeforeHandle(ReceiverContext receiverContext)
    {
        return Task.CompletedTask;
    }
    Task Handle(ReceiverContext receiverContext, TEvent @event);

    Task PostHandle(ReceiverContext receiverContext)
    {
        return Task.CompletedTask;
    }
}
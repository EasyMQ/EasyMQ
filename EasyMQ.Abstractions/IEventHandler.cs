namespace EasyMQ.Abstractions;

public interface IEventHandler<in TEvent> where TEvent: IEvent
{
    Task BeforeHandle(MessageContext messageContext)
    {
        return Task.CompletedTask;
    }
    Task Handle(MessageContext messageContext, TEvent @event);

    Task PostHandle(MessageContext messageContext)
    {
        return Task.CompletedTask;
    }
}
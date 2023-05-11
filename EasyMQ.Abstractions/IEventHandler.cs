namespace EasyMQ.Abstractions;

public interface IEventHandler<in TEvent> where TEvent: IEvent
{
    Task Handle(MessageContext messageContext, TEvent @event);
}
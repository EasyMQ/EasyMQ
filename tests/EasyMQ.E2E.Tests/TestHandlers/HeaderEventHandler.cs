using System.Threading.Tasks;
using EasyMQ.Abstractions;
using EasyMQ.Abstractions.Consumer;

namespace EasyMQ.E2E.Tests.TestHandlers;

public class HeaderEvent : IEvent
{
    public string EventName { get; set; }
}
public class HeaderEventHandler: IEventHandler<HeaderEvent>
{
    private readonly IFakeLogger _logger;

    public HeaderEventHandler(IFakeLogger logger)
    {
        _logger = logger;
    }
    public Task Handle(ReceiverContext receiverContext, HeaderEvent @event)
    {
        _logger.Log(@event.EventName);
        return Task.CompletedTask;
    }
}
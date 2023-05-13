using System;
using System.Threading.Tasks;
using EasyMQ.Abstractions;
using EasyMQ.Abstractions.Consumer;
using Microsoft.Extensions.Logging;

namespace EasyMQ.E2E.Tests.TestHandlers;

public class TopicEvent : IEvent
{
    public string EventName { get; set; }
}

public interface IFakeLogger
{
    void Log(string message);
}
public class TopicEventHandler : IEventHandler<TopicEvent>
{
    private readonly IFakeLogger _logger;

    public TopicEventHandler(IFakeLogger logger)
    {
        _logger = logger;
    }
    public Task Handle(ReceiverContext receiverContext, TopicEvent @event)
    {
        _logger.Log(@event.EventName);
        return Task.CompletedTask;
    }
}
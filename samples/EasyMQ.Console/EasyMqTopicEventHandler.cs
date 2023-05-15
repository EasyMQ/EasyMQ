using EasyMQ.Abstractions;
using EasyMQ.Abstractions.Consumer;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EasyMQ.Console;

/// <summary>
/// A container class that implements <see cref="IEvent"/>
/// Each <see cref="IEventHandler{TEvent}"/> needs an <see cref="IEvent"/>
/// </summary>
public class EasyMqTopicEvent : IEvent
{
    public string EventName { get; set; }
}

/// <summary>
/// An example handler consuming <see cref="EasyMqTopicEvent"/>
/// </summary>
public class EasyMqTopicEventHandler : IEventHandler<EasyMqTopicEvent>
{
    private readonly ILogger<EasyMqTopicEventHandler> _logger;

    public EasyMqTopicEventHandler(ILogger<EasyMqTopicEventHandler> logger)
    {
        _logger = logger;
    }

    public Task BeforeHandle(ReceiverContext receiverContext)
    {
        _logger.LogInformation("Optionally implementing the Before hook, to do preprocessing");
        return Task.CompletedTask;
    }
    public Task Handle(ReceiverContext receiverContext, EasyMqTopicEvent topicEvent)
    {
        _logger.LogInformation("Received a new message, {event}",
            JsonConvert.SerializeObject(topicEvent, Formatting.Indented));
        return Task.CompletedTask;
    }
}
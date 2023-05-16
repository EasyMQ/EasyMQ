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

    public Task BeforeHandle(ReceiverContext receiverContext, EasyMqTopicEvent @event)
    {
        _logger.LogInformation("Optionally implementing the Before hook, to do preprocessing");
        return Task.CompletedTask;
    }
    public Task Handle(ReceiverContext receiverContext, EasyMqTopicEvent topicEvent)
    {
        _logger.LogInformation("Received a new message in {Name} with routing key {RoutingKey}, {Event}",
            nameof(EasyMqTopicEventHandler),
            receiverContext.RoutingKey,
            JsonConvert.SerializeObject(topicEvent, Formatting.Indented));
        return Task.CompletedTask;
    }
}

public class EasyMqTopicEventHandler2 : IEventHandler<EasyMqTopicEvent>
{
    private readonly ILogger<EasyMqTopicEventHandler2> _logger;

    public EasyMqTopicEventHandler2(ILogger<EasyMqTopicEventHandler2> logger)
    {
        _logger = logger;
    }
    public Task Handle(ReceiverContext receiverContext, EasyMqTopicEvent @event)
    {
        _logger.LogInformation("Received a new message in {Name} with routing key {RoutingKey}, {Event}",
            nameof(EasyMqTopicEventHandler2),
            receiverContext.RoutingKey,
            JsonConvert.SerializeObject(@event, Formatting.Indented));
        return Task.CompletedTask;
    }
}
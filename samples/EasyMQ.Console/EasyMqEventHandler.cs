using EasyMQ.Abstractions;
using EasyMQ.Abstractions.Consumer;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EasyMQ.Console;

public class EasyMqEvent : IEvent
{
    public string EventName { get; set; }
}
public class EasyMqEventHandler : IEventHandler<EasyMqEvent>
{
    private readonly ILogger<EasyMqEventHandler> _logger;

    public EasyMqEventHandler(ILogger<EasyMqEventHandler> logger)
    {
        _logger = logger;
    }

    public Task BeforeHandle(ReceiverContext receiverContext)
    {
        _logger.LogInformation("Optionally implementing the Before hook, to do preprocessing");
        return Task.CompletedTask;
    }
    public Task Handle(ReceiverContext receiverContext, EasyMqEvent @event)
    {
        _logger.LogInformation("Received a new message, {event}",
            JsonConvert.SerializeObject(@event, Formatting.Indented));
        return Task.CompletedTask;
    }
}
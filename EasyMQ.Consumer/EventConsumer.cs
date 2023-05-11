using EasyMQ.Abstractions;
using Microsoft.Extensions.Configuration;

namespace EasyMQ.Consumers;

public sealed class EventConsumer<TEvent>: IMessageConsumer
    where TEvent: class, IEvent, new()
{
    private readonly IConfiguration _configuration;
    private readonly IEventHandler<TEvent> _eventHandler;
    private readonly string _configSection;

    public EventConsumer(IConfiguration configuration,
        IEventHandler<TEvent> eventHandler,
        Func<string> configSectionFactory)
    {
        _configuration = configuration;
        _eventHandler = eventHandler;
        _configSection = configSectionFactory();
    }
    public ConsumerQueueAndExchangeConfiguration GetQueueAndExchangeConfiguration()
    {
        var rabbitConfig = _configuration.GetSection(_configSection).Get<List<ConsumerConfiguration>>();
        var config = rabbitConfig.FirstOrDefault(
            c =>
                c.EventType.Equals(typeof(TEvent).Name));
        var bindings = config.Bindings.Select(configBinding => configBinding.Arguments).ToList();
        return new ConsumerQueueAndExchangeConfiguration()
        {
            QueueName = config.QueueName,
            ExchangeName = config.ExchangeName,
            ExchangeType = config.ExchangeType,
            Bindings = bindings,
            IsDurable = config.IsDurable,
            RoutingKey = config.RoutingKey,
            IsExclusiveQueue = config.IsExclusiveQueue,
            QueueAutoDelete = config.QueueAutoDelete,
            ExchangeAutoDelete = config.ExchangeAutoDelete,
            ShouldDeclareExchange = config.ShouldDeclareExchange,
            ShouldDeclareQueue = config.ShouldDeclareQueue
        };
    }

    public async Task Consume(MessageContext context)
    {
        var newEvent = new TEvent
        {
            Context = context
        };
        await _eventHandler.Handle(newEvent);
    }
}
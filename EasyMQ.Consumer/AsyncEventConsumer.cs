using System.Text.Json;
using EasyMQ.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace EasyMQ.Consumers;

public sealed class AsyncEventConsumer<TEvent>: IMessageConsumer
    where TEvent: class, IEvent, new()
{
    private readonly IEventHandler<TEvent> _eventHandler;
    private readonly IOptions<List<ConsumerConfiguration>> _consumerConfiguration;

    public AsyncEventConsumer(IEventHandler<TEvent> eventHandler,
        IOptions<List<ConsumerConfiguration>> consumerConfiguration)
    {
        _eventHandler = eventHandler;
        _consumerConfiguration = consumerConfiguration;
    }
    public ConsumerQueueAndExchangeConfiguration GetQueueAndExchangeConfiguration()
    {
        var rabbitConfig = _consumerConfiguration.Value;
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
        var newEvent = JsonSerializer.Deserialize<TEvent>(context.Body.AsSpan(0, context.BodySize));
        await _eventHandler.BeforeHandle(context);
        await _eventHandler.Handle(context, newEvent);
        await _eventHandler.PostHandle(context);
    }
}
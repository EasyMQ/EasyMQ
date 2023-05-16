using System.Runtime.CompilerServices;
using System.Text.Json;
using EasyMQ.Abstractions;
using EasyMQ.Abstractions.Consumer;
using Microsoft.Extensions.Options;

[assembly: InternalsVisibleTo("EasyMQ.E2E.Tests")]
[assembly: InternalsVisibleTo("EasyMQ.Consumer.Tests")]


namespace EasyMQ.Consumers;

internal sealed class AsyncEventConsumer<TEvent>: IEventConsumer
    where TEvent: class, IEvent, new()
{
    private readonly Func<IEventHandler<TEvent>> _handlerFactory;
    private readonly IOptions<List<ConsumerConfiguration>> _consumerConfiguration;

    public AsyncEventConsumer(Func<IEventHandler<TEvent>> handlerFactory,
        IOptions<List<ConsumerConfiguration>> consumerConfiguration)
    {
        _handlerFactory = handlerFactory;
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

    public async Task Consume(ReceiverContext context)
    {
        var eventHandler = _handlerFactory();
        var newEvent = JsonSerializer.Deserialize<TEvent>(context.Body.AsSpan(0, context.BodySize));
        await eventHandler.BeforeHandle(context, newEvent);
        await eventHandler.Handle(context, newEvent);
        await eventHandler.PostHandle(context, newEvent);
    }
}
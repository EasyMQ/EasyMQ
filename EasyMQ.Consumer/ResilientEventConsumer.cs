using EasyMQ.Abstractions;
using EasyMQ.Abstractions.Consumer;
using Microsoft.Extensions.Options;

namespace EasyMQ.Consumers;

public sealed class ResilientEventConsumer<TEvent, THandler> : IEventConsumer
    where TEvent : class, IEvent, new()
    where THandler : IEventHandler<TEvent>
{
    private Func<IEventHandler<TEvent>> _handlerFactory;
    private IOptions<List<RabbitConsumerConfiguration>> _consumerConfiguration;

    public ResilientEventConsumer(Func<IEventHandler<TEvent>> handlerFactory,
        IOptions<List<RabbitConsumerConfiguration>> consumerConfiguration)
    {
        _handlerFactory = handlerFactory;
        _consumerConfiguration = consumerConfiguration;
    }
    public Task ConsumeAsync(ReceiverContext context)
    {
        throw new NotImplementedException();
    }

    public ConsumerConfiguration GetConsumerConfiguration()
    {
        var rabbitConfig = _consumerConfiguration.Value;
        rabbitConfig.ForEach(c => c.Validate());

        var config = rabbitConfig.FirstOrDefault(
            c =>
                c.EventName.Equals(typeof(TEvent).Name) && c.EventHandlerName.Equals(typeof(THandler).Name));
        var bindings = config.Bindings.Select(configBinding => configBinding.Arguments).ToList();
        return new ConsumerConfiguration()
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
}
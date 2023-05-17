using System.Text;
using System.Text.Json;
using EasyMQ.Abstractions;
using EasyMQ.Abstractions.Producer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace EasyMQ.Publisher;

/// <summary>
/// Bootstraps a publisher using <see cref="IEventProducer{TEvent}"/>
/// To be used with <see cref="IEventPublisher{TEvent}"/> for publishing events.
/// </summary>
/// <typeparam name="TEvent"></typeparam>
public sealed class AsyncEventPublisher<TEvent> : IEventProducer<TEvent>, IEventPublisher<TEvent>
    where TEvent : IEvent
{
    private readonly ILogger<AsyncEventPublisher<TEvent>> _logger;
    private readonly RabbitProducerConfiguration _producerConfig;
    private readonly DefaultObjectPool<IModel> _channelPool;

    public AsyncEventPublisher(IPooledObjectPolicy<IModel> pooledChannelPolicy, 
        IOptions<List<RabbitProducerConfiguration>> producerConfiguration,
        ILogger<AsyncEventPublisher<TEvent>> logger)
    {
        _logger = logger;
        _channelPool = new DefaultObjectPool<IModel>(pooledChannelPolicy, Environment.ProcessorCount * 2);
        _producerConfig = GetProducerExchangeConfiguration(producerConfiguration.Value);
    }

    public RabbitProducerConfiguration GetProducerExchangeConfiguration(
        List<RabbitProducerConfiguration> rabbitProducerConfigurations)
    {
        rabbitProducerConfigurations.ForEach(p => p.Validate());
        var config = rabbitProducerConfigurations.FirstOrDefault(
            c =>
                c.EventName.Equals(typeof(TEvent).Name));
        return config;
    }

    public Task PublishAsync(TEvent @event, ProducerContext context)
    {
        var channel = _channelPool.Get();
        try
        {
            if (_producerConfig.ShouldDeclareExchange)
                channel.ExchangeDeclare(_producerConfig.ExchangeName, _producerConfig.ExchangeType,
                    _producerConfig.IsDurable, _producerConfig.ExchangeAutoDelete);
            IBasicProperties basicProperties = null;
            if (context.Headers != null)
            {
                basicProperties = channel.CreateBasicProperties();
                basicProperties.Headers = context.Headers;
            }
            channel.BasicPublish(_producerConfig.ExchangeName, context.RoutingKey, context.Mandatory, basicProperties,
                Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event)));
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to publish message to RabbitMQ, " +
                             "{ExceptionType} {Message} {Data}",
                e.GetType(),
                e.Message,
                e.Data);
        }
        finally
        {
            _channelPool.Return(channel);
        }
        
        return Task.CompletedTask;
    }
}
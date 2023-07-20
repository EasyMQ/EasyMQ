using System.Text;
using System.Text.Json;
using EasyMQ.Abstractions;
using EasyMQ.Abstractions.Producer;
using EasyMQ.EventHost.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace EasyMQ.Publisher;

/// <summary>
/// Bootstraps a publisher <see cref="IEventPublisher{TEvent}"/>
/// </summary>
/// <typeparam name="TEvent"></typeparam>
public sealed class AsyncEventPublisher<TEvent> : IEventPublisher<TEvent>
    where TEvent : IEvent
{
    private readonly ILogger<AsyncEventPublisher<TEvent>> _logger;
    private readonly RabbitProducerConfiguration _producerConfig;
    private readonly IConnectionProvider _connectionProvider;
    private readonly IModel _channel;
    private readonly object _channelLock;
    private readonly DefaultObjectPool<IModel> _channelPool;

    public AsyncEventPublisher(IPooledObjectPolicy<IModel> pooledChannelPolicy, 
        IOptions<List<RabbitProducerConfiguration>> producerConfiguration,
        ILogger<AsyncEventPublisher<TEvent>> logger,
        IConnectionProvider connectionProvider)
    {
        _logger = logger;
        _channelPool = new DefaultObjectPool<IModel>(pooledChannelPolicy, Environment.ProcessorCount * 2);
        _producerConfig = GetProducerExchangeConfiguration(producerConfiguration.Value);
        _connectionProvider = connectionProvider;

        _channelLock = new object();
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
            lock (_channelLock)
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
                channel.BasicPublish(_producerConfig.ExchangeName, _producerConfig.RoutingKey, context.Mandatory, basicProperties,
                    Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event)));
            }
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
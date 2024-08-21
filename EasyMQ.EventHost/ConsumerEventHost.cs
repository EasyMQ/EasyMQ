using System.Buffers;
using System.Collections.Concurrent;
using System.Threading.Channels;
using EasyMQ.Abstractions;
using EasyMQ.Abstractions.Consumer;
using EasyMQ.EventHost.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace EasyMQ.Consumer;

/// <summary>
/// Bootstraps a consumer of type <see cref="IEventConsumer"/>
/// </summary>
/// <typeparam name="TConsumer">Implements all <see cref="IEventConsumer"/> types.</typeparam>
public sealed class ConsumerEventHost<TConsumer> : IHostedService
    where TConsumer: IEventConsumer
{
    private readonly TConsumer _consumer;
    private readonly IConnectionProvider _provider;
    private readonly ILogger<ConsumerEventHost<TConsumer>> _logger;
    private Task _processorTask = null!;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private IModel _consumerChannel = null!;
    private readonly Channel<Func<ValueTask>> _processorChannel;

    public ConsumerEventHost(TConsumer consumer,
        IConnectionProvider provider,
        ILogger<ConsumerEventHost<TConsumer>> logger)
    {
        _consumer = consumer;
        _provider = provider;
        _logger = logger;
        _cancellationTokenSource = new CancellationTokenSource();
        _processorChannel = Channel.CreateUnbounded<Func<ValueTask>>();
    }
    public Task StartAsync(CancellationToken cancellationToken)
    {
        var connection = _provider.AcquireConsumerConnection();
        _consumerChannel = SetupConsumer(connection);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _consumerChannel.Close();
        _processorChannel.Writer.Complete();
        // Wait for queue to process pending requests
        while(_processorChannel.Reader.Count > 0) { }
        _cancellationTokenSource.Cancel();
            
        return Task.CompletedTask;
    }

    private IModel SetupConsumer(IConnection connection)
    {
        IModel channel;
        try
        {
            var config = _consumer.GetConsumerConfiguration();
                
            channel = connection.CreateModel();
            ConfigureExchangeAndQueues(config, channel);

            _processorTask = Task.Factory.StartNew(ProcessTasks, TaskCreationOptions.LongRunning);

            var basicConsumer = new AsyncEventingBasicConsumer(channel);
            basicConsumer.Received += async (_, args) =>
            {
                // Need to copy, since args.Body is not thread safe
                // Zero allocation buffer
                // Rent may return size more than requested or equal
                // Need to create a span using .AsSpan(0, length) to read it correctly
                var sharedMemory = ArrayPool<byte>.Shared;
                var bodyLength = args.Body.Length;
                var rentedMemory = sharedMemory.Rent(bodyLength);
                args.Body.CopyTo(rentedMemory);
                var written = _processorChannel.Writer.TryWrite(async () =>
                {
                    try
                    {
                        await _consumer.ConsumeAsync(new ReceiverContext(
                            rentedMemory,
                            args.RoutingKey,
                            (ushort) args.Body.Length,
                            args.DeliveryTag,
                            args.Exchange,
                            args.ConsumerTag,
                            args.Redelivered));
                        channel.BasicAck(args.DeliveryTag, false);
                    }
                    catch (Exception e)
                    {
                        _logger.LogCritical(
                            "Could not process the message successfully:: {ExceptionType} \n" +
                            "{Message} \n " +
                            "StackTrace:: {StackTrace} \n",
                            e.Message,
                            e.StackTrace,
                            e.GetType());
                        channel.BasicNack(args.DeliveryTag, false, true);
                    }
                    finally
                    {
                        ArrayPool<byte>.Shared.Return(rentedMemory);
                    }
                });
                if (!written)
                {
                    channel.BasicNack(args.DeliveryTag, false, true);
                }

                await Task.Yield();
            };
            channel.BasicConsume(config.QueueName, false, consumer: basicConsumer);
        }
        catch (OperationInterruptedException ex)
        {
            _logger.LogCritical("{Exception}:: {SerializeObject}", 
                nameof(OperationInterruptedException), 
                JsonConvert.SerializeObject(ex));
            throw new ApplicationException($"RMQ OperationInterruptedException {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError("{Exception}:: {SerializeObject}", 
                nameof(Exception), 
                JsonConvert.SerializeObject(ex));
            throw;
        }
        return channel;
    }
    
    private void ConfigureExchangeAndQueues(ConsumerConfiguration config, IModel channel)
    {
        _logger.LogInformation("{Object}", JsonConvert.SerializeObject(config));
        if (config.ShouldDeclareQueue)
            channel.QueueDeclare(config.QueueName, config.IsDurable, config.IsExclusiveQueue,
                config.QueueAutoDelete);
        if (config.ShouldDeclareExchange)
            channel.ExchangeDeclare(config.ExchangeName, config.ExchangeType, config.IsDurable,
                config.ExchangeAutoDelete);
        if (!string.IsNullOrWhiteSpace(config.RoutingKey) && config.ExchangeType.Equals(ExchangeType.Topic))
            channel.QueueBind(config.QueueName, config.ExchangeName, config.RoutingKey);
        if (config.ExchangeType.Equals(ExchangeType.Fanout))
            channel.QueueBind(config.QueueName, config.ExchangeName, "");
        if (config.ExchangeType.Equals(ExchangeType.Direct))
            channel.QueueBind(config.QueueName, config.ExchangeName, config.RoutingKey);

        config.Bindings.ForEach(b => channel.QueueBind(config.QueueName, config.ExchangeName, config.RoutingKey, b));
    }
    
    private async Task ProcessTasks()
    {
        await foreach (var action in _processorChannel.Reader.ReadAllAsync(_cancellationTokenSource.Token))
        {
            // Don't want any rogue exception crashing the entire task queue
            try
            {
                await action.Invoke();
            }
            catch (ObjectDisposedException) { }
            catch (Exception ex)
            {
                _logger.LogCritical(
                    "Message was not processed ExceptionMessage:: {Message} \n" +
                    "StackTrace:: {StackTrace} \n" +
                    "Data:: {Data} \n",
                    ex.Message,
                    ex.StackTrace,
                    ex.Data);
            }
        }
    }
}
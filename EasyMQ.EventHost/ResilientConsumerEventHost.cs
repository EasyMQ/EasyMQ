using EasyMQ.EventHost.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Channels;
using System.Threading;
using Newtonsoft.Json;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using RabbitMQ.Client;
using EasyMQ.Abstractions.Consumer;
using System.Buffers;

namespace EasyMQ.Consumer
{
    public sealed class ResilientConsumerEventHost<TConsumer> : IHostedService
        where TConsumer: IEventConsumer
    {
        public ResilientConsumerEventHost(TConsumer consumer,
            IConnectionProvider connectionProvider,
            ILogger<ResilientConsumerEventHost<TConsumer>> logger)
        {
            _consumer = consumer;
            _connectionProvider = connectionProvider;
            _logger = logger;

            _cancellationTokenSource = new CancellationTokenSource();
            _processorChannel = Channel.CreateUnbounded<Func<ValueTask>>();
        }

        private TConsumer _consumer { get; }
        private IConnectionProvider _connectionProvider { get; }
        public ILogger<ResilientConsumerEventHost<TConsumer>> _logger { get; }

        private CancellationTokenSource _cancellationTokenSource;
        private Channel<Func<ValueTask>> _processorChannel;
        private Task<Task> _processorTask;
        private IModel _consumerChannel;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var connection = _connectionProvider.AcquireConsumerConnection();
            _consumerChannel = SetupConsumer(connection);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _consumerChannel.Close();
            _processorChannel.Writer.Complete();
            // Wait for queue to process pending requests
            while (_processorChannel.Reader.Count > 0) { }
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
                var resiliencyWrapper = new PolicyWrapper(config.RetryPolicy, _connectionProvider);

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
                                (ushort)args.Body.Length,
                                args.DeliveryTag,
                                args.Exchange,
                                args.ConsumerTag,
                                args.Redelivered));
                            channel.BasicAck(args.DeliveryTag, false);
                        }
                        catch (Exception e)
                        {
                            channel.BasicAck(args.DeliveryTag, false);
                            _logger.LogCritical(
                                "Could not process the message successfully:: {ExceptionType} \n" +
                                "{Message} \n " +
                                "StackTrace:: {StackTrace} \n",
                                e.Message,
                                e.StackTrace,
                                e.GetType());
                            if (args.BasicProperties.Headers is not null)
                            {
                                if (args.BasicProperties.Headers.ContainsKey("x-retries"))
                                {
                                    args.BasicProperties.Headers.TryGetValue("x-retries", out var retries);
                                    if ((int)retries == config.RetryPolicy.RetryNumberOfTimes)
                                    {
                                        // no-op
                                        //channel.BasicNack(args.DeliveryTag, false, false);
                                    }
                                    else
                                    {
                                        resiliencyWrapper.ApplyPolicy(config, args);
                                    }
                                }
                            }
                            else
                            {
                                resiliencyWrapper.ApplyPolicy(config, args);
                            }
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

            config.Bindings.ForEach(b => channel.QueueBind(config.QueueName, config.ExchangeName, config.RoutingKey, b));

            if (config.RetryPolicy != null)
            {
                var exchangeArguments = new Dictionary<string, object>
                {
                    { "x-delayed-type", config.ExchangeType}
                };
                switch (config.RetryPolicy.RetryType)
                {
                    
                    case RetryType.Immediate:
                        channel.ExchangeDeclare(exchange: $"{config.ExchangeName}.immediate.retry", type: "x-delayed-message", durable: config.IsDurable, arguments: exchangeArguments, autoDelete: config.ExchangeAutoDelete);
                        channel.QueueBind(config.QueueName, $"{config.ExchangeName}.immediate.retry", routingKey: config.QueueName);
                        break;
                    case RetryType.Delayed:
                        channel.ExchangeDeclare(exchange: $"{config.ExchangeName}.delayed.retry", type: "x-delayed-message", durable: config.IsDurable, arguments: exchangeArguments, autoDelete: config.ExchangeAutoDelete);
                        channel.QueueBind(config.QueueName, $"{config.ExchangeName}.delayed.retry", routingKey: config.QueueName);
                        break;
                }
            }
        }
    }
}

## easyMQ
[![dotnet](https://github.com/coderookie1994/easyMQ/actions/workflows/dotnet.yml/badge.svg?branch=main)](https://github.com/coderookie1994/easyMQ/actions/workflows/dotnet.yml)

A dead simple library which aims to simplify writing [RabbitMQ](https://www.rabbitmq.com/) code in [.NET Core](https://learn.microsoft.com/en-us/dotnet/core/introduction).

No need to manually write consumers or manage connections and channels. Each domain event gets its own channel and `IHostedService` which will consume events using RabbitMQ's `AsyncEventingBasicConsumer`.
Each domain event needs to be configured in a consumer section defined by the client application. Here is an example of a producer and consumer configuration from one of the samples below: -

```json
{
  "RabbitConsumerConfigurations": [
    {
      "EventType": "EasyMqEvent",
      "QueueName": "easymq_q",
      "ExchangeName": "easymq.tx",
      "ExchangeType": "topic",
      "RoutingKey": "test",
      "IsDurable": false,
      "ShouldDeclareQueue": true,
      "ShouldDeclareExchange": true,
      "IsExclusiveQueue": false,
      "QueueAutoDelete": true,
      "ExchangeAutoDelete": false,
      "Bindings": []
    }
  ],
  "RabbitProducerConfigurations": [
    {
      "EventType": "EasyMqEvent",
      "ExchangeName": "easymq.tx",
      "ExchangeType": "topic",
      "ShouldDeclareExchange": true,
      "IsDurable": false,
      "ExchangeAutoDelete": false,
      "RoutingKey": "test"
    }
  ]
}
```
With the configuration above, an event consumer of type `AsyncEventConsumer<EasyMqEvent>` with message type of `EasyMqEvent` will be spawned with the given queue and exchange configuration.
Similarly an `AsyncEventProducer<EasyMqEvent>` will be provisioned with an `IEventPublisher<EasyMqEvent>` accessible to the client for publishing messages.
The corresponding code for the consumer event handler is as follows: -

```csharp
public class EasyMqEventHandler : IEventHandler<EasyMqEvent>
{
    private readonly ILogger<EasyMqEventHandler> _logger;

    public EasyMqEventHandler(ILogger<EasyMqEventHandler> logger)
    {
        _logger = logger;
    }

    public Task BeforeHandle(MessageContext messageContext)
    {
        _logger.LogInformation("Optionally implementing the Before hook, to do preprocessing");
        return Task.CompletedTask;
    }
    public Task Handle(MessageContext messageContext, EasyMqEvent @event)
    {
        _logger.LogInformation("Received a new message, {event}",
            JsonConvert.SerializeObject(@event, Formatting.Indented));
        return Task.CompletedTask;
    }
}
```
On the producer side
```csharp
    
  private readonly IEventPublisher<EasyMqEvent> _eventPublisher;
  
  await _eventPublisher.Publish(new EasyMqEvent()
                  {
                      EventName = "Hello world"
                  }, new ProducerContext()
                  {
                      Mandatory = false,
                      RoutingKey = "test"
                  });
```
In startup, calling the two extension methods `AddEasyMq`, `AddEventConsumer` and `AddEventProducer` is all it takes to configure the event handlers.

```csharp
await Host.CreateDefaultBuilder(args)
    .ConfigureHostConfiguration(configurationBuilder => configurationBuilder
        .AddJsonFile("appsettings.json", false, true)
        .AddUserSecrets<Program>())
    .ConfigureServices((context, services) =>
    {
        services.AddEasyMq(context.Configuration, builder =>
            {
                builder.WithConnectionFactory(factory =>
                {
                    factory.Uri =
                        new Uri(
                            $"amqp://{context.Configuration["rmq_username"]}:{context.Configuration["rmq_password"]}@localhost:5672/");
                    factory.DispatchConsumersAsync = true;
                    factory.TopologyRecoveryEnabled = true;
                    factory.AutomaticRecoveryEnabled = true;
                    return factory;
                });
                builder.WithConsumerSection("RabbitConsumerConfigurations");
                builder.WithProducerSection("RabbitProducerConfigurations");
            })
            .AddEventConsumer<EasyMqEvent, EasyMqEventHandler>()
            .AddEventProducer<EasyMqEvent>();
    })
    .RunConsoleAsync();
```
### Pending Action Items
- [ ] Consumer capabilities
  - [x] Consume from Topic Exchanges
  - [x] Consume from Header Exchanges
  - [x] Consume from Fanout Exchanges
  - [ ] Consume from Direct Exchanges
  - [ ] Consume from Default Exchange 
- [x] Producer capabilities
- [ ] Tracing
- [ ] Error queues and exception flows





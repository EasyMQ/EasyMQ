## easyMQ
[![dotnet](https://github.com/coderookie1994/easyMQ/actions/workflows/dotnet.yml/badge.svg?branch=main)](https://github.com/coderookie1994/easyMQ/actions/workflows/dotnet.yml)

A dead simple library which aims to simplify writing [RabbitMQ](https://www.rabbitmq.com/) code in [.NET Core](https://learn.microsoft.com/en-us/dotnet/core/introduction).

No need to manually write consumers or manage connections and channels. Each domain event gets its own channel and `IHostedService` which will consume events using RabbitMQ's `AsyncEventingBasicConsumer`.
Each domain event needs to be configured in a consumer section defined by the client application. Here is an example from one of the samples below: -

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
  ]
}
```
With the configuration above, an event consumer of type `AsyncEventConsumer<EasyMqEvent>` with message type of `EasyMqEvent` will be spawned with the given queue and exchange configuration.
The corresponding code for the event handler is as follows: -

```csharp
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
    public Task Handle(MessageContext messageContext, EasyMqEvent @event)
    {
        _logger.LogInformation("Received a new message, {event}",
            JsonConvert.SerializeObject(@event, Formatting.Indented));
        return Task.CompletedTask;
    }
}
```
In startup, calling the two extension methods `AddEasyMqConsumer` and `AddEventConsumer` is all it takes to configure the event handlers.

```csharp
await Host.CreateDefaultBuilder(args)
    .ConfigureHostConfiguration(configurationBuilder => configurationBuilder.AddJsonFile("appsettings.json", false, true))
    .ConfigureServices((context, services) =>
    {
        services.AddEasyMqConsumer(factory =>
            {
                factory.Uri = new Uri("amqp://localhost:5672/");
                factory.DispatchConsumersAsync = true;
                factory.TopologyRecoveryEnabled = true;
                factory.AutomaticRecoveryEnabled = true;
                return factory;
            }, context.Configuration, "RabbitConsumerConfigurations")
            .AddEventConsumer<EasyMqEvent, EasyMqEventHandler>();
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
- [ ] Producer capabilities
- [ ] Tracing
- [ ] Error queues and exception flows





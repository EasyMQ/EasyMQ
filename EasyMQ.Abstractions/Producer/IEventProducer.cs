namespace EasyMQ.Abstractions.Producer;

public interface IEventProducer<in TEvent> where TEvent: IEvent
{
    internal RabbitProducerConfiguration GetProducerExchangeConfiguration(List<RabbitProducerConfiguration> rabbitProducerConfigurations);
    Task Publish(TEvent @event, ProducerContext context);
}

public class ProducerContext
{
    public string RoutingKey { get; set; }
    public bool Mandatory { get; set; }
}
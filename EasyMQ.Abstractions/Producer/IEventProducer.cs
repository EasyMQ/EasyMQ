namespace EasyMQ.Abstractions.Producer;

/// <summary>
/// An internal interface which is used to bootstrap a producer.
/// </summary>
/// <typeparam name="TEvent"></typeparam>
public interface IEventProducer<in TEvent> where TEvent: IEvent
{
    RabbitProducerConfiguration GetProducerExchangeConfiguration(List<RabbitProducerConfiguration> rabbitProducerConfigurations);
}

public class ProducerContext
{
    public string RoutingKey { get; set; } = string.Empty;
    public bool Mandatory { get; set; }
    public Dictionary<string, dynamic> Headers { get; set; } = new();
}
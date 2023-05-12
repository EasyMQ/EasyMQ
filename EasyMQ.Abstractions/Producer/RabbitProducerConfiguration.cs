namespace EasyMQ.Abstractions.Producer;

public class RabbitProducerConfiguration
{
    public string EventType { get; set; }
    public string ExchangeName { get; set; }
    public string ExchangeType { get; set; }
    public bool ShouldDeclareExchange { get; set; }
    public bool IsDurable { get; set; }
    public bool ExchangeAutoDelete { get; set; }
    public string RoutingKey { get; set; }
}
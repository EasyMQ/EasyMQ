using CommunityToolkit.Diagnostics;

namespace EasyMQ.Abstractions.Consumer;

public class RabbitConsumerConfiguration
{
    public RabbitConsumerConfiguration()
    {
        Bindings = new List<Binding>();
    }
    public string EventName { get; set; }
    public string EventHandlerName { get; set; }
    public bool IsDurable { get; set; }
    public string QueueName { get; set; }
    public string ExchangeName { get; set; }
    public string ExchangeType { get; set; }
        
    public bool ShouldDeclareQueue { get; set; }
    public bool ShouldDeclareExchange { get; set; }
    public List<Binding> Bindings { get; set; }
    public bool IsExclusiveQueue { get; set; }
    public bool QueueAutoDelete { get; set; }
    public bool ExchangeAutoDelete { get; set; }
    public string RoutingKey { get; set; } = string.Empty;

    public void Validate()
    {
        Guard.IsNotNullOrEmpty(EventName);
        Guard.IsNotNullOrEmpty(EventHandlerName);
        Guard.IsNotNullOrEmpty(QueueName);
    }
}
public class Binding
{
    public Dictionary<string, dynamic> Arguments { get; set; }
}
namespace EasyMQ.Abstractions.Consumer;

public class ConsumerConfiguration
{
    public ConsumerConfiguration()
    {
        Bindings = new List<Binding>();
    }
    public string EventType { get; set; }
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
}
public class Binding
{
    public Dictionary<string, dynamic> Arguments { get; set; }
}
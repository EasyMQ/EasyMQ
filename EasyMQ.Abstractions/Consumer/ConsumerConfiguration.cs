namespace EasyMQ.Abstractions.Consumer;

public class ConsumerConfiguration
{
    public string ExchangeName { get; set; }
    public string QueueName { get; set; }
    public string ExchangeType { get; set; }
    public bool IsDurable { get; set; }
    public bool ShouldDeclareQueue { get; set; }
    public bool ShouldDeclareExchange { get; set; }
    public List<Dictionary<string,object>> Bindings { get; set; }
    public bool IsExclusiveQueue { get; set; }
    public bool QueueAutoDelete { get; set; }
    public bool ExchangeAutoDelete { get; set; }
    public string RoutingKey { get; set; } = string.Empty;
}
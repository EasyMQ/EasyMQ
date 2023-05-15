namespace EasyMQ.Abstractions.Consumer;

public class ReceiverContext
{
    public byte[] Body { get; private set; }
    public string RoutingKey { get; private set; }
    public ushort BodySize { get; private set; }
    public ulong DeliveryTag { get; private set; }
    public string Exchange { get; private set; }
    public string ConsumerTag { get; private set; }
    public bool Redelivered { get; private set; }
    public ReceiverContext(byte[] body, 
        string routingKey, 
        ushort bodySize,
        ulong deliveryTag,
        string exchange,
        string consumerTag,
        bool redelivered)
    {
        DeliveryTag = deliveryTag;
        Body = body;
        RoutingKey = routingKey;
        BodySize = bodySize;
        Exchange = exchange;
        ConsumerTag = consumerTag;
        Redelivered = redelivered;
    }
}
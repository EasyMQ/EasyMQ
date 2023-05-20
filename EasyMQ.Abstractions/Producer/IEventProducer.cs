namespace EasyMQ.Abstractions.Producer;

public class ProducerContext
{
    public bool Mandatory { get; set; }
    public Dictionary<string, dynamic> Headers { get; set; } = new();
}
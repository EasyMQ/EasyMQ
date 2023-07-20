namespace EasyMQ.Abstractions.Consumer;

public class RetryPolicy
{
    public string Type { get; set; }
    public int RetryNumberOfTimes { get; set; }
}

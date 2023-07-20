namespace EasyMQ.Abstractions.Consumer;

public class RetryPolicy
{
    public RetryType RetryType;
    public int RetryNumberOfTimes { get; set; }
    
}

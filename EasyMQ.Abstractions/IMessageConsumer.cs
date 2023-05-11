namespace EasyMQ.Abstractions;

public interface IMessageConsumer
{
    ConsumerQueueAndExchangeConfiguration GetQueueAndExchangeConfiguration();
    Task Consume(MessageContext context);
}
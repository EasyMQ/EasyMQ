using RabbitMQ.Client;

namespace EasyMQ.Consumer.Interfaces;

public interface IConnectionProvider
{
    internal IConnection AcquireConsumerConnection();
    internal IConnection AcquireProducerConnection();
}
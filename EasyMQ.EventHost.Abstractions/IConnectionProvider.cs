using RabbitMQ.Client;

namespace EasyMQ.EventHost.Abstractions;

public interface IConnectionProvider
{
    IConnection AcquireConsumerConnection();
    IConnection AcquireProducerConnection();
}
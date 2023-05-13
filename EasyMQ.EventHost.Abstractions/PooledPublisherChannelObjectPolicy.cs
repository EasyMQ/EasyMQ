using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Client;

namespace EasyMQ.EventHost.Abstractions;

public sealed class PooledPublisherChannelObjectPolicy : IPooledObjectPolicy<IModel>
{
    private readonly IConnection _connection;

    public PooledPublisherChannelObjectPolicy(IConnectionProvider connectionProvider)
    {
        _connection = connectionProvider.AcquireProducerConnection();
    }
    public IModel Create()
    {
        return _connection.CreateModel();
    }

    public bool Return(IModel obj)
    {
        if (obj.IsOpen)
            return true;
        
        obj?.Dispose();
        return false;
    }
}
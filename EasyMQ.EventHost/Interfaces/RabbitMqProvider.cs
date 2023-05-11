using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace EasyMQ.Consumer.Interfaces;

public class RabbitMqProvider : IRabbitMqProvider, IConnectionProvider
{
    private bool _consumerConnectionInitialized;
    private bool _producerConnectionInitialized;
    private readonly IConnectionFactory _factory;
    private readonly ILogger<RabbitMqProvider> _logger;
    private IConnection _consumerConnection;
    private IConnection _producerConnection;

    public RabbitMqProvider(IConnectionFactory factory, ILogger<RabbitMqProvider> logger)
    {
        _factory = factory;
        _logger = logger;
    }
    public void InitializeConnection()
    {
        if (_consumerConnectionInitialized) return;
        if (_producerConnectionInitialized) return;
            
        try
        {
            _consumerConnection = _factory.CreateConnection();
            _producerConnection = _factory.CreateConnection();
                
            _consumerConnectionInitialized = true;
            _producerConnectionInitialized = true;
        }
        catch (BrokerUnreachableException ex)
        {
            _logger.LogCritical($"Broker unreachable:: {ex.Message}");
        }
    }

    public IConnection AcquireConsumerConnection()
    {
        if (_consumerConnectionInitialized)
            return _consumerConnection;
            
        throw new ApplicationException("RMQ consumer connection not initialized");
    }

    public IConnection AcquireProducerConnection()
    {
        if (_producerConnectionInitialized)
            return _producerConnection;
            
        throw new ApplicationException("RMQ producer connection not initialized");
    }

    public void ShutDown()
    {
        _consumerConnection?.Dispose();
        _producerConnection?.Dispose();
    }
}
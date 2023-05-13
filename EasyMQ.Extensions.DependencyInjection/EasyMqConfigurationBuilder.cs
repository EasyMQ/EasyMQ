using RabbitMQ.Client;

namespace EasyMQ.Extensions.DependencyInjection;

public sealed class EasyMqConfigurationBuilder
{
    private Func<ConnectionFactory,IConnectionFactory> _factory;
    private string _consumerSection = string.Empty;
    private string _producerSection = string.Empty;

    public EasyMqConfigurationBuilder()
    {
        IConnectionFactory DefaultFactory(ConnectionFactory factory)
        {
            return new ConnectionFactory() {Uri = new Uri("amqp://localhost:5672"), DispatchConsumersAsync = true, TopologyRecoveryEnabled = true, AutomaticRecoveryEnabled = true};
        }

        _factory = DefaultFactory;
    }
    public void WithConsumerSection(string sectionName)
    {
        _consumerSection = sectionName;
    }

    public void WithProducerSection(string sectionName)
    {
        _producerSection = sectionName;
    }

    public void WithConnectionFactory(Func<ConnectionFactory, IConnectionFactory> factory)
    {
        _factory = factory;
    }

    internal string GetConsumerSection() => _consumerSection;
    internal string GetProducerSection() => _producerSection;
    internal Func<ConnectionFactory, IConnectionFactory> GetFactory => _factory;
}
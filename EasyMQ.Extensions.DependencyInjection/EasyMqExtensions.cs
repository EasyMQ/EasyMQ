using EasyMQ.Abstractions.Consumer;
using EasyMQ.Abstractions.Producer;
using EasyMQ.Consumer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace EasyMQ.Extensions.DependencyInjection;

public static class EasyMqExtensions
{
    /// <summary>
    /// Configures basic internal dependencies for RabbitMQ.
    /// <see cref="setup"/> captures the <see cref="ConnectionFactory"/> configuration and consumer/producer sections.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="setup"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static IServiceCollection AddEasyMq(this IServiceCollection services,
        IConfiguration configuration,
        Action<EasyMqConfigurationBuilder> setup)
    {
        var builder = new EasyMqConfigurationBuilder();
        setup(builder);
        _ = builder.GetConsumerSection() != string.Empty
            ? services.Configure<List<ConsumerConfiguration>>(configuration.GetSection(builder.GetConsumerSection()))
            : null;
        _ = builder.GetProducerSection() != string.Empty
            ? services.Configure<List<RabbitProducerConfiguration>>(configuration.GetSection(builder.GetProducerSection()))
            : null;

        services.AddRabbitMq(builder.GetFactory);
        
        return services;
    }
}
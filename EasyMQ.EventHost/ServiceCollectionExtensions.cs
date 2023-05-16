using EasyMQ.Consumer;
using EasyMQ.EventHost.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Client;

namespace EasyMQ.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRabbitMq(this IServiceCollection services,
        Func<ConnectionFactory, IConnectionFactory> connFactory)
    {
        services.TryAddSingleton<RabbitMqProvider>();
        services.TryAddSingleton<IConnectionProvider>(sp =>
        {
            var provider = sp.GetRequiredService<RabbitMqProvider>();
            provider.InitializeConnection();
            return provider;
        });
        services.TryAddSingleton<IRabbitMqProvider>(sp => sp.GetRequiredService<RabbitMqProvider>());
        services.TryAddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
        services.AddSingleton<IPooledObjectPolicy<IModel>, PooledPublisherChannelObjectPolicy>();
        _ = connFactory ?? throw new ArgumentNullException($"{nameof(connFactory)} cannot be null");
        var connectionFactory = connFactory(new ConnectionFactory());
        services.AddSingleton(connectionFactory);
        return services;
    }
}
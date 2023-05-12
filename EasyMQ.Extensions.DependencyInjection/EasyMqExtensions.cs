using EasyMQ.Abstractions;
using EasyMQ.Abstractions.Consumer;
using EasyMQ.Abstractions.Producer;
using EasyMQ.Consumer;
using EasyMQ.Consumers;
using EasyMQ.EventHost.Abstractions;
using EasyMQ.Publisher;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Client;

namespace EasyMQ.Extensions.DependencyInjection;

public static class EasyMqExtensions
{
    public static IServiceCollection AddEasyMq(this IServiceCollection services, 
        Func<ConnectionFactory, IConnectionFactory> rmqConnectionFactory, 
        IConfiguration configuration, 
        string? consumerSection = null,
        string? producerSection = null)
    {
        _ = consumerSection != null
            ? services.Configure<List<ConsumerConfiguration>>(configuration.GetSection(consumerSection))
            : null;
        _ = producerSection != null
            ? services.Configure<List<RabbitProducerConfiguration>>(configuration.GetSection(producerSection))
            : null;
        
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
        _ = rmqConnectionFactory ?? throw new ArgumentNullException($"{nameof(rmqConnectionFactory)} cannot be null");
        var connectionFactory = rmqConnectionFactory(new ConnectionFactory());
        services.AddSingleton(connectionFactory);
        return services;
    }
    public static IServiceCollection AddEventConsumer<TEvent, THandler>(this IServiceCollection services) 
        where TEvent : class, IEvent, new()
        where THandler: class, IEventHandler<TEvent>
    {
        services.AddTransient<IEventHandler<TEvent>, THandler>();
        services.AddSingleton<AsyncEventConsumer<TEvent>>();
        services.AddHostedService<ConsumerEventHost<AsyncEventConsumer<TEvent>>>();
        return services;
    }

    public static IServiceCollection AddEventProducer<TEvent>(this IServiceCollection services)
        where TEvent : class, IEvent, new()
    {
        services.AddSingleton<AsyncEventPublisher<TEvent>>();
        services.AddSingleton<IEventProducer<TEvent>>(sp =>
            sp.GetRequiredService<AsyncEventPublisher<TEvent>>());
        services.AddSingleton<IEventPublisher<TEvent>>(sp =>
            sp.GetRequiredService<AsyncEventPublisher<TEvent>>());
        return services;
    }
}
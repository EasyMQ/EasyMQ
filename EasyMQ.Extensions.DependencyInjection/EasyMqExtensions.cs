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
        _ = builder.GetFactory ?? throw new ArgumentNullException($"{nameof(builder.GetFactory)} cannot be null");
        var connectionFactory = builder.GetFactory(new ConnectionFactory());
        services.AddSingleton(connectionFactory);
        return services;
    }
    /// <summary>
    /// Registers a <see cref="TEvent"/> with a <see cref="THandler"/>.
    /// Internally it will spin up an <see cref="Microsoft.Extensions.Hosting.IHostedService"/>
    /// to consume external events as per the configurations provided.
    /// </summary>
    /// <param name="services"></param>
    /// <typeparam name="TEvent">Types that implement <see cref="IEvent"/></typeparam>
    /// <typeparam name="THandler">Types that implement <see cref="IEventHandler{TEvent}"/></typeparam>
    /// <returns></returns>
    public static IServiceCollection AddEventConsumer<TEvent, THandler>(this IServiceCollection services) 
        where TEvent : class, IEvent, new()
        where THandler: class, IEventHandler<TEvent>
    {
        services.AddTransient<IEventHandler<TEvent>, THandler>();
        services.AddSingleton<Func<IEventHandler<TEvent>>>(sp =>
        {
            var scope = sp.CreateScope();
            return scope.ServiceProvider.GetRequiredService<IEventHandler<TEvent>>;
        });
        services.AddSingleton<AsyncEventConsumer<TEvent>>();
        services.AddHostedService<ConsumerEventHost<AsyncEventConsumer<TEvent>>>();
        return services;
    }

    public static IServiceCollection AddEventProducer<TEvent>(this IServiceCollection services)
        where TEvent : class, IEvent, new()
    {
        services.AddTransient<AsyncEventPublisher<TEvent>>();
        services.AddTransient<IEventProducer<TEvent>>(sp =>
            sp.GetRequiredService<AsyncEventPublisher<TEvent>>());
        services.AddTransient<IEventPublisher<TEvent>>(sp =>
            sp.GetRequiredService<AsyncEventPublisher<TEvent>>());
        return services;
    }
}
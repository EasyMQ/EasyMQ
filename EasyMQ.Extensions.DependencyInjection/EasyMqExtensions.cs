using EasyMQ.Abstractions;
using EasyMQ.Consumer;
using EasyMQ.Consumer.Interfaces;
using EasyMQ.Consumers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RabbitMQ.Client;

namespace EasyMQ.Extensions.DependencyInjection;

public static class EasyMqExtensions
{
    public static IServiceCollection AddEasyMqConsumer(this IServiceCollection services, 
        Func<ConnectionFactory, IConnectionFactory> rmqConnectionFactory, IConfiguration configuration, string consumerSection)
    {
        services.Configure<ConsumerConfiguration>(_ => configuration.GetSection(consumerSection));
        services.TryAddSingleton<RabbitMqProvider>();
        services.TryAddSingleton<IConnectionProvider>(sp => sp.GetRequiredService<RabbitMqProvider>());
        services.TryAddSingleton<IRabbitMqProvider>(sp => sp.GetRequiredService<RabbitMqProvider>());
        
        _ = rmqConnectionFactory ?? throw new ArgumentNullException($"{nameof(rmqConnectionFactory)} cannot be null");
        var connectionFactory = rmqConnectionFactory(new ConnectionFactory());
        services.AddSingleton(connectionFactory);
        return services;
    }
    public static IServiceCollection AddEventConsumer<TEvent>(this IServiceCollection services) where TEvent : class, IEvent, new()
    {
        services.AddSingleton<EventConsumer<TEvent>>();
        services.AddHostedService<ConsumerEventHost<EventConsumer<TEvent>>>();
        return services;
    }
}
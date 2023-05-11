using EasyMQ.Abstractions;
using EasyMQ.Consumer;
using EasyMQ.Consumer.Interfaces;
using EasyMQ.Consumers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace EasyMQ.Extensions.DependencyInjection;

public static class EasyMqExtensions
{
    public static IServiceCollection AddEasyMqConsumer(this IServiceCollection services, 
        Func<ConnectionFactory, IConnectionFactory> rmqConnectionFactory, IConfiguration configuration, string consumerSection)
    {
        services.Configure<List<ConsumerConfiguration>>(configuration.GetSection(consumerSection));
        services.TryAddSingleton<RabbitMqProvider>();
        services.TryAddSingleton<IConnectionProvider>(sp =>
        {
            var provider = sp.GetRequiredService<RabbitMqProvider>();
            provider.InitializeConnection();
            return provider;
        });
        services.TryAddSingleton<IRabbitMqProvider>(sp => sp.GetRequiredService<RabbitMqProvider>());
        
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
}
using EasyMQ.Abstractions;
using EasyMQ.Abstractions.Producer;
using EasyMQ.Publisher;
using Microsoft.Extensions.DependencyInjection;

namespace EasyMQ.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEventProducer<TEvent>(this IServiceCollection services)
        where TEvent : class, IEvent, new()
    {
        services.AddTransient<AsyncEventPublisher<TEvent>>();
        services.AddTransient<IEventPublisher<TEvent>>(sp =>
            sp.GetRequiredService<AsyncEventPublisher<TEvent>>());
        return services;
    }
}
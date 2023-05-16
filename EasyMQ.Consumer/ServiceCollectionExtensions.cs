using EasyMQ.Abstractions;
using EasyMQ.Abstractions.Consumer;
using EasyMQ.Consumer;
using EasyMQ.Consumers;
using Microsoft.Extensions.DependencyInjection;

namespace EasyMQ.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
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
}
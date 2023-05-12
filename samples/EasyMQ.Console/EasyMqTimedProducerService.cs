using EasyMQ.Abstractions.Producer;
using Microsoft.Extensions.Hosting;

namespace EasyMQ.Console;
/// <summary>
/// A producer which keeps producing messages every 5 seconds to the exchange
/// </summary>
public class EasyMqTimedProducerService: IHostedService
{
    private readonly IEventPublisher<EasyMqEvent> _eventPublisher;
    private Timer? _timer;

    public EasyMqTimedProducerService(IEventPublisher<EasyMqEvent> eventPublisher)
    {
        _eventPublisher = eventPublisher;
    }
    public Task StartAsync(CancellationToken cancellationToken)
    {
        TimerCallback Callback()
        {
            return (object? state) =>
            {
                _eventPublisher.Publish(new EasyMqEvent()
                {
                    EventName = "Hello world"
                }, new ProducerContext()
                {
                    Mandatory = false,
                    RoutingKey = "test"
                }).GetAwaiter().GetResult();
            };
        }

        _timer = new Timer(Callback(), null, TimeSpan.Zero, TimeSpan.FromSeconds(2));
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Dispose();
        return Task.CompletedTask;
    }
}
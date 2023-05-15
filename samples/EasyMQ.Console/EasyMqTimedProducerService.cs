using EasyMQ.Abstractions.Producer;
using Microsoft.Extensions.Hosting;

namespace EasyMQ.Console;
/// <summary>
/// A producer which keeps producing messages every 5 seconds to the exchanges
/// </summary>
public class EasyMqTimedProducerService: IHostedService
{
    private readonly IEventPublisher<EasyMqTopicEvent> _topicEventPublisher;
    private readonly IEventPublisher<EasyMqHeaderEvent> _headerEventPublisher;
    private Timer? _timer;

    public EasyMqTimedProducerService(
        IEventPublisher<EasyMqTopicEvent> topicEventPublisher,
        IEventPublisher<EasyMqHeaderEvent> headerEventPublisher)
    {
        _topicEventPublisher = topicEventPublisher;
        _headerEventPublisher = headerEventPublisher;
    }
    public Task StartAsync(CancellationToken cancellationToken)
    {
        TimerCallback Callback()
        {
            return (object? state) =>
            {
                _topicEventPublisher.Publish(new EasyMqTopicEvent()
                {
                    EventName = "Topic Event"
                }, new ProducerContext()
                {
                    Mandatory = false,
                    RoutingKey = "test"
                }).GetAwaiter().GetResult();

                _headerEventPublisher.Publish(new EasyMqHeaderEvent()
                {
                    EventName = "Header Event"
                }, new ProducerContext()
                {
                    Mandatory = false,
                    Headers = new Dictionary<string, dynamic>()
                    {
                        {"EVENT_CODE", "header-evt-1"}
                    }
                });
            };
        }

        _timer = new Timer(Callback(), null, TimeSpan.Zero, TimeSpan.FromMilliseconds(500));
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Dispose();
        return Task.CompletedTask;
    }
}
using System;
using System.Threading.Tasks;
using Castle.Core.Logging;
using EasyMQ.Abstractions;
using EasyMQ.Abstractions.Consumer;
using Microsoft.Extensions.Logging;

namespace EasyMQ.E2E.Tests.TestHandlers;

public class TopicEvent : IEvent
{
    public string EventName { get; set; }
}

public class RetryTopicEvent : IEvent
{
    public string EventName { get; set; }
}

public class DelayedRetryEvent : IEvent
{
    public string EventName { get; set; }
}

public class TopicEventHandler : IEventHandler<TopicEvent>
{
    private readonly IFakeLogger _logger;

    public TopicEventHandler(IFakeLogger logger)
    {
        _logger = logger;
    }
    public Task Handle(ReceiverContext receiverContext, TopicEvent @event)
    {
        _logger.Log(@event.EventName);
        return Task.CompletedTask;
    }
}

public class TopicEventHandler2 : IEventHandler<TopicEvent>
{
    private readonly IFakeLogger _logger;

    public TopicEventHandler2(IFakeLogger logger)
    {
        _logger = logger;
    }
    public Task Handle(ReceiverContext receiverContext, TopicEvent @event)
    {
        _logger.Log(@event.EventName);
        return Task.CompletedTask;
    }
}

public class TestTopicErrorHandler : IEventHandler<RetryTopicEvent>
{
    private IFakeLogger _logger;

    public TestTopicErrorHandler(IFakeLogger logger)
    {
        _logger = logger;
    }
    public Task Handle(ReceiverContext receiverContext, RetryTopicEvent @event)
    {
        _logger.Log(@event.EventName);
        throw new NotImplementedException();
    }
}

public class DelayedRetryTopicErrorHandler : IEventHandler<DelayedRetryEvent>
{
    private readonly IFakeLogger _logger;

    public DelayedRetryTopicErrorHandler(IFakeLogger logger)
    {
        _logger = logger;
    }
    public Task Handle(ReceiverContext receiverContext, DelayedRetryEvent @event)
    {
        _logger.Log(@event.EventName);
        throw new NotImplementedException();
    }
}

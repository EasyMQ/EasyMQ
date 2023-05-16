using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using EasyMQ.Abstractions;
using EasyMQ.Abstractions.Consumer;
using EasyMQ.Consumers;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace EasyMQ.Consumer.Tests;

class TestHandler : IEventHandler<EventConsumerTests.TestEvent>
{
    public Task Handle(ReceiverContext receiverContext, EventConsumerTests.TestEvent @event)
    {
        throw new NotImplementedException();
    }
}
public class EventConsumerTests
{
    private readonly IEventHandler<TestEvent> _eventHandler;
    private readonly IOptions<List<ConsumerConfiguration>> _consumerConfigs;
    private readonly AsyncEventConsumer<TestEvent, TestHandler> _consumer;

    public class TestEvent : IEvent
    {
        public string EventName { get; set; }
    }

    public EventConsumerTests()
    {
        _eventHandler = Substitute.For<IEventHandler<TestEvent>>();
        var handlerFactory = () => _eventHandler;
        _consumerConfigs = Options.Create(new List<ConsumerConfiguration>()
        {
            new ConsumerConfiguration()
            {
                EventName = "TestEvent",
                EventHandlerName = "TestHandler"
            }
        });
        _consumer = new AsyncEventConsumer<TestEvent, TestHandler>(handlerFactory, _consumerConfigs);
    }

    [Fact]
    public void GivenAnEvent_ShouldGetRequisiteConfigurations()
    {
        _consumer.GetQueueAndExchangeConfiguration().Should().NotBeNull();
    }

    [Fact]
    public async Task GivenANewMessageContext_HandlerShouldBeInvoked()
    {
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new TestEvent()));
        await _consumer.Consume(new ReceiverContext(body,
            null, (ushort) body.Length, 0, null, null, false));

        await _eventHandler.Received(1).BeforeHandle(Arg.Any<ReceiverContext>(), Arg.Any<TestEvent>());
        await _eventHandler.Received(1).Handle(Arg.Any<ReceiverContext>(),Arg.Any<TestEvent>());
        await _eventHandler.Received(1).PostHandle(Arg.Any<ReceiverContext>(), Arg.Any<TestEvent>());
    } 
}
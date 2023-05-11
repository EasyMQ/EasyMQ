using System.Collections.Generic;
using System.Threading.Tasks;
using EasyMQ.Abstractions;
using EasyMQ.Consumers;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestPlatform.Common.Interfaces;
using NSubstitute;
using Xunit;

namespace EasyMQ.Consumer.Tests;

public class EventConsumerTests
{
    private readonly IEventHandler<TestEvent> _eventHandler;
    private readonly IOptions<List<ConsumerConfiguration>> _consumerConfigs;
    private readonly EventConsumer<TestEvent> _consumer;

    public class TestEvent : IEvent
    {
        public MessageContext Context { get; set; }
    }

    public EventConsumerTests()
    {
        _eventHandler = Substitute.For<IEventHandler<TestEvent>>();
        _consumerConfigs = Options.Create(new List<ConsumerConfiguration>()
        {
            new ConsumerConfiguration()
            {
                EventType = "TestEvent",
            }
        });
        _consumer = new EventConsumer<TestEvent>(_eventHandler, _consumerConfigs);
    }

    [Fact]
    public void GivenAnEvent_ShouldGetRequisiteConfigurations()
    {
        _consumer.GetQueueAndExchangeConfiguration().Should().NotBeNull();
    }

    [Fact]
    public async Task GivenANewMessageContext_HandlerShouldBeInvoked()
    {
        await _consumer.Consume(new MessageContext(null, null, 0, 0, null, null, false));

        await _eventHandler.Received(1).Handle(Arg.Any<MessageContext>(),Arg.Any<TestEvent>());
    } 
}
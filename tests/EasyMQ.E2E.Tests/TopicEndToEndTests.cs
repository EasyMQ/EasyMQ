using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EasyMQ.Abstractions.Producer;
using EasyMQ.Consumer;
using EasyMQ.Consumers;
using EasyMQ.E2E.Tests.TestHandlers;
using EasyMQ.Extensions.DependencyInjection;
using EasyMQ.Publisher;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace EasyMQ.E2E.Tests;

public class TopicEndToEndTests: Fixture
{
    private IFakeLogger _topicLogger;

    protected override void AddServices(IServiceCollection services, IConfigurationRoot configurationRoot)
    {
        _topicLogger = Substitute.For<IFakeLogger>();
        services
            .AddEventConsumer<TopicEvent, TopicEventHandler>()
            .AddEventProducer<TopicEvent>()
            .AddSingleton(_topicLogger)
            .AddTransient<ILogger<ConsumerEventHost<AsyncEventConsumer<TopicEvent>>>>(sp =>
                Substitute.For<ILogger<ConsumerEventHost<AsyncEventConsumer<TopicEvent>>>>());
        
        base.AddServices(services, configurationRoot);
    }

    [Fact]
    public async Task ForATopicEvent_WhenIPublishAnEvent_TheTopicEventHandlerShouldGetInvoked()
    {
        await Given<IEventPublisher<TopicEvent>>(i =>
        {
            i.Publish(new TopicEvent() {EventName = "test"},
                new ProducerContext() {RoutingKey = "test", Mandatory = false});
            
            return Task.CompletedTask;
        });
        
        Thread.Sleep(500);
        
        await Then<IFakeLogger>(i =>
        {
            i.Received(1).Log(Arg.Any<string>());
            return Task.CompletedTask;
        });
    }
    
    [Fact]
    public async Task ForATopicEvent_WhenIPublishAnEventWithAnUnknownRoutingKey_TheTopicEventHandlerShouldNotGetInvoked()
    {
        await Given<IEventPublisher<TopicEvent>>(i =>
        {
            i.Publish(new TopicEvent() {EventName = "test"},
                new ProducerContext() {RoutingKey = "not_test", Mandatory = false});
            
            return Task.CompletedTask;
        });
        
        Thread.Sleep(500);
        
        await Then<IFakeLogger>(i =>
        {
            i.DidNotReceive().Log(Arg.Any<string>());
            return Task.CompletedTask;
        });
    }
}
using System;
using System.Threading;
using System.Threading.Tasks;
using EasyMQ.Abstractions.Producer;
using EasyMQ.Consumer;
using EasyMQ.Consumers;
using EasyMQ.E2E.Tests.TestHandlers;
using EasyMQ.Extensions.DependencyInjection;
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
            .AddEventConsumer<TopicEvent, TopicEventHandler2>()
            .AddEventProducer<TopicEvent>()
            .AddSingleton(_topicLogger)
            .AddTransient<ILogger<ConsumerEventHost<AsyncEventConsumer<TopicEvent, TopicEventHandler>>>>(sp =>
                Substitute.For<ILogger<ConsumerEventHost<AsyncEventConsumer<TopicEvent, TopicEventHandler>>>>())
            .AddTransient<ILogger<ConsumerEventHost<AsyncEventConsumer<TopicEvent, TopicEventHandler2>>>>(sp =>
                Substitute.For<ILogger<ConsumerEventHost<AsyncEventConsumer<TopicEvent, TopicEventHandler2>>>>());
        
        base.AddServices(services, configurationRoot);
    }
    [Fact]
    public async Task ForATopicEvent_WhenIPublishAnEvent_TheTopicEventHandlerShouldGetInvoked()
    {
        await Given<IEventPublisher<TopicEvent>>(async i =>
        {
            await i.PublishAsync(new TopicEvent() {EventName = "test"},
                new ProducerContext() {RoutingKey = "test", Mandatory = false});
            // i.Publish(new TopicEvent() {EventName = "test"}, new ProducerContext() {RoutingKey = "test2"});
        });
        
        Thread.Sleep(500);
        
        await Then<IFakeLogger>(i =>
        {
            i.Received(1).Log(Arg.Any<string>());
            return Task.CompletedTask;
        });
    }

    [Fact]
    public async Task ForATopicEvent_WhenIPublishAnEventWithTwoRoutingKeys_BothTopicEventHandlersShouldGetInvoked()
    {
        await Given<IEventPublisher<TopicEvent>>(async i =>
        {
            await i.PublishAsync(new TopicEvent() {EventName = "test"},
                new ProducerContext() {RoutingKey = "test", Mandatory = false});
            await i.PublishAsync(new TopicEvent() {EventName = "test"}, new ProducerContext() {RoutingKey = "test2"});
        });
        
        Thread.Sleep(500);
        
        await Then<IFakeLogger>(i =>
        {
            i.Received(2).Log(Arg.Any<string>());
            i.ClearReceivedCalls();
            return Task.CompletedTask;
        });
    }
    
    [Fact]
    public async Task ForATopicEvent_WhenIPublishAnEventWithAnUnknownRoutingKey_TheTopicEventHandlerShouldNotGetInvoked()
    {
        await Given<IEventPublisher<TopicEvent>>(async i =>
        {
            await i.PublishAsync(new TopicEvent() {EventName = "test"},
                new ProducerContext() {RoutingKey = "not_test", Mandatory = false});
        });
        
        Thread.Sleep(500);
        
        await Then<IFakeLogger>(i =>
        {
            i.DidNotReceive().Log(Arg.Any<string>());
            i.ClearReceivedCalls();
            return Task.CompletedTask;
        });
    }
}
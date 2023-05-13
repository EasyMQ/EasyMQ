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

public class EndToEndTests: Fixture
{
    private readonly IFakeLogger _topicLogger;

    public EndToEndTests()
    {
        _topicLogger = Substitute.For<IFakeLogger>();
    }
    protected override void AddServices(IServiceCollection services, IConfigurationRoot configurationRoot)
    {
        services
            .AddEventConsumer<TopicEvent, TopicEventHandler>()
            .AddEventProducer<TopicEvent>()
            .AddSingleton<IFakeLogger>(sp => _topicLogger)
            .AddTransient<ILogger<AsyncEventPublisher<TopicEvent>>>(sp =>
                Substitute.For<ILogger<AsyncEventPublisher<TopicEvent>>>())
            .AddTransient<ILogger<RabbitMqProvider>>(sp => Substitute.For<ILogger<RabbitMqProvider>>())
            .AddTransient<ILogger<ConsumerEventHost<AsyncEventConsumer<TopicEvent>>>>(sp =>
                Substitute.For<ILogger<ConsumerEventHost<AsyncEventConsumer<TopicEvent>>>>());
        
        base.AddServices(services, configurationRoot);
    }

    [Fact]
    public async Task ForATopicEvent_WhenIPublishAnEvent_TheTopicEventHandlerShouldGetInvoked()
    {
        await When<IEventPublisher<TopicEvent>>(i =>
        {
            i.Publish(new TopicEvent() {EventName = "test"},
                new ProducerContext() {RoutingKey = "test", Mandatory = false});
            
            return Task.CompletedTask;
        });
        
        Thread.Sleep(1000);
        
        await Then<IFakeLogger>(i =>
        {
            i.Received(1).Log(Arg.Any<string>());
            return Task.CompletedTask;
        });
    }
}
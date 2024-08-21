using System.Collections.Generic;
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

public class HeaderEndToEndTests: Fixture
{
    private IFakeLogger _topicLogger;

    protected override void AddServices(IServiceCollection services, IConfigurationRoot configurationRoot)
    {
        _topicLogger = Substitute.For<IFakeLogger>();
        services
            .AddEventConsumer<HeaderEvent, HeaderEventHandler>()
            .AddEventProducer<HeaderEvent>()
            .AddSingleton(_topicLogger)
            .AddTransient(sp =>
                Substitute.For<ILogger<ConsumerEventHost<AsyncEventConsumer<HeaderEvent, HeaderEventHandler>>>>());
        
        base.AddServices(services, configurationRoot);
    }
    
    [Fact]
    public async Task ForAHeaderEvent_WhenIPublishAnEventWith_ThenHeaderEventHandlerShouldGetInvoked()
    {
        await Given<IEventPublisher<HeaderEvent>>(i =>
        {
            i.PublishAsync(new HeaderEvent() {EventName = "headerEvent"}, new ProducerContext()
            {
                Mandatory = false,
                Headers = new Dictionary<string, dynamic>()
                {
                    {"EVENT_CODE", "header-evt-1"}
                }
            });
            return Task.CompletedTask;
        });
        
        Thread.Sleep(400);
        
        await Then<IFakeLogger>(i =>
        {
            i.Received(1).Log(Arg.Any<string>());
            return Task.CompletedTask;
        });
    }

    [Fact]
    public async Task ForAHeaderEvent_WhenIPublishAnEventWithInvalidHeaders_ThenHeaderEventHandlerShouldNotBeInvoked()
    {
        await Given<IEventPublisher<HeaderEvent>>(i =>
        {
            i.PublishAsync(new HeaderEvent() {EventName = "headerEvent"}, new ProducerContext()
            {
                Mandatory = false,
                Headers = new Dictionary<string, dynamic>()
                {
                    {"EVENT_CODE", "header-evt-invalid"}
                }
            });
            return Task.CompletedTask;
        });
        
        Thread.Sleep(400);
        
        await Then<IFakeLogger>(i =>
        {
            i.DidNotReceive().Log(Arg.Any<string>());
            return Task.CompletedTask;
        });
    }
}
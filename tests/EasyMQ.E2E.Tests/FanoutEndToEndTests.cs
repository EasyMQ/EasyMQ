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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EasyMQ.E2E.Tests
{
    public class FanoutEndToEndTests: Fixture
    {
        private IFakeLogger _fanoutLogger;

        protected override void AddServices(IServiceCollection services, IConfigurationRoot configurationRoot)
        {
            _fanoutLogger = Substitute.For<IFakeLogger>();
            services
                .AddEventProducer<FanoutEvent>()
                .AddEventConsumer<FanoutEvent, FanoutEventHandler1>()
                .AddEventConsumer<FanoutEvent, FanoutEventHandler2>()
                .AddEventConsumer<FanoutEvent, FanoutEventHandler3>()
                .AddSingleton(_fanoutLogger)
                .AddTransient(sp =>
                    Substitute.For<ILogger<AsyncEventPublisher<FanoutEvent>>>())
                .AddTransient(sp =>
                    Substitute.For<ILogger<ConsumerEventHost<AsyncEventConsumer<FanoutEvent, FanoutEventHandler1>>>>())
                .AddTransient(sp =>
                    Substitute.For<ILogger<ConsumerEventHost<AsyncEventConsumer<FanoutEvent, FanoutEventHandler2>>>>())
                .AddTransient(sp =>
                    Substitute.For<ILogger<ConsumerEventHost<AsyncEventConsumer<FanoutEvent, FanoutEventHandler3>>>>());

            base.AddServices(services, configurationRoot);
        }

        [Fact]
        public async Task ForAFanoutEvent_AllTheRegisteredHandlersShouldBeInvoked()
        {
            await Given<IEventPublisher<FanoutEvent>>(i =>
            {
                i.PublishAsync(new FanoutEvent() { EventName = "fanout-evt" }, new ProducerContext() { Mandatory = false});
                return Task.CompletedTask;
            });

            Thread.Sleep(800);

            await Then<IFakeLogger>(i =>
            {
                i.Received(3).Log(Arg.Any<string>());
                return Task.CompletedTask;
            });
        }
    }
}

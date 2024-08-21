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
    public class DirectEndToEndTests: Fixture
    {
        private IFakeLogger _fakeLogger;

        public DirectEndToEndTests()
        {
        }

        protected override void AddServices(IServiceCollection services, IConfigurationRoot configurationRoot)
        {
            _fakeLogger = Substitute.For<IFakeLogger>();
            services
                .AddEventConsumer<DirectEvent, DirectEventHandler>()
                .AddEventProducer<DirectEvent>()
                .AddSingleton(_fakeLogger)
                .AddTransient(sp => Substitute.For<ILogger<AsyncEventPublisher<DirectEvent>>>())
                .AddTransient(sp => Substitute.For<ILogger<ConsumerEventHost<AsyncEventConsumer<DirectEvent, DirectEventHandler>>>>());

            base.AddServices(services, configurationRoot);
        }

        [Fact]
        public async Task ForADirectExchangeWhenAMessageIsPublishedWithAnEmptyRKey_ThatHasAQueueWithWildCardBindingAndAQueueWithout_BothShouldConsumeTheSameMessage()
        {
            await Given<IEventPublisher<DirectEvent>>(i =>
            {
                return i.PublishAsync(new DirectEvent { EventName = "direct_event" }, new ProducerContext());
            });

            Thread.Sleep(400);

            await Then<IFakeLogger>(i =>
            {
                i.Received(1).Log(Arg.Any<string>());
                return Task.CompletedTask;
            });
        }
    }
}

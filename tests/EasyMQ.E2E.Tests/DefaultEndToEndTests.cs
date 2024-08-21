
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
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EasyMQ.E2E.Tests
{
    public class DefaultEndToEndTests: Fixture
    {
        private IFakeLogger _fakeLogger;

        protected override void AddServices(IServiceCollection services, IConfigurationRoot configurationRoot)
        {
            _fakeLogger = Substitute.For<IFakeLogger>();
            services
                .AddEventConsumer<DefaultEvent, DefaultEventHandler>()
                .AddEventProducer<DefaultEvent>()
                .AddSingleton(_fakeLogger)
                .AddTransient(sp => Substitute.For<ILogger<AsyncEventPublisher<DefaultEvent>>>())
                .AddTransient(sp => Substitute.For<ILogger<ConsumerEventHost<AsyncEventConsumer<DefaultEvent, DefaultEventHandler>>>>());

            base.AddServices(services, configurationRoot);
        }

        [Fact]
        public async Task ForADefaultExchange_MessagePublishedWithTheRoutingKey_ShouldEndUpInTheQueue()
        {
            await Given<IEventPublisher<DefaultEvent>>(i =>
            {
                return i.PublishAsync(new DefaultEvent { EventName = "default-event" }, new ProducerContext());
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

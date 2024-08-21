using EasyMQ.Abstractions;
using EasyMQ.Abstractions.Consumer;
using System.Threading.Tasks;

namespace EasyMQ.E2E.Tests.TestHandlers
{
    public class DirectEvent : IEvent
    {
        public string EventName { get; set; }
    }
    public class DirectEventHandler : IEventHandler<DirectEvent>
    {
        private IFakeLogger _logger;

        public DirectEventHandler(IFakeLogger logger)
        {
            _logger = logger;
        }
        public Task Handle(ReceiverContext receiverContext, DirectEvent @event)
        {
            _logger.Log(@event.EventName);
            return Task.CompletedTask;
        }
    }
}

using EasyMQ.Abstractions;
using EasyMQ.Abstractions.Consumer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyMQ.E2E.Tests.TestHandlers
{
    public class DefaultEvent: IEvent
    {
        public string EventName { get; set; }
    }
    public class DefaultEventHandler : IEventHandler<DefaultEvent>
    {
        private IFakeLogger _logger;

        public DefaultEventHandler(IFakeLogger fakeLogger)
        {
            _logger = fakeLogger;
        }
        public Task Handle(ReceiverContext receiverContext, DefaultEvent @event)
        {
            _logger.Log(@event.EventName);
            return Task.CompletedTask;
        }
    }
}

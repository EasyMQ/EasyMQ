using EasyMQ.Abstractions;
using EasyMQ.Abstractions.Consumer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyMQ.E2E.Tests.TestHandlers
{
    public class FanoutEvent: IEvent
    {
        public string EventName { get; set; }
    }
    public class FanoutEventHandler1 : IEventHandler<FanoutEvent>
    {
        private readonly IFakeLogger _logger;

        public FanoutEventHandler1(IFakeLogger logger)
        {
            _logger = logger;
        }

        public Task Handle(ReceiverContext receiverContext, FanoutEvent @event)
        {
            _logger.Log(@event.EventName);
            return Task.CompletedTask;
        }
    }

    public class FanoutEventHandler2 : IEventHandler<FanoutEvent>
    {
        private readonly IFakeLogger _logger;

        public FanoutEventHandler2(IFakeLogger logger)
        {
            _logger = logger;
        }

        public Task Handle(ReceiverContext receiverContext, FanoutEvent @event)
        {
            _logger.Log(@event.EventName);
            return Task.CompletedTask;
        }
    }

    public class FanoutEventHandler3 : IEventHandler<FanoutEvent>
    {
        private readonly IFakeLogger _logger;

        public FanoutEventHandler3(IFakeLogger logger)
        {
            _logger = logger;
        }

        public Task Handle(ReceiverContext receiverContext, FanoutEvent @event)
        {
            _logger.Log(@event.EventName);
            return Task.CompletedTask;
        }
    }
}

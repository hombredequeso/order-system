using System;
using Hdq.OrderApi.Messages;
using Hdq.RestBus.Subscriber;
using Hdq.TestService.Events;
using Hdq.Statistics.Handlers;

namespace Hdq.Statistics
{
    public static class HandlerFactory
    {
        public static Action<DomainMessageProcessingContext, object> GetHandlerForMessageType(
            Type messageType)
        {
            if (messageType == typeof(SomethingHappenedEvent))
                return (c,m) => new WidgetizeWhenSomethingHappenedEventHandler().Handle((SomethingHappenedEvent) m);
            if (messageType == typeof(OrderPlacedEvent))
                return (c, m) => Handlers.HandlerFactory.GetOrderPlacedHandler()(c, (OrderPlacedEvent) m);
            return (c,m) => { };
        }
    }
}
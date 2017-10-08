using System;
using CarrierPidgin.OrderService.Messages;
using Hdq.RestBus.Receiver;
using CarrierPidgin.ServiceA.Handlers;
using CarrierPidgin.TestService.Events;

namespace CarrierPidgin.ServiceA
{
    public static class HandlerFactory
    {
        public static Action<DomainMessageProcessor.DomainMessageProcessingContext, object> GetHandlerForMessageType(
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
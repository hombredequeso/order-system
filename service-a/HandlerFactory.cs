using System;
using CarrierPidgin.OrderService.Messages;
using CarrierPidgin.ServiceA.TestDomain;
using CarrierPidgin.TestService.Events;

namespace CarrierPidgin.ServiceA
{
    public static class HandlerFactory
    {
        public static Action<DomainMessageProcessor.DomainMessageProcessingContext, object> GetForMessageType(Type messageType)
        {
            if (messageType == typeof(SomethingHappenedEvent))
                return (c,m) => new WidgetizeWhenSomethingHappenedEventHandler().Handle((SomethingHappenedEvent) m);
            if (messageType == typeof(OrderPlacedEvent))
                return (c, m) => Statistics.HandlerFactory.GetOrderPlacedHandler()(c, (OrderPlacedEvent) m);
            return (c,m) => { };
        }
    }
}
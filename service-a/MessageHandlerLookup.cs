using System;
using CarrierPidgin.OrderService.Messages;
using CarrierPidgin.ServiceA.Statistics;
using CarrierPidgin.ServiceA.TestDomain;
using CarrierPidgin.TestService.Events;

namespace CarrierPidgin.ServiceA
{
    public static class MessageHandlerLookup
    {
        public static Action<DomainMessageProcessor.DomainMessageProcessingContext, object> GetMessageHandler(Type messageType)
        {
            if (messageType == typeof(SomethingHappenedEvent))
                return (c,m) => new WidgetizeWhenSomethingHappenedEventHandler().Handle((SomethingHappenedEvent) m);
            if (messageType == typeof(OrderPlacedEvent))
                return (c, m) => HandleConstruction.GetOrderPlacedHandler()(c, (OrderPlacedEvent) m);
            return (c,m) => { };
        }
    }
}
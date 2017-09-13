using System;
using CarrierPidgin.OrderService.Messages;
using CarrierPidgin.ServiceA.Statistics;
using CarrierPidgin.ServiceA.TestDomain;
using CarrierPidgin.TestService.Events;

namespace CarrierPidgin.ServiceA
{
    public static class MessageHandlerLookup
    {
        public static Action<object> GetMessageHandler(Type messageType)
        {
            if (messageType == typeof(SomethingHappenedEvent))
                return e => new WidgetizeWhenSomethingHappenedEventHandler().Handle((SomethingHappenedEvent) e);
            if (messageType == typeof(OrderPlacedEvent))
                return m => HandleConstruction.GetHandlerWithDeDup2()((OrderPlacedEvent) m);
            return e => { };
        }
    }
}
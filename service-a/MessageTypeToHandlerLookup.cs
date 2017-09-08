using System;
using System.Collections.Generic;
using CarrierPidgin.OrderService.Messages;
using CarrierPidgin.ServiceA.Statistics;
using CarrierPidgin.ServiceA.TestDomain;
using CarrierPidgin.TestService.Events;

namespace CarrierPidgin.ServiceA
{
    public static class MessageTypeToHandlerLookup
    {
        public static List<object> GetHandler(Type messageType)
        {
            if (messageType == typeof(SomethingHappenedEvent))
                return new List<object> {new WidgetizeWhenSomethingHappenedEventHandler()};
            if (messageType == typeof(OrderPlacedEvent))
                return new List<object> {new AddToStatsWhenOrderPlacedHandler()};
            return new List<object>();
        }
    }
}
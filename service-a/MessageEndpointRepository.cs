using System;
using System.Collections.Generic;
using System.Linq;
using Hdq.RestBus;
using CarrierPidgin.OrderService.Messages;
using Hdq.RestBus.Receiver;
using CarrierPidgin.ServiceA.OrderDomain;
using CarrierPidgin.ServiceA.TestDomain;
using CarrierPidgin.TestService.Events;

namespace CarrierPidgin.ServiceA
{
    public static class MessageEndpointRepository
    {
        public static List<MessageEndpoint> GetAll(UnitOfWork statisticsUow)
        {
            return TestEndpointRepository.GetAll()
                .Concat(OrderEndpointRepository.GetAll(statisticsUow))
                .ToList();
        }

        public static Dictionary<string, Type> GetMessageTypeLookup()
        {
            var allDomainMessageTypeLookup = new Dictionary<string, Type>();
            foreach (var keyValuePair in TestEvents.MessageTypeLookup)
            {
                allDomainMessageTypeLookup.Add(keyValuePair.Value, keyValuePair.Key);
            }
            foreach (var kvp in OrderEvents.MessageTypeLookup)
            {
                allDomainMessageTypeLookup.Add(kvp.Value, kvp.Key);
            }
            return allDomainMessageTypeLookup;
        }
    }
}
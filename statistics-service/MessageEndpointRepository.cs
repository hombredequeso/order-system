using System;
using System.Collections.Generic;
using System.Linq;
using Hdq.RestBus;
using Hdq.OrderApi.Messages;
using Hdq.RestBus.Subscriber;
using Hdq.TestService.Events;
using Hdq.Statistics.OrderDomain;
using Hdq.Statistics.TestDomain;

namespace Hdq.Statistics
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
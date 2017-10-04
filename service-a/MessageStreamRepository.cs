using System;
using System.Collections.Generic;
using System.Linq;
using CarrierPidgin.Lib;
using CarrierPidgin.OrderService.Messages;
using CarrierPidgin.ServiceA.Bus;
using CarrierPidgin.ServiceA.OrderChannels;
using CarrierPidgin.ServiceA.TestChannels;
using CarrierPidgin.TestService.Events;

namespace CarrierPidgin.ServiceA
{
    public static class MessageStreamRepository
    {
        public static List<MessageStream> GetAllMessageStreams(UnitOfWork statisticsUow)
        {
            return TestStreamRepository.GetAll()
                .Concat(OrderStreamRepository.GetAll(statisticsUow))
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
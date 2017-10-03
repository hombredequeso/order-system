using System;
using System.Collections.Generic;

namespace CarrierPidgin.ServiceA.Bus
{
    public class MessageProcessingData
    {
        public MessageProcessingData(
            Dictionary<string, Type> messageTypeLookup, 
            Func<Type, Action<DomainMessageProcessor.DomainMessageProcessingContext, object>> domainMessageProcessorLookup)
        {
            MessageTypeLookup = messageTypeLookup;
            DomainMessageProcessorLookup = domainMessageProcessorLookup;
        }

        public Dictionary<string, Type> MessageTypeLookup { get; }
        public Func<Type, Action<DomainMessageProcessor.DomainMessageProcessingContext, object>> DomainMessageProcessorLookup
        {
            get;
        }
    }
}
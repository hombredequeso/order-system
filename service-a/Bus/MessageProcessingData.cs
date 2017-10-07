using System;
using CarrierPidgin.Lib;
using Hdq.Lib;

namespace CarrierPidgin.ServiceA.Bus
{
    public class MessageProcessingData
    {
        public MessageProcessingData(
            Func<Type, Action<DomainMessageProcessor.DomainMessageProcessingContext, object>> domainMessageProcessorLookup, 
            Func<string, Either<DeserializeError, TransportMessage>> deserializeTransportMessage)
        {
            DomainMessageProcessorLookup = domainMessageProcessorLookup;
            DeserializeTransportMessage = deserializeTransportMessage;
        }

        public Func<Type, Action<DomainMessageProcessor.DomainMessageProcessingContext, object>> DomainMessageProcessorLookup
        {
            get;
        }

        public Func<string, Either<DeserializeError, TransportMessage>> DeserializeTransportMessage { get; }
    }
}
using System.Collections.Generic;
using Hdq.RestBus;
using Hdq.RestBus.Receiver;
using CarrierPidgin.ServiceA.Handlers;

namespace CarrierPidgin.ServiceA.OrderDomain
{
    public static class OrderEndpointRepository
    {
        public static List<MessageEndpoint> GetAll(UnitOfWork uow)
        {
            var messageEndpointName = new MessageEndpointName("orderdomain/order stream #1");
            var lastMessageProcessed = MessageNumberRepository.GetLastProcessedMessageNumber(
                uow,
                messageEndpointName);

            var lmp = lastMessageProcessed.Match(x => x.Item1, () => MessageEndpoint.NoMessagesProcessed);

            return new List<MessageEndpoint>()
            {
                new MessageEndpoint(messageEndpointName,
                "eventstream/orderdomain/order/0,9",
                lmp, 
                PollingPolicy.DefaultDelayMs * 5, 
                PollingPolicy.DefaultPollingErrorPolicy)
            };
        }
        
    }
}
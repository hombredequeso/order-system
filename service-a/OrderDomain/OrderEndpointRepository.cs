using System.Collections.Generic;
using Hdq.RestBus;
using Hdq.RestBus.Receiver;
using CarrierPidgin.ServiceA.Handlers;

namespace CarrierPidgin.ServiceA.OrderDomain
{
    public static class OrderChannel
    {
        public static HttpChannelBase GetBase()
        {
            return new HttpChannelBase("http", "localhost", 8080);
        }
        
    }
    public static class OrderEndpointRepository
    {
        public static List<MessageEndpoint> GetAll(UnitOfWork uow)
        {
            var channelBase = OrderChannel.GetBase();
            var messageEndpointName = new MessageEndpointName("orderdomain/order stream #1");
            var lastMessageProcessed = MessageNumberRepository.GetLastProcessedMessageNumber(
                uow,
                messageEndpointName);

            var lmp = lastMessageProcessed.Match(x => x.Item1, () => MessageEndpoint.NoMessagesProcessed);

            return new List<MessageEndpoint>()
            {
                new MessageEndpoint(
                    messageEndpointName,
                    new HttpChannel(
                        channelBase,
                        "eventstream/orderdomain/order/0,9"),
                lmp, 
                PollingDelays.DefaultDelayMs * 5, 
                PollingDelays.DefaultPollingErrorDelays)
            };
        }
        
    }
}
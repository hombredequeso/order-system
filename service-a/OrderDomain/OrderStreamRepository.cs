using System.Collections.Generic;
using CarrierPidgin.Lib;
using CarrierPidgin.ServiceA.Bus;
using CarrierPidgin.ServiceA.Statistics;

namespace CarrierPidgin.OrderService.Messages
{
    public static class OrderStreamRepository
    {
        public static List<MessageStream> Get(UnitOfWork uow)
        {
            var messageStreamName = "orderdomain/order stream #1";
            var lastMessageProcessed = MessageNumberRepository.GetLastProcessedMessageNumber(
                uow,
                new MessageStreamName(messageStreamName));

            var lmp = lastMessageProcessed.Match(x => x.Item1, () => MessageStream.NoMessagesProcessed);

            return new List<MessageStream>()
            {
                new MessageStream(
                    "eventstream/orderdomain/order/0,9", 
                    lmp,
                    "orderdomain/order stream #1",
                    PollingPolicy.DefaultDelayMs * 5,
                    PollingPolicy.DefaultPollingErrorPolicy)
            };
        }
        
    }
}
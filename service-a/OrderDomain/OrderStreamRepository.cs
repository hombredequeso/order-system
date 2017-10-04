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
            var messageStreamName = new MessageStreamName("orderdomain/order stream #1");
            var lastMessageProcessed = MessageNumberRepository.GetLastProcessedMessageNumber(
                uow,
                messageStreamName);

            var lmp = lastMessageProcessed.Match(x => x.Item1, () => MessageStream.NoMessagesProcessed);

            return new List<MessageStream>()
            {
                new MessageStream(
                    "eventstream/orderdomain/order/0,9", 
                    lmp,
                    messageStreamName,
                    PollingPolicy.DefaultDelayMs * 5,
                    PollingPolicy.DefaultPollingErrorPolicy)
            };
        }
        
    }
}
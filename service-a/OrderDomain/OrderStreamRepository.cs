using System.Collections.Generic;
using CarrierPidgin.Lib;
using CarrierPidgin.ServiceA.Bus;
using CarrierPidgin.ServiceA.Handlers;

namespace CarrierPidgin.ServiceA.OrderDomain
{
    public static class OrderStreamRepository
    {
        public static List<MessageStream> GetAll(UnitOfWork uow)
        {
            var messageStreamName = new MessageStreamName("orderdomain/order stream #1");
            var lastMessageProcessed = MessageNumberRepository.GetLastProcessedMessageNumber(
                uow,
                messageStreamName);

            var lmp = lastMessageProcessed.Match(x => x.Item1, () => MessageStream.NoMessagesProcessed);

            return new List<MessageStream>()
            {
                new MessageStream(messageStreamName,
                "eventstream/orderdomain/order/0,9",
                lmp, 
                PollingPolicy.DefaultDelayMs * 5, 
                PollingPolicy.DefaultPollingErrorPolicy)
            };
        }
        
    }
}
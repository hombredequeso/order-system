using System.Collections.Generic;

namespace CarrierPidgin.ServiceA
{
    public static class MessageStreamRepository
    {
        public static Dictionary<Poller.PollingError, uint> DefaultPollingErrorPolicy = new Dictionary<Poller.PollingError, uint>()
        {
            { Poller.PollingError.UnableToConnect, 5000 },
            { Poller.PollingError.ErrorDeserializingContent, 10000 },
            { Poller.PollingError.ErrorMakingHttpRequest, 10000 },
            { Poller.PollingError.UnknownErrorOnGet, 10000 }
        };
        public static uint DefaultDelayMs = 1000;


        public static List<OrderedMessageStream> Get()
        {
            return new List<OrderedMessageStream>()
            {
                new OrderedMessageStream(
                    "teststream", 
                    OrderedMessageStream.NoMessagesProcessed, 
                    "TestStream #1",
                    DefaultDelayMs,
                    DefaultPollingErrorPolicy),
                new OrderedMessageStream(
                    "eventstream/orderdomain/order/0,9", 
                    OrderedMessageStream.NoMessagesProcessed, 
                    "orderdomain/order stream #1",
                    DefaultDelayMs * 5,
                    DefaultPollingErrorPolicy)
            };

            // Add test stream a second time:

            // new OrderedMessageStream(
            //     "teststream/0,9", 
            //     OrderedMessageStream.NoMessagesProcessed, 
            //     "TestStream #2",
            //     DefaultDelayMs * 5,
            //     DefaultPollingErrorPolicy)
        }
    }
}
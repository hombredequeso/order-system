using System.Collections.Generic;
using CarrierPidgin.ServiceA.Bus;
using CarrierPidgin.ServiceA.Bus.Sample;

namespace CarrierPidgin.ServiceA
{
    public static class TestMessageStreamRepository
    {
        public static Dictionary<HttpMessagePoller.PollingError, uint> DefaultPollingErrorPolicy = new Dictionary<HttpMessagePoller.PollingError, uint>()
        {
            { HttpMessagePoller.PollingError.UnableToConnect, 5000 },
            { HttpMessagePoller.PollingError.ErrorDeserializingContent, 10000 },
            { HttpMessagePoller.PollingError.ErrorMakingHttpRequest, 10000 },
            { HttpMessagePoller.PollingError.UnknownErrorOnGet, 10000 }
        };
        public static uint DefaultDelayMs = 1000;

        public static List<TestOrderedMessageStream> Get()
        {
            return new List<TestOrderedMessageStream>()
            {
                new TestOrderedMessageStream(
                    "teststream", 
                    TestOrderedMessageStream.NoMessagesProcessed, 
                    "TestStream #1",
                    DefaultDelayMs,
                    DefaultPollingErrorPolicy),
                new TestOrderedMessageStream(
                    "eventstream/orderdomain/order/0,9", 
                    // TODO: this next line should come from the db.
                    TestOrderedMessageStream.NoMessagesProcessed, 
                    "orderdomain/order stream #1",
                    DefaultDelayMs * 5,
                    DefaultPollingErrorPolicy)
            };
        }
    }
}
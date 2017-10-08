using System.Collections.Generic;
using Hdq.RestBus.Receiver;

namespace CarrierPidgin.ServiceA.TestDomain
{
    public static class TestEndpointRepository
    {
        public static List<MessageEndpoint> GetAll()
        {
            return new List<MessageEndpoint>()
            {
                new MessageEndpoint(
                    new MessageEndpointName("TestStream #1"),
                    "teststream",
                    MessageEndpoint.NoMessagesProcessed, 
                    PollingPolicy.DefaultDelayMs, 
                    PollingPolicy.DefaultPollingErrorPolicy)
            };
        }
    }
}
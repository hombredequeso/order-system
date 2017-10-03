using System.Collections.Generic;
using CarrierPidgin.ServiceA.Bus;

namespace CarrierPidgin.ServiceA.TestDomain
{
    public static class TestStreamRepository
    {
        public static List<MessageStream> Get()
        {
            return new List<MessageStream>()
            {
                new MessageStream(
                    "teststream", 
                    MessageStream.NoMessagesProcessed, 
                    "TestStream #1",
                    PollingPolicy.DefaultDelayMs,
                    PollingPolicy.DefaultPollingErrorPolicy)
            };
        }
        
    }
}
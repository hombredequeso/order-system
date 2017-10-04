using System.Collections.Generic;
using CarrierPidgin.ServiceA.Bus;

namespace CarrierPidgin.ServiceA.TestChannels
{
    public static class TestStreamRepository
    {
        public static List<MessageStream> GetAll()
        {
            return new List<MessageStream>()
            {
                new MessageStream(
                    new MessageStreamName("TestStream #1"),
                    "teststream",
                    MessageStream.NoMessagesProcessed, 
                    PollingPolicy.DefaultDelayMs, 
                    PollingPolicy.DefaultPollingErrorPolicy)
            };
        }
    }
}
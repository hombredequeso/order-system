using System.Collections.Generic;
using Hdq.RestBus.Subscriber;

namespace Hdq.Statistics.TestDomain
{
    public static class TestChannel
    {
        public static HttpChannelBase GetBase()
        {
            return new HttpChannelBase("http", "localhost", 8080);
        }
        
    }
    public static class TestEndpointRepository
    {

        public static List<MessageEndpoint> GetAll()
        {
            var channelBase = TestChannel.GetBase();
            return new List<MessageEndpoint>()
            {
                new MessageEndpoint(
                    new MessageEndpointName("TestStream #1"),
                    new HttpChannel(channelBase, "teststream"),
                    MessageEndpoint.NoMessagesProcessed, 
                    PollingDelays.DefaultDelayMs, 
                    PollingDelays.DefaultPollingErrorDelays)
            };
        }
    }
}
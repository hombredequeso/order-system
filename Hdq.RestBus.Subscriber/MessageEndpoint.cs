using System.Collections.Generic;

namespace Hdq.RestBus.Subscriber
{
    public class MessageEndpoint
    {
        public MessageEndpoint(
            MessageEndpointName name, 
            HttpChannel channel, 
            long lastSuccessfullyProcessedMessage, 
            uint defaultPollingDelayMs, 
            Dictionary<HttpChannelPoller.PollingError, uint> pollingErrorDelays)
        {
            Channel = channel;
            LastSuccessfullyProcessedMessage = lastSuccessfullyProcessedMessage;
            Name = name;
            DefaultDelayMs = defaultPollingDelayMs;
            PollingErrorDelays = pollingErrorDelays;
        }

        public MessageEndpointName Name { get; }
        public HttpChannel Channel { get;  }
        public long LastSuccessfullyProcessedMessage { get; }

        public Dictionary<HttpChannelPoller.PollingError, uint> PollingErrorDelays { get; }
        public uint DefaultDelayMs { get; }

        public static readonly long NoMessagesProcessed = -1;
    }
}
using System.Collections.Generic;

namespace Hdq.RestBus.Receiver
{
    public class MessageEndpoint
    {
        public MessageEndpoint(
            MessageEndpointName name, 
            string path, 
            long lastSuccessfullyProcessedMessage, 
            uint defaultPollingDelayMs, 
            Dictionary<HttpChannelPoller.PollingError, uint> pollingErrorPolicy)
        {
            Path = path;
            LastSuccessfullyProcessedMessage = lastSuccessfullyProcessedMessage;
            Name = name;
            DefaultDelayMs = defaultPollingDelayMs;
            PollingErrorPolicy = pollingErrorPolicy;
        }

        public MessageEndpointName Name { get; }
        public string Path { get; }
        public long LastSuccessfullyProcessedMessage { get; }

        public Dictionary<HttpChannelPoller.PollingError, uint> PollingErrorPolicy { get; }
        public uint DefaultDelayMs { get; }

        public static readonly long NoMessagesProcessed = -1;
    }
}
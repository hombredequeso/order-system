using System.Collections.Generic;

namespace CarrierPidgin.ServiceA.Bus
{
    public class MessageStream
    {
        public MessageStream(
            MessageStreamName name, 
            string path, 
            long lastSuccessfullyProcessedMessage, 
            uint defaultPollingDelayMs, 
            Dictionary<HttpMessagePoller.PollingError, uint> pollingErrorPolicy)
        {
            Path = path;
            LastSuccessfullyProcessedMessage = lastSuccessfullyProcessedMessage;
            Name = name;
            DefaultDelayMs = defaultPollingDelayMs;
            PollingErrorPolicy = pollingErrorPolicy;
        }

        public MessageStreamName Name { get; }
        public string Path { get; }
        public long LastSuccessfullyProcessedMessage { get; }

        public Dictionary<HttpMessagePoller.PollingError, uint> PollingErrorPolicy { get; }
        public uint DefaultDelayMs { get; }

        public static readonly long NoMessagesProcessed = -1;
    }
}
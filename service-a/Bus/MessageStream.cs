using System.Collections.Generic;

namespace CarrierPidgin.ServiceA.Bus
{
    public class MessageStream
    {
        public MessageStream(
            string path, 
            long lastSuccessfullyProcessedMessage, 
            MessageStreamName name,
            uint defaultPollingDelayMs,
            Dictionary<HttpMessagePoller.PollingError, uint> pollingErrorPolicy)
        {
            Path = path;
            LastSuccessfullyProcessedMessage = lastSuccessfullyProcessedMessage;
            Name = name;
            DefaultDelayMs = defaultPollingDelayMs;
            PollingErrorPolicy = pollingErrorPolicy;
        }

        public MessageStreamName Name { get; set; }
        public string Path { get; set; }
        public long LastSuccessfullyProcessedMessage { get; set; }

        public Dictionary<HttpMessagePoller.PollingError, uint> PollingErrorPolicy { get; set; }
        public uint DefaultDelayMs { get; set; }

        public static long NoMessagesProcessed = -1;
    }
}
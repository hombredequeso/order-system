using System.Collections.Generic;

namespace CarrierPidgin.ServiceA.Bus
{
    public class MessageStream
    {
        public MessageStream(
            string name, 
            long lastSuccessfullyProcessedMessage, 
            string description,
            uint defaultPollingDelayMs,
            Dictionary<HttpMessagePoller.PollingError, uint> pollingErrorPolicy)
        {
            Name = name;
            LastSuccessfullyProcessedMessage = lastSuccessfullyProcessedMessage;
            Description = description;
            DefaultDelayMs = defaultPollingDelayMs;
            PollingErrorPolicy = pollingErrorPolicy;
        }

        public string Description { get; set; }
        public string Name { get; set; }
        public long LastSuccessfullyProcessedMessage { get; set; }

        public Dictionary<HttpMessagePoller.PollingError, uint> PollingErrorPolicy { get; set; }
        public uint DefaultDelayMs { get; set; }

        public static long NoMessagesProcessed = -1;
    }
}
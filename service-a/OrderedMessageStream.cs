using System.Collections.Generic;

namespace CarrierPidgin.ServiceA
{
    public class OrderedMessageStream
    {
        public OrderedMessageStream(
            string name, 
            int lastSuccessfullyProcessedMessage, 
            string description,
            uint defaultPollingDelayMs,
            Dictionary<Poller.PollingError, uint> pollingErrorPolicy)
        {
            Name = name;
            LastSuccessfullyProcessedMessage = lastSuccessfullyProcessedMessage;
            Description = description;
            DefaultDelayMs = defaultPollingDelayMs;
            PollingErrorPolicy = pollingErrorPolicy;
        }

        public string Description { get; set; }
        public string Name { get; set; }
        public int LastSuccessfullyProcessedMessage { get; set; }

        public Dictionary<Poller.PollingError, uint> PollingErrorPolicy { get; set; }
        public uint DefaultDelayMs { get; set; }

        public static int NoMessagesProcessed = -1;
    }
}
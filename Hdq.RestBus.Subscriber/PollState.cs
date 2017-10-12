using System.Collections.Generic;

namespace Hdq.RestBus.Subscriber
{
    public class PollState
    {
        public PollState(
            MessageEndpointName messageEndpointName, 
            uint defaultDelayMs, 
            Dictionary<HttpChannelPoller.PollingError, uint> pollingErrorDelayPolicy, 
            long lastMessageSuccessfullyProcessed, 
            string nextUrl, 
            uint delayMs)
        {
            NextUrl = nextUrl;
            DelayMs = delayMs;
            DefaultDelayMs = defaultDelayMs;
            LastMessageSuccessfullyProcessed = lastMessageSuccessfullyProcessed;
            MessageEndpointName = messageEndpointName;
            PollingErrorDelayPolicy = pollingErrorDelayPolicy;
        }


        public string NextUrl { get; }
        public uint DelayMs { get; }
        public long LastMessageSuccessfullyProcessed { get; }
        public  MessageEndpointName MessageEndpointName { get; }

        public uint DefaultDelayMs { get; }
        public static uint NoDelay = 0;
        public Dictionary<HttpChannelPoller.PollingError, uint> PollingErrorDelayPolicy { get; }

        public bool CanPoll()
        {
            return !string.IsNullOrEmpty(NextUrl);
        }

        public bool ShouldDelay()
        {
            return DelayMs > 0;
        }

        public PollState WithDelayFor(HttpChannelPoller.PollingError error)
        {
            return new PollState(
                MessageEndpointName,
                DefaultDelayMs, 
                PollingErrorDelayPolicy, 
                LastMessageSuccessfullyProcessed, 
                NextUrl, 
                PollingErrorDelayPolicy[error]);
        }

        public PollState WithDelay(uint newDelay)
        {
            return new PollState(
                MessageEndpointName,
                DefaultDelayMs, 
                PollingErrorDelayPolicy, 
                LastMessageSuccessfullyProcessed, 
                NextUrl, 
                newDelay);

        }
        public PollState With(
            string nextUrl = null,
            uint? delayMs = null,
            long? lastMessage = null)
        {
            return new PollState(
                MessageEndpointName,
                DefaultDelayMs, 
                PollingErrorDelayPolicy, 
                lastMessage ?? LastMessageSuccessfullyProcessed, 
                nextUrl ?? NextUrl, 
                delayMs ?? DelayMs);
        }
    }
}
using System.Collections.Generic;

namespace CarrierPidgin.ServiceA.Bus
{
    public class PollState
    {
        public PollState(
            MessageStreamName messageStreamName, 
            uint defaultDelayMs, 
            Dictionary<HttpMessagePoller.PollingError, uint> pollingErrorDelayPolicy, 
            long lastMessageSuccessfullyProcessed, 
            string nextUrl, 
            uint delayMs)
        {
            NextUrl = nextUrl;
            DelayMs = delayMs;
            DefaultDelayMs = defaultDelayMs;
            LastMessageSuccessfullyProcessed = lastMessageSuccessfullyProcessed;
            MessageStreamName = messageStreamName;
            PollingErrorDelayPolicy = pollingErrorDelayPolicy;
        }


        public string NextUrl { get; }
        public uint DelayMs { get; }
        public long LastMessageSuccessfullyProcessed { get; }
        public  MessageStreamName MessageStreamName { get; }

        public uint DefaultDelayMs { get; }
        public static uint NoDelay = 0;
        public Dictionary<HttpMessagePoller.PollingError, uint> PollingErrorDelayPolicy { get; }

        public bool CanPoll()
        {
            return !string.IsNullOrEmpty(NextUrl);
        }

        public bool ShouldDelay()
        {
            return DelayMs > 0;
        }

        public PollState WithDelayFor(HttpMessagePoller.PollingError error)
        {
            return new PollState(
                MessageStreamName,
                DefaultDelayMs, 
                PollingErrorDelayPolicy, 
                LastMessageSuccessfullyProcessed, 
                NextUrl, 
                PollingErrorDelayPolicy[error]);
        }

        public PollState WithDelay(uint newDelay)
        {
            return new PollState(
                MessageStreamName,
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
                MessageStreamName,
                DefaultDelayMs, 
                PollingErrorDelayPolicy, 
                lastMessage ?? LastMessageSuccessfullyProcessed, 
                nextUrl ?? NextUrl, 
                delayMs ?? DelayMs);
        }
    }
}
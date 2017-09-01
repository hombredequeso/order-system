using System.Collections.Generic;

namespace CarrierPidgin.ServiceA
{
    public class PollState
    {
        public PollState(
            string nextUrl,
            uint delayMs,
            int lastMessageSuccessfullyProcessed,
            string messageStreamDescription,
            Dictionary<Poller.PollingError, uint> pollingErrorDelayPolicy,
            uint defaultDelayMs)
        {
            NextUrl = nextUrl;
            DelayMs = delayMs;
            DefaultDelayMs = defaultDelayMs;
            LastMessageSuccessfullyProcessed = lastMessageSuccessfullyProcessed;
            MessageStreamName = messageStreamDescription;
            PollingErrorDelayPolicy = pollingErrorDelayPolicy;
        }


        public string NextUrl { get; }
        public uint DelayMs { get; }
        public int LastMessageSuccessfullyProcessed { get; }
        public  string MessageStreamName { get; }

        public uint DefaultDelayMs { get; }
        public static uint NoDelay = 0;
        public Dictionary<Poller.PollingError, uint> PollingErrorDelayPolicy { get; }

        public bool CanPoll()
        {
            return !string.IsNullOrEmpty(NextUrl);
        }

        public bool ShouldDelay()
        {
            return DelayMs > 0;
        }

        public PollState WithDelayFor(Poller.PollingError error)
        {
            return new PollState(
                this.NextUrl, 
                this.PollingErrorDelayPolicy[error], 
                this.LastMessageSuccessfullyProcessed,
                this.MessageStreamName,
                this.PollingErrorDelayPolicy,
                this.DefaultDelayMs);
        }

        public PollState WithDelay(uint newDelay)
        {
            return new PollState(
                this.NextUrl, 
                newDelay, 
                this.LastMessageSuccessfullyProcessed,
                this.MessageStreamName,
                this.PollingErrorDelayPolicy,
                this.DefaultDelayMs);

        }
        public PollState With(
            string nextUrl = null,
            uint? delayMs = null,
            int? lastMessage = null)
        {
            return new PollState(
                nextUrl ?? this.NextUrl,
                delayMs ?? this.DelayMs,
                lastMessage ?? this.LastMessageSuccessfullyProcessed,
                this.MessageStreamName,
                this.PollingErrorDelayPolicy,
                this.DefaultDelayMs);
        }
    }
}
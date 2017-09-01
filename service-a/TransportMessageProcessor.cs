using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using CarrierPidgin.Lib;
using NLog;

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


    public class MessageProcessingContext
    {
        protected MessageProcessingContext()
        {
            Unprocessed = new List<DomainMessage>();
            ProcessedSuccessfully = new List<DomainMessage>();
            ProcessedUnsuccessfully = new List<DomainMessage>();
        }

        public MessageProcessingContext(
            IEnumerable<DomainMessage> processedSuccessfully, 
            IEnumerable<DomainMessage> processedUnsuccessfully, 
            IEnumerable<DomainMessage> unprocessed)
        {
            Unprocessed = unprocessed.ToList();
            ProcessedSuccessfully = processedSuccessfully.ToList();
            ProcessedUnsuccessfully = processedUnsuccessfully.ToList();
        }

        public static MessageProcessingContext Start()
        {
            return new MessageProcessingContext();
        }

        public  MessageProcessingContext AddSuccess(DomainMessage e)
        {
            return new MessageProcessingContext(
                ProcessedSuccessfully.Concat(new[] {e}),
                ProcessedUnsuccessfully,
                Unprocessed);
        }

        public  MessageProcessingContext AddFailure(DomainMessage e)
        {
            return new MessageProcessingContext(
                ProcessedSuccessfully,
                ProcessedUnsuccessfully.Concat(new[] {e}),
                Unprocessed);
        }

        public  MessageProcessingContext AddUnprocessed(DomainMessage e)
        {
            return new MessageProcessingContext(
                ProcessedSuccessfully,
                ProcessedUnsuccessfully,
                Unprocessed.Concat(new[] {e}));
        }

        public List<DomainMessage> ProcessedSuccessfully { get; }
        public List<DomainMessage> ProcessedUnsuccessfully { get; }
        public List<DomainMessage> Unprocessed { get; }
        
    }

    // Responsible for processing a TransportMessage.
    // To change the policy of "must processes all messages in order", this would be the function to alter.
    public static class TransportMessageProcessor
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static PollState ProcessTransportMessage(PollState pollStatus, TransportMessage transportMessage)
        {
            Logger.Trace("---------------------------------------------------------");
            Logger.Trace($"PROCESSING TRANSPORT MESSAGE FOR: {pollStatus.MessageStreamName}");
            List<DomainMessage> unprocessedMessages = transportMessage
                .Messages
                .Where(x => (int) (x.Header.MessageNumber) > pollStatus.LastMessageSuccessfullyProcessed)
                .OrderBy(x => x.Header.MessageNumber)
                .ToList();

            Link prevLink = transportMessage.Header.Links.SingleOrDefault(l => l.Rel.Contains(Link.Previous));
            bool areUnprocessedMessagesInEarlierTransportMessage =
                unprocessedMessages.Any() &&
                prevLink != null &&
                unprocessedMessages.Min(x => (int) x.Header.MessageNumber) != pollStatus.LastMessageSuccessfullyProcessed + 1;

            if (areUnprocessedMessagesInEarlierTransportMessage)
                return pollStatus.With(prevLink.Href, 0, pollStatus.LastMessageSuccessfullyProcessed);

            var initialState = MessageProcessingContext.Start();
            var finalState = unprocessedMessages.Aggregate(initialState, TransportMessageProcessor.ProcessNext);


            var nextLink = transportMessage.Header.Links.SingleOrDefault(l => l.Rel.Contains(Link.Next));
            var selfLink = transportMessage.Header.Links.Single(l => l.Rel.Contains(Link.Self));

            var finishedWithThisTransportMessage =
                nextLink != null &&
                unprocessedMessages.Count == finalState.ProcessedSuccessfully.Count;

            var newLastMessageNumberProcessed = finalState
                    .ProcessedSuccessfully
                    .Select(x => (int)x.Header.MessageNumber)
                    .Concat(new[] { pollStatus.LastMessageSuccessfullyProcessed })
                    .Max();

            return finishedWithThisTransportMessage ?
                pollStatus.With(
                    nextLink.Href, 
                    PollState.NoDelay, 
                    newLastMessageNumberProcessed) :
                pollStatus.With(
                    selfLink.Href, 
                    pollStatus.DefaultDelayMs, 
                    newLastMessageNumberProcessed);
        }

        private static MessageProcessingContext ProcessNext(
            MessageProcessingContext processingContext, 
            DomainMessage domainMessage)
        {
            if (processingContext.ProcessedUnsuccessfully.Any())
                return processingContext.AddUnprocessed(domainMessage);

            var processingResult = DomainMessageProcessor.ProcessMessage(domainMessage);

            return processingResult.GetType() == typeof(DomainMessageProcessor.ProcessMessageSuccess) 
                ? processingContext.AddSuccess(domainMessage) 
                : processingContext.AddFailure(domainMessage);
        }

        public static void LogError(PollState ps, Poller.PollingError error)
        {
            switch (error)
            {
                case Poller.PollingError.UnableToConnect:
                {
                    Logger.Warn($"Error GET {ps.NextUrl}: Unable to connect to api");
                    break;
                }
                case Poller.PollingError.UnknownErrorOnGet:
                {
                    Logger.Error($"Error GET {ps.NextUrl}: Unknown error on get");
                    break;
                }
                case Poller.PollingError.ErrorMakingHttpRequest:
                {
                    Logger.Error($"Error GET {ps.NextUrl}: making request");
                    break;
                }
                case Poller.PollingError.ErrorDeserializingContent:
                {
                    Logger.Warn($"Error GET {ps.NextUrl}: This is probably never going to work");
                    break;
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using CarrierPidgin.Lib;
using NLog;

namespace CarrierPidgin.ServiceA
{
    public class PollState
    {
        public PollState(string nextUrl, int delay)
        {
            NextUrl = nextUrl;
            Delay = delay;
        }

        public string NextUrl { get; }
        public int Delay { get; }

        public bool CanPoll()
        {
            return !string.IsNullOrEmpty(NextUrl);
        }

        public bool ShouldDelay()
        {
            return Delay >= 0;
        }
    }


    public class MessageProcessingContext
    {
        protected MessageProcessingContext()
        {
            Unprocessed = new List<DomainEvent>();
            ProcessedSuccessfully = new List<DomainEvent>();
            ProcessedUnsuccessfully = new List<DomainEvent>();
        }

        public MessageProcessingContext(
            IEnumerable<DomainEvent> processedSuccessfully, 
            IEnumerable<DomainEvent> processedUnsuccessfully, 
            IEnumerable<DomainEvent> unprocessed)
        {
            Unprocessed = unprocessed.ToList();
            ProcessedSuccessfully = processedSuccessfully.ToList();
            ProcessedUnsuccessfully = processedUnsuccessfully.ToList();
        }

        public static MessageProcessingContext Start()
        {
            return new MessageProcessingContext();
        }

        public  MessageProcessingContext AddSuccess(DomainEvent e)
        {
            return new MessageProcessingContext(
                ProcessedSuccessfully.Concat(new[] {e}),
                ProcessedUnsuccessfully,
                Unprocessed);
        }

        public  MessageProcessingContext AddFailure(DomainEvent e)
        {
            return new MessageProcessingContext(
                ProcessedSuccessfully,
                ProcessedUnsuccessfully.Concat(new[] {e}),
                Unprocessed);
        }

        public  MessageProcessingContext AddUnprocessed(DomainEvent e)
        {
            return new MessageProcessingContext(
                ProcessedSuccessfully,
                ProcessedUnsuccessfully,
                Unprocessed.Concat(new[] {e}));
        }

        public List<DomainEvent> ProcessedSuccessfully { get; }
        public List<DomainEvent> ProcessedUnsuccessfully { get; }
        public List<DomainEvent> Unprocessed { get; }
        
    }

    // Responsible for processing a TransportMessage.
    // To change the policy of "must processes all messages in order", this would be the function to alter.
    public static class TransportMessageProcessor
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public static int LastEventNumber = -1;

        public static PollState ProcessTransportMessage(PollState pollStatus, TransportMessage transportMessage)
        {
            List<DomainEvent> unprocessedMessages = transportMessage
                .Messages
                .Where(x => (int) (x.Header.EventNumber) > LastEventNumber)
                .OrderBy(x => x.Header.EventNumber)
                .ToList();

            Link prevLink = transportMessage.Header.Links.SingleOrDefault(l => l.Rel.Contains(Link.Previous));
            bool areUnprocessedMessagesInEarlierTransportMessage =
                unprocessedMessages.Any() &&
                prevLink != null &&
                unprocessedMessages.Min(x => (int) x.Header.EventNumber) != LastEventNumber + 1;

            if (areUnprocessedMessagesInEarlierTransportMessage)
                return new PollState(prevLink.Href, 0);

            var initialState = MessageProcessingContext.Start();
            var finalState = unprocessedMessages.Aggregate(initialState, TransportMessageProcessor.ProcessNext);
            LastEventNumber = finalState.ProcessedSuccessfully.Select(x => (int) x.Header.EventNumber)
                .Concat(new[] {LastEventNumber})
                .Max();


            var nextLink = transportMessage.Header.Links.SingleOrDefault(l => l.Rel.Contains(Link.Next));
            var selfLink = transportMessage.Header.Links.Single(l => l.Rel.Contains(Link.Self));

            var finishedWithThisTransportMessage =
                nextLink != null &&
                unprocessedMessages.Count == finalState.ProcessedSuccessfully.Count;

            return finishedWithThisTransportMessage ?
                new PollState(nextLink.Href, 0) :
                new PollState(selfLink.Href, 1000);
        }

        private static MessageProcessingContext ProcessNext(
            MessageProcessingContext processingContext, 
            DomainEvent domainEvent)
        {
            if (processingContext.ProcessedUnsuccessfully.Any())
                return processingContext.AddUnprocessed(domainEvent);

            var processingResult = DomainMessageProcessor.ProcessMessage(domainEvent);

            return processingResult.GetType() == typeof(DomainMessageProcessor.ProcessMessageSuccess) 
                ? processingContext.AddSuccess(domainEvent) 
                : processingContext.AddFailure(domainEvent);
        }

        public static PollState ProcessPollError(PollState ps, Poller.PollingError error)
        {
            switch (error)
            {
                case Poller.PollingError.UnableToConnect:
                {
                    Logger.Warn($"Error GET {ps.NextUrl}: Unable to connect to api");
                    return new PollState(ps.NextUrl, 5000);
                }
                case Poller.PollingError.UnknownErrorOnGet:
                {
                    Logger.Error($"Error GET {ps.NextUrl}: Unknown error on get");
                    return new PollState(ps.NextUrl, 10000);
                }
                case Poller.PollingError.ErrorMakingHttpRequest:
                {
                    Logger.Error($"Error GET {ps.NextUrl}: making request");
                    return new PollState(ps.NextUrl, 10000);
                }
                case Poller.PollingError.ErrorDeserializingContent:
                {
                    Logger.Warn($"Error GET {ps.NextUrl}: This is probably never going to work");
                    return new PollState(ps.NextUrl, 10000);
                }
                default:
                    return ps;
            }
        }
    }
}
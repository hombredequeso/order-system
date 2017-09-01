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
        public static int LastEventNumber = -1;

        public static PollState ProcessTransportMessage(PollState pollStatus, TransportMessage transportMessage)
        {
            List<DomainMessage> unprocessedMessages = transportMessage
                .Messages
                .Where(x => (int) (x.Header.MessageNumber) > LastEventNumber)
                .OrderBy(x => x.Header.MessageNumber)
                .ToList();

            Link prevLink = transportMessage.Header.Links.SingleOrDefault(l => l.Rel.Contains(Link.Previous));
            bool areUnprocessedMessagesInEarlierTransportMessage =
                unprocessedMessages.Any() &&
                prevLink != null &&
                unprocessedMessages.Min(x => (int) x.Header.MessageNumber) != LastEventNumber + 1;

            if (areUnprocessedMessagesInEarlierTransportMessage)
                return new PollState(prevLink.Href, 0);

            var initialState = MessageProcessingContext.Start();
            var finalState = unprocessedMessages.Aggregate(initialState, TransportMessageProcessor.ProcessNext);
            LastEventNumber = finalState.ProcessedSuccessfully.Select(x => (int) x.Header.MessageNumber)
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
            DomainMessage domainMessage)
        {
            if (processingContext.ProcessedUnsuccessfully.Any())
                return processingContext.AddUnprocessed(domainMessage);

            var processingResult = DomainMessageProcessor.ProcessMessage(domainMessage);

            return processingResult.GetType() == typeof(DomainMessageProcessor.ProcessMessageSuccess) 
                ? processingContext.AddSuccess(domainMessage) 
                : processingContext.AddFailure(domainMessage);
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
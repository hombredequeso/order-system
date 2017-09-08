using System.Collections.Generic;
using System.Linq;
using CarrierPidgin.Lib;
using NLog;

namespace CarrierPidgin.ServiceA
{
    // Responsible for processing a TransportMessage.
    // To change the policy of "must processes all messages in order", this would be the function to alter.
    public static class TransportMessageProcessor
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static PollState ProcessTransportMessage(PollState pollStatus, TransportMessage transportMessage)
        {
            // Logger.Trace("---------------------------------------------------------");
            // Logger.Trace($"PROCESSING TRANSPORT MESSAGE FOR: {pollStatus.MessageStreamName}");
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
                    Logger.Fatal($"Error GET {ps.NextUrl}: This is probably never going to work");
                    break;
                }
            }
        }
    }
}
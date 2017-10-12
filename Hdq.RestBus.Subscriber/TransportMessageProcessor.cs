using System;
using System.Collections.Generic;
using System.Linq;
using NLog;

namespace Hdq.RestBus.Subscriber
{
    // Responsible for processing a TransportMessage.
    // To change the policy of "must processes all messages in order", this would be the function to alter.
    public static class TransportMessageProcessor
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static PollState ProcessTransportMessage(
            PollState pollStatus,
            TransportMessage transportMessage,
            Func<Type, Action<DomainMessageProcessingContext, object>> processors)
        {
            // Logger.Trace("---------------------------------------------------------");
            // Logger.Trace($"PROCESSING TRANSPORT MESSAGE FOR: {pollStatus.MessageEndpointName}");
            List<DomainMessage> unprocessedMessages = transportMessage
                .Messages
                .Where(x => (x.Header.MessageNumber) > pollStatus.LastMessageSuccessfullyProcessed)
                .OrderBy(x => x.Header.MessageNumber)
                .ToList();

            Link prevLink = transportMessage.Header.Links.SingleOrDefault(l => l.Rel.Contains(Link.Previous));
            bool areUnprocessedMessagesInEarlierTransportMessage =
                unprocessedMessages.Any() &&
                prevLink != null &&
                unprocessedMessages.Min(x => x.Header.MessageNumber) != pollStatus.LastMessageSuccessfullyProcessed + 1;

            if (areUnprocessedMessagesInEarlierTransportMessage)
                return pollStatus.With(prevLink.Href, 0, pollStatus.LastMessageSuccessfullyProcessed);

            var initialState = MessageEndpointContext.Start(pollStatus.MessageEndpointName);

            MessageEndpointContext ProcessMessageF(MessageEndpointContext context, DomainMessage domainMessage) =>
                ProcessNext(context, domainMessage, processors);

            var finalState = unprocessedMessages.Aggregate(
                initialState,
                ProcessMessageF);


            var nextLink = transportMessage.Header.Links.SingleOrDefault(l => l.Rel.Contains(Link.Next));
            var selfLink = transportMessage.Header.Links.Single(l => l.Rel.Contains(Link.Self));

            var finishedWithThisTransportMessage =
                nextLink != null &&
                unprocessedMessages.Count == finalState.ProcessedSuccessfully.Count;

            var newLastMessageNumberProcessed = finalState
                    .ProcessedSuccessfully
                    .Select(x => x.Header.MessageNumber)
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

        private static MessageEndpointContext ProcessNext(
            MessageEndpointContext endpointContext, 
            DomainMessage domainMessage,
            Func<Type, Action<DomainMessageProcessingContext, object>> processors)
        {
            if (endpointContext.ProcessedUnsuccessfully.Any())
                return endpointContext.AddUnprocessed(domainMessage);

            var processingResult = DomainMessageProcessor.ProcessMessage(
                domainMessage,
                endpointContext.SourceQueue,
                processors);

            return processingResult.GetType() == typeof(DomainMessageProcessor.ProcessMessageSuccess) 
                ? endpointContext.AddSuccess(domainMessage) 
                : endpointContext.AddFailure(domainMessage);
        }

        public static void LogError(PollState ps, HttpChannelPoller.PollingError error)
        {
            switch (error)
            {
                case HttpChannelPoller.PollingError.UnableToConnect:
                {
                    Logger.Warn($"Error GET {ps.NextUrl}: Unable to connect to api");
                    break;
                }
                case HttpChannelPoller.PollingError.UnknownErrorOnGet:
                {
                    Logger.Error($"Error GET {ps.NextUrl}: Unknown error on get");
                    break;
                }
                case HttpChannelPoller.PollingError.ErrorMakingHttpRequest:
                {
                    Logger.Error($"Error GET {ps.NextUrl}: making request");
                    break;
                }
                case HttpChannelPoller.PollingError.ErrorDeserializingContent:
                {
                    Logger.Fatal($"Error GET {ps.NextUrl}: This is probably never going to work");
                    break;
                }
            }
        }
    }
}
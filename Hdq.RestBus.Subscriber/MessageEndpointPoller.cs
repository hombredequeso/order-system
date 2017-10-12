using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hdq.Lib;

namespace Hdq.RestBus.Subscriber
{
    public static class MessageEndpointPoller
    {
        // This is the entry point for an application to use this library.
        // Calling this function sets up the system to poll an endpoint, passing messages through the
        // relevant domainMessageProcessors.
        // Incidentally, it is essentially the last function in the receiving library to be non-functional,
        // what with its returning a Task (as good as void), and having a while loop that mutates the pollStatus
        // object.
        public static async Task MainInfinitePollerAsync(
            MessageEndpointState endpointState, 
            Func<Type, Action<DomainMessageProcessingContext, object>> domainMessageProcessors,
            Func<string, Either<DeserializeError, TransportMessage>> deserializeTransportMessage,
            uint pollRateMs, 
            Dictionary<HttpChannelPoller.PollingError, uint> pollingErrorDelays, 
            Func<IHttpService> httpServiceCreator, 
            CancellationToken ct)
        {

            using (IHttpService client = httpServiceCreator())
            {
                while (!ct.IsCancellationRequested)
                {
                    var channelLocation = endpointState.Channel;
                    var uriBuilder = new UriBuilder(
                            channelLocation.HttpChannelBase.Scheme,
                            channelLocation.HttpChannelBase.Host,
                            channelLocation.HttpChannelBase.Port)
                        {Path = channelLocation.Path};

                    var startUrl = uriBuilder.ToString();

                    var pollStatus = new PollState(endpointState.EndpointName,
                        pollRateMs, pollingErrorDelays, endpointState.LastSuccessfullyProcessedMessage, startUrl, 0);

                    while (pollStatus.CanPoll())
                    {
                        if (pollStatus.ShouldDelay())
                            await Task.Delay((int) pollStatus.DelayMs, ct);

                        pollStatus = await Execute(
                            pollStatus,
                            client,
                            ct,
                            domainMessageProcessors,
                            deserializeTransportMessage);
                    }
                }
            }
        }

        // The start of a largely functional code.
        // Given the current pollstate (ps), this function polls the channel through the 'client',
        // executing the relevant domainMessageProcessors for any new messages received, in order,
        // without processing duplicates.
        public static async Task<PollState> Execute(
            PollState ps, 
            IHttpService client, 
            CancellationToken ct,
            Func<Type, Action<DomainMessageProcessingContext, object>> domainMessageProcessors,
            Func<string, Either<DeserializeError, TransportMessage>> deserializeTransportMessage)
        {
            Either<HttpChannelPoller.PollingError, TransportMessage> transportMessage = 
                await HttpChannelPoller.Poll(ps.NextUrl, client, ct, deserializeTransportMessage);
            PollState pollStatus = transportMessage.Match(
                error =>
                {
                    TransportMessageProcessor.LogError(ps, error);
                    return ps.WithDelayFor(error);
                },
                m => TransportMessageProcessor.ProcessTransportMessage(
                    ps,
                    m,
                    domainMessageProcessors));

            return pollStatus;
        }
    }
}
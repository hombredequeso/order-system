using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hdq.Lib;

namespace Hdq.RestBus.Receiver
{
    public static class MessageEndpointPoller
    {
        public static async Task MainInfinitePollerAsync(
            MessageEndpointState endpointState, 
            Func<Type, Action<DomainMessageProcessor.DomainMessageProcessingContext, object>> domainMessageProcessors,
            Func<string, Either<DeserializeError, TransportMessage>> deserializeTransportMessage,
            uint pollRateMs, 
            Dictionary<HttpChannelPoller.PollingError, uint> pollingErrorPolicy, 
            Func<IHttpService> httpServiceCreator, 
            CancellationToken ct)
        {

            using (IHttpService client = httpServiceCreator())
            {
                while (!ct.IsCancellationRequested)
                {
                    var channelLocation = endpointState.ChannelLocation;
                    var uriBuilder = new UriBuilder(
                            channelLocation.Scheme,
                            channelLocation.Host,
                            channelLocation.Port)
                        {Path = channelLocation.Path};

                    var startUrl = uriBuilder.ToString();

                    var pollStatus = new PollState(endpointState.EndpointName,
                        pollRateMs, pollingErrorPolicy, endpointState.LastSuccessfullyProcessedMessage, startUrl, 0);

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

        public static async Task<PollState> Execute(
            PollState ps, 
            IHttpService client, 
            CancellationToken ct,
            Func<Type, Action<DomainMessageProcessor.DomainMessageProcessingContext, object>> domainMessageProcessors,
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
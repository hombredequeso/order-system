using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CarrierPidgin.Lib;
using Hdq.Lib;

namespace CarrierPidgin.ServiceA.Bus
{
    public static class MessageStreamPoller
    {
        public static async Task MainInfinitePollerAsync(
            MessageStreamState streamState, 
            Func<Type, Action<DomainMessageProcessor.DomainMessageProcessingContext, object>> domainMessageProcessors,
            Func<string, Either<DeserializeError, TransportMessage>> deserializeTransportMessage,
            uint streamPollRateMs, 
            Dictionary<HttpMessagePoller.PollingError, uint> pollingErrorPolicy, 
            Func<IHttpService> httpServiceCreator, 
            CancellationToken ct)
        {

            using (IHttpService client = httpServiceCreator())
            {
                while (!ct.IsCancellationRequested)
                {
                    var streamLocation = streamState.StreamLocation;
                    var uriBuilder = new UriBuilder(
                            streamLocation.Scheme,
                            streamLocation.Host,
                            streamLocation.Port)
                        {Path = streamLocation.Path};

                    var startUrl = uriBuilder.ToString();

                    var pollStatus = new PollState(streamState.StreamName,
                        streamPollRateMs, pollingErrorPolicy, streamState.LastSuccessfullyProcessedMessage, startUrl, 0);

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
            Either<HttpMessagePoller.PollingError, TransportMessage> transportMessage = 
                await HttpMessagePoller.Poll(ps.NextUrl, client, ct, deserializeTransportMessage);
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
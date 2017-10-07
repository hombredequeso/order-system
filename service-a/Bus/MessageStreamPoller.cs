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
            CancellationToken ct,
            uint initialPollRateMs,
            Dictionary<HttpMessagePoller.PollingError, uint> pollingErrorPolicy,
            MessageProcessingData mpd,
            Func<IHttpService> httpServiceCreator)
        {

            using (IHttpService client = httpServiceCreator())
            {
                while (!ct.IsCancellationRequested)
                {
                    var streamLocation = streamState.StreamLocation;
                    var uriBuilder = new UriBuilder(streamLocation.Scheme, streamLocation.Host, streamLocation.Port);
                    uriBuilder.Path = streamLocation.Path;
                    var startUrl = uriBuilder.ToString();

                    var pollStatus = new PollState(
                        startUrl, 
                        0, 
                        streamState.LastSuccessfullyProcessedMessage,
                        streamState.StreamName,
                        pollingErrorPolicy,
                        initialPollRateMs);

                    while (pollStatus.CanPoll())
                    {
                        if (pollStatus.ShouldDelay())
                            await Task.Delay((int) pollStatus.DelayMs, ct);

                        pollStatus = await Execute(
                            pollStatus,
                            client,
                            ct,
                            mpd);
                    }
                }
            }
        }

        public static async Task<PollState> Execute(
            PollState ps, 
            IHttpService client, 
            CancellationToken ct,
            MessageProcessingData mpd)
        {
            Either<HttpMessagePoller.PollingError, TransportMessage> transportMessage = 
                await HttpMessagePoller.Poll(ps.NextUrl, client, ct, mpd.DeserializeTransportMessage);
            PollState pollStatus = transportMessage.Match(
                error =>
                {
                    TransportMessageProcessor.LogError(ps, error);
                    return ps.WithDelayFor(error);
                },
                m => TransportMessageProcessor.ProcessTransportMessage(
                    ps,
                    m,
                    mpd));

            return pollStatus;
        }
    }
}
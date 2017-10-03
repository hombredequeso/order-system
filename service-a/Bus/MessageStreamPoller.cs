using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace CarrierPidgin.ServiceA.Bus
{
    public static class MessageStreamPoller
    {
        public static async Task MainInfinitePollerAsync(
            MessageStreamState streamState, 
            CancellationToken ct,
            uint initialPollRateMs,
            Dictionary<HttpMessagePoller.PollingError, uint> PollingErrorPolicy,
            Func<Type, Action<DomainMessageProcessor.DomainMessageProcessingContext, object>> domainMessageProcessorLookup,
            Dictionary<string, Type> messageTypeLookup
            )
        {
            var streamLocation = streamState.StreamLocation;

            using (HttpClient client = new HttpClient())
            {
                var uriBuilder = new UriBuilder(streamLocation.Scheme, streamLocation.Host, streamLocation.Port);
                client.BaseAddress = uriBuilder.Uri;
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                uriBuilder.Path = streamLocation.Path;
                var startUrl = uriBuilder.ToString();

                while (!ct.IsCancellationRequested)
                {
                    var messageStreamName = new MessageStreamName(streamState.Description);
                    var pollStatus = new PollState(
                        startUrl, 
                        initialPollRateMs, 
                        streamState.LastSuccessfullyProcessedMessage,
                        messageStreamName,
                        PollingErrorPolicy,
                        initialPollRateMs);

                    while (pollStatus.CanPoll())
                        pollStatus = await Execute(
                            pollStatus,
                            client,
                            ct,
                            domainMessageProcessorLookup,
                            messageTypeLookup);
                }
            }
        }

        public static async Task<PollState> Execute(
            PollState ps, 
            HttpClient client, 
            CancellationToken ct,
            Func<Type, Action<DomainMessageProcessor.DomainMessageProcessingContext, object>> domainMessageProcessorLookup,
            Dictionary<string, Type> messageTypeLookup
            )
        {
            var transportMessage = await HttpMessagePoller.Poll(ps.NextUrl, client, ct);
            PollState pollStatus = transportMessage.Match(
                error =>
                {
                    TransportMessageProcessor.LogError(ps, error);
                    return ps.WithDelayFor(error);
                },
                m => TransportMessageProcessor.ProcessTransportMessage(
                    ps, 
                    m, 
                    domainMessageProcessorLookup,
                    messageTypeLookup));

            if (pollStatus.ShouldDelay())
                await Task.Delay((int)pollStatus.DelayMs, ct);

            return pollStatus;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace CarrierPidgin.ServiceA
{
    internal class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static void Main(string[] args)
        {
            Logger.Trace("Start");
            var cts = new CancellationTokenSource();
            var ct = cts.Token;


            var messageStreams = MessageStreamRepository.Get();

            foreach (var messageStream in messageStreams)
            {
                var streamLocation = ServiceLocator.GetMessageStreamLocation(messageStream.Name);
                var messageStreamState = new MessageStreamState(
                    streamLocation, 
                    messageStream.LastSuccessfullyProcessedMessage,
                    messageStream.Description);

                MainInfinitePollerAsync(
                    messageStreamState, 
                    ct, 
                    messageStream.DefaultDelayMs, 
                    messageStream.PollingErrorPolicy);
            }

            Console.WriteLine("press enter to stop");
            Console.Read();
            cts.Cancel();
            Logger.Trace("End");
        }

        public static async Task MainInfinitePollerAsync(
            MessageStreamState streamState, 
            CancellationToken ct,
            uint initialPollRateMs,
            Dictionary<Poller.PollingError, uint> PollingErrorPolicy)
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
                        pollStatus = await Execute(pollStatus, client, ct);
                }
            }
        }

        public static async Task<PollState> Execute(PollState ps, HttpClient client, CancellationToken ct)
        {
            var transportMessage = await Poller.Poll(ps.NextUrl, client, ct);
            PollState pollStatus = transportMessage.Match(
                error =>
                {
                    TransportMessageProcessor.LogError(ps, error);
                    return ps.WithDelayFor(error);
                },
                m => TransportMessageProcessor.ProcessTransportMessage(ps, m));

            if (pollStatus.ShouldDelay())
                await Task.Delay((int)pollStatus.DelayMs, ct);

            return pollStatus;
        }
    }
}
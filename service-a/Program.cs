using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace CarrierPidgin.ServiceA
{
    public class OrderedMessageStream
    {
        public OrderedMessageStream(string name, int lastSuccessfullyProcessedMessage, string description)
        {
            Name = name;
            LastSuccessfullyProcessedMessage = lastSuccessfullyProcessedMessage;
            Description = description;
        }

        public string Description { get; set; }
        public string Name { get; set; }
        public int LastSuccessfullyProcessedMessage { get; set; }

        public static int NoMessagesProcessed = -1;
    }

    public static class MessageStreamRepository
    {
        public static OrderedMessageStream Get()
        {
            return new OrderedMessageStream("teststream", OrderedMessageStream.NoMessagesProcessed, "TestStream #1");
        }
    }

    public class MessageStreamState
    {
        public MessageStreamState(
            MessageStreamLocation streamLocation, 
            int lastSuccessfullyProcessedMessage,
            string description)
        {
            StreamLocation = streamLocation;
            LastSuccessfullyProcessedMessage = lastSuccessfullyProcessedMessage;
            Description = description;
        }

        public MessageStreamLocation StreamLocation { get; }
        public int LastSuccessfullyProcessedMessage { get; }
        public string Description { get; }
    }


    internal class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static void Main(string[] args)
        {
            Logger.Trace("Start");
            var cts = new CancellationTokenSource();
            var ct = cts.Token;

            var messageStream = MessageStreamRepository.Get();
            var streamLocation = ServiceLocator.GetMessageStreamLocation(messageStream.Name);
            var messageStreamState = new MessageStreamState(
                streamLocation, 
                messageStream.LastSuccessfullyProcessedMessage,
                messageStream.Description);
            uint defaultDelayMs = 1000;
            Dictionary<Poller.PollingError, uint> pollingErrorPolicy = new Dictionary<Poller.PollingError, uint>()
            {
                { Poller.PollingError.UnableToConnect, 5000 },
                { Poller.PollingError.ErrorDeserializingContent, 10000 },
                { Poller.PollingError.ErrorMakingHttpRequest, 10000 },
                { Poller.PollingError.UnknownErrorOnGet, 10000 }
            };

            MainInfinitePollerAsync(messageStreamState, ct, defaultDelayMs, pollingErrorPolicy);



            // Example of running another message stream state, on the same message stream.
            //
            // var messageStreamState2 = new MessageStreamState(
            //     streamLocation, 
            //     messageStream.LastSuccessfullyProcessedMessage,
            //     "ANOTHER STREAM  #2");
            // MainInfinitePollerAsync(messageStreamState2, ct, 5000, pollingErrorPolicy);



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
                    var pollStatus = new PollState(
                        startUrl, 
                        initialPollRateMs, 
                        streamState.LastSuccessfullyProcessedMessage,
                        streamState.Description,
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
using System;
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

            var streamLocation = ServiceLocator.GetMessageStreamLocation("teststream");

            MainInfinitePollerAsync(streamLocation, ct);
            Console.WriteLine("press enter to stop");
            Console.Read();
            cts.Cancel();
            Logger.Trace("End");
        }

        public static async Task MainInfinitePollerAsync(MessageStreamLocation stream, CancellationToken ct)
        {

            using (HttpClient client = new HttpClient())
            {
                var uriBuilder = new UriBuilder(stream.Scheme, stream.Host, stream.Port);
                client.BaseAddress = uriBuilder.Uri;
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                uriBuilder.Path = stream.Path;
                var startUrl = uriBuilder.ToString();

                while (!ct.IsCancellationRequested)
                {
                    var pollStatus = new PollState(startUrl, 1000);
                    while (pollStatus.CanPoll())
                        pollStatus = await Execute(pollStatus, client, ct);
                }
            }
        }

        public static async Task<PollState> Execute(PollState ps, HttpClient client, CancellationToken ct)
        {
            var transportMessage = await Poller.Poll(ps.NextUrl, client, ct);
            var pollStatus = transportMessage.Match(
                error => TransportMessageProcessor.ProcessPollError(ps, error),
                m => TransportMessageProcessor.ProcessTransportMessage(ps, m));

            if (pollStatus.ShouldDelay())
                await Task.Delay(pollStatus.Delay, ct);

            return pollStatus;
        }
    }


}
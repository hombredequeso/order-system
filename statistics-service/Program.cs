using System;
using System.Collections.Generic;
using System.Threading;
using Hdq.RestBus;
using Hdq.RestBus.Subscriber;
using Hdq.Lib;
using Hdq.Statistics.Dal;
using NLog;

namespace Hdq.Statistics
{
    internal class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static List<MessageEndpoint> GetMessageEndpoints()
        {
            using (var statisticsUow = new UnitOfWork(Database.ConnectionString))
            {
                return MessageEndpointRepository.GetAll(statisticsUow);
            }
        }

        private static void Main(string[] args)
        {
            Logger.Trace("Start");
            var cts = new CancellationTokenSource();
            var ct = cts.Token;

            Either<DeserializeError, TransportMessage> DeserializeTransportMessage(string s) => 
                TransportMessageDeserializer.Deserialize(s, MessageEndpointRepository.GetMessageTypeLookup());

            foreach (var endpoint in GetMessageEndpoints())
            {
                var messageEndpointState = new MessageEndpointState(
                    endpoint.Channel,
                    endpoint.LastSuccessfullyProcessedMessage,
                    endpoint.Name);

                IHttpService ServiceCreator() => new HttpService(new UriBuilder(
                    endpoint.Channel.HttpChannelBase.Scheme, 
                    endpoint.Channel.HttpChannelBase.Host, 
                    endpoint.Channel.HttpChannelBase.Port).Uri);

                MessageEndpointPoller.MainInfinitePollerAsync(
                    messageEndpointState,
                    HandlerFactory.GetHandlerForMessageType,
                    DeserializeTransportMessage,
                    endpoint.DefaultDelayMs, 
                    endpoint.PollingErrorDelays, 
                    ServiceCreator, 
                    ct);
            }

            Console.WriteLine("press enter to stop");
            Console.Read();
            cts.Cancel();
            Logger.Trace("End");
        }
    }
}
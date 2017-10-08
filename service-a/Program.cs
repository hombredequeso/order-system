using System;
using System.Collections.Generic;
using System.Threading;
using Hdq.RestBus;
using Hdq.RestBus.Receiver;
using CarrierPidgin.ServiceA.Dal;
using Hdq.Lib;
using NLog;

namespace CarrierPidgin.ServiceA
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
                Deserializer.DeserializeTransportMessage(s, MessageEndpointRepository.GetMessageTypeLookup());

            foreach (var endpoint in GetMessageEndpoints())
            {
                var channelLocation = ServiceLocator.GetMessageChannelLocation(endpoint.Path);
                var messageEndpointState = new MessageEndpointState(
                    channelLocation,
                    endpoint.LastSuccessfullyProcessedMessage,
                    endpoint.Name);

                var uriBuilder = new UriBuilder(channelLocation.Scheme, channelLocation.Host, channelLocation.Port);
                IHttpService ServiceCreator() => new HttpService(uriBuilder.Uri);

                MessageEndpointPoller.MainInfinitePollerAsync(
                    messageEndpointState,
                    HandlerFactory.GetHandlerForMessageType,
                    DeserializeTransportMessage,
                    endpoint.DefaultDelayMs, endpoint.PollingErrorPolicy, ServiceCreator, ct);
            }

            Console.WriteLine("press enter to stop");
            Console.Read();
            cts.Cancel();
            Logger.Trace("End");
        }
    }
}
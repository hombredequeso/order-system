using System;
using System.Collections.Generic;
using System.Threading;
using CarrierPidgin.Lib;
using CarrierPidgin.ServiceA.Bus;
using CarrierPidgin.ServiceA.Dal;
using Hdq.Lib;
using NLog;

namespace CarrierPidgin.ServiceA
{
    internal class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static List<MessageStream> GetMessageStreams()
        {
            using (var statisticsUow = new UnitOfWork(Database.ConnectionString))
            {
                return MessageStreamRepository.GetAllMessageStreams(statisticsUow);
            }
        }

        private static void Main(string[] args)
        {
            Logger.Trace("Start");
            var cts = new CancellationTokenSource();
            var ct = cts.Token;

            Either<DeserializeError, TransportMessage> DeserializeTransportMessage(string s) => 
                MessageTransform.DeserializeTransportMessage(s, MessageStreamRepository.GetMessageTypeLookup());

            foreach (var messageStream in GetMessageStreams())
            {
                var streamLocation = ServiceLocator.GetMessageStreamLocation(messageStream.Path);
                var messageStreamState = new MessageStreamState(
                    streamLocation,
                    messageStream.LastSuccessfullyProcessedMessage,
                    messageStream.Name);

                var uriBuilder = new UriBuilder(streamLocation.Scheme, streamLocation.Host, streamLocation.Port);
                IHttpService ServiceCreator() => new HttpService(uriBuilder.Uri);

                MessageStreamPoller.MainInfinitePollerAsync(
                    messageStreamState,
                    HandlerFactory.GetHandlerForMessageType,
                    DeserializeTransportMessage,
                    messageStream.DefaultDelayMs, messageStream.PollingErrorPolicy, ServiceCreator, ct);
            }

            Console.WriteLine("press enter to stop");
            Console.Read();
            cts.Cancel();
            Logger.Trace("End");
        }
    }
}
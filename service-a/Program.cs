using System;
using System.Collections.Generic;
using System.Threading;
using CarrierPidgin.Lib;
using CarrierPidgin.ServiceA.Bus;
using CarrierPidgin.ServiceA.Dal;
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

            List<MessageStream> messageStreams = GetMessageStreams();
            var messageProcessingData = new MessageProcessingData(
                    MessageStreamRepository.GetMessageTypeLookup(),
                    HandlerFactory.GetHandlerForMessageType
                );

            foreach (var messageStream in messageStreams)
            {
                var streamLocation = ServiceLocator.GetMessageStreamLocation(messageStream.Path);
                var messageStreamState = new MessageStreamState(
                    streamLocation,
                    messageStream.LastSuccessfullyProcessedMessage,
                    messageStream.Name);

                MessageStreamPoller.MainInfinitePollerAsync(
                    messageStreamState,
                    ct,
                    messageStream.DefaultDelayMs,
                    messageStream.PollingErrorPolicy,
                    messageProcessingData);
            }

            Console.WriteLine("press enter to stop");
            Console.Read();
            cts.Cancel();
            Logger.Trace("End");
        }
    }
}
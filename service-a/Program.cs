using System;
using System.Collections.Generic;
using System.Threading;
using CarrierPidgin.Lib;
using CarrierPidgin.OrderService.Messages;
using CarrierPidgin.ServiceA.Bus;
using CarrierPidgin.TestService.Events;
using NLog;

namespace CarrierPidgin.ServiceA
{
    internal class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();


        private static Dictionary<string, Type> GetDomainMessageTypeLookup()
        {
            var allDomainMessageTypeLookup = new Dictionary<string, Type>();
            foreach (var keyValuePair in SomethingHappenedEvent.MessageTypeLookup)
            {
                allDomainMessageTypeLookup.Add(keyValuePair.Key, keyValuePair.Value);
            }
            foreach (var kvp in OrderEvents.OrderEventType)
            {
                allDomainMessageTypeLookup.Add(kvp.Value, kvp.Key);
            }
            return allDomainMessageTypeLookup;
        }

        private static void Main(string[] args)
        {
            Logger.Trace("Start");
            var cts = new CancellationTokenSource();
            var ct = cts.Token;

            List<MessageStream> messageStreams;
            using (var uow = new UnitOfWork(Statistics.Dal.Database.ConnectionString))
            {
                messageStreams = MessageStreamRepository.Get(uow);
            }

            var messageProcessingData = new MessageProcessingData(
                    GetDomainMessageTypeLookup(),
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
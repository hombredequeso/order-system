using System;
using System.Collections.Generic;
using System.Threading;
using CarrierPidgin.OrderService.Messages;
using CarrierPidgin.ServiceA.Bus;
using CarrierPidgin.ServiceA.Bus.Sample;
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


            List<TestOrderedMessageStream> messageStreams = TestMessageStreamRepository.Get();
            Dictionary<string, Type> messageTypeLookup = GetDomainMessageTypeLookup();

            var messageProcessingData = new MessageProcessingData(
                    GetDomainMessageTypeLookup(),
                    HandlerFactory.GetHandlerForMessageType
                );


            foreach (var messageStream in messageStreams)
            {
                var streamLocation = ServiceLocator.GetMessageStreamLocation(messageStream.Name);
                var messageStreamState = new MessageStreamState(
                    streamLocation,
                    messageStream.LastSuccessfullyProcessedMessage,
                    messageStream.Description);

                MessageStreamPoller.MainInfinitePollerAsync(
                    messageStreamState,
                    ct,
                    messageStream.DefaultDelayMs,
                    messageStream.PollingErrorPolicy,
                    HandlerFactory.GetHandlerForMessageType,
                    messageTypeLookup);
            }

            Console.WriteLine("press enter to stop");
            Console.Read();
            cts.Cancel();
            Logger.Trace("End");
        }
    }

    public class MessageProcessingData
    {
        public MessageProcessingData(
            Dictionary<string, Type> messageTypeLookup, 
            Func<Type, Action<DomainMessageProcessor.DomainMessageProcessingContext, object>> domainMessageProcessorLookup)
        {
            MessageTypeLookup = messageTypeLookup;
            DomainMessageProcessorLookup = domainMessageProcessorLookup;
        }

        public Dictionary<string, Type> MessageTypeLookup { get; }
        public Func<Type, Action<DomainMessageProcessor.DomainMessageProcessingContext, object>> DomainMessageProcessorLookup
        {
            get;
        }
    }
}
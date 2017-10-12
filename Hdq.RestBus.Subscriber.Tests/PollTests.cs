using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Hdq.Lib;
using NUnit.Framework;

namespace Hdq.RestBus.Subscriber.Tests
{
    public class TestMessage
    {
        public int MessageId { get; set; }

        public static string MessageName = "PollTest.TestMessage";
    }

    public class TestMessageHandler
    {
        static TestMessageHandler()
        {
            Counter = 0;
        }
        public static int Counter { get; private set; }
        public void Handle(TestMessage m)
        {
            ++Counter;
        }
    }

    [TestFixture]
    public class PollTests
    {
        public static Action<DomainMessageProcessingContext, object> GetHandlerForMessageType(
            Type messageType)
        {
            if (messageType == typeof(TestMessage))
                return (c, m) => { new TestMessageHandler().Handle((TestMessage)m); };
            return (c,m) => { };
        }

        public static PollState BasicInitialPollState(
            Dictionary<HttpChannelPoller.PollingError, uint> pollingPolicy)
        {
            return new PollState(new MessageEndpointName("testStream"),
                PollingDelays.DefaultDelayMs, pollingPolicy ?? PollingDelays.DefaultPollingErrorDelays, MessageEndpoint.NoMessagesProcessed, "/next/url", 0);
        }

        [Test]
        public async Task When_Polling_Results_In_An_Http_Error_The_Returned_PollState_Has_Delay_Set_To_The_Specified_Http_Error_Delay()
        {
            // Given:
            uint httpErrorDelay = 12345;
            var pollingPolicy = PollingDelays.DefaultPollingErrorDelays;
            pollingPolicy[HttpChannelPoller.PollingError.ErrorMakingHttpRequest] = httpErrorDelay;
            var testResponses = new List<Either<HttpError, string>>()
            {
                new Either<HttpError, string>(new HttpError(HttpStatusCode.BadRequest))
            };

            Func<string, Either<DeserializeError, TransportMessage>> deserializeTransportMessage = s =>
                TransportMessageDeserializer.Deserialize(s,
                    new Dictionary<string, Type> {{TestMessage.MessageName, typeof(TestMessage)}});
            var initialPollState = BasicInitialPollState(pollingPolicy);

            // When:
            PollState newPollStateTask = await MessageEndpointPoller.Execute(
                initialPollState, 
                new TestHttpService(testResponses), 
                new CancellationToken(), 
                GetHandlerForMessageType,
               deserializeTransportMessage );

            // Then
            Assert.NotNull(newPollStateTask);
            Assert.AreEqual(httpErrorDelay, newPollStateTask.DelayMs);
        }

        [Test]
        public void Read_Json()
        {

            var d = TestContext.CurrentContext.TestDirectory;
            Console.WriteLine(d);
            var loc = Path.Combine(d, "http-responses\\sample.json");
            Console.WriteLine(loc);

            var content = File.ReadAllText(loc);
            Console.Write(content);
        }

        [Test]
        public async Task Polling_From_Start_Works()
        {
            // Given:
            uint httpErrorDelay = 12345;
            var pollingPolicy = PollingDelays.DefaultPollingErrorDelays;
            pollingPolicy[HttpChannelPoller.PollingError.ErrorMakingHttpRequest] = httpErrorDelay;


            var loc = Path.Combine(
                TestContext.CurrentContext.TestDirectory, 
                "http-responses\\sample.json");
            var content = File.ReadAllText(loc);
            var testResponses = new List<Either<HttpError, string>>()
            {
                new Either<HttpError, string>(content)
            };

            var initialPollState = BasicInitialPollState(pollingPolicy);

            Either<DeserializeError, TransportMessage> DeserializeTransportMessage(string s) => 
                TransportMessageDeserializer.Deserialize(
                    s, 
                    new Dictionary<string, Type> {{TestMessage.MessageName, typeof(TestMessage)}});

            // When:
            PollState newPollStateTask = await MessageEndpointPoller.Execute(
                initialPollState, 
                new TestHttpService(testResponses), 
                new CancellationToken(), 
                GetHandlerForMessageType,
               DeserializeTransportMessage );

            // Then
            //Assert.AreEqual(httpErrorDelay, newPollStateTask.DelayMs);
            Assert.AreEqual(4, TestMessageHandler.Counter);
        }

    }


}
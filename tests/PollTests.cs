using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CarrierPidgin.ServiceA.Bus;
using Hdq.Lib;
using NUnit.Framework;

namespace tests
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
        
        // public static async Task<PollState> Execute(
        //     PollState ps, 
        //     IHttpService client, 
        //     CancellationToken ct,
        //     MessageProcessingData mpd)

        public static Action<DomainMessageProcessor.DomainMessageProcessingContext, object> GetHandlerForMessageType(
            Type messageType)
        {
            if (messageType == typeof(TestMessage))
                return (c, m) => { new TestMessageHandler().Handle((TestMessage)m); };
            return (c,m) => { };
        }

        public static PollState BasicInitialPollState(
            Dictionary<HttpMessagePoller.PollingError, uint> pollingPolicy)
        {
            return new PollState(
                "/next/url", 
                0,
                MessageStream.NoMessagesProcessed, 
                new MessageStreamName("testStream"),
                pollingPolicy ?? PollingPolicy.DefaultPollingErrorPolicy,
                PollingPolicy.DefaultDelayMs);
        }

        [Test]
        public async Task When_Polling_Results_In_An_Http_Error_The_Returned_PollState_Has_Delay_Set_To_The_Specified_Http_Error_Delay()
        {
            // Given:
            uint httpErrorDelay = 12345;
            var pollingPolicy = PollingPolicy.DefaultPollingErrorPolicy;
            pollingPolicy[HttpMessagePoller.PollingError.ErrorMakingHttpRequest] = httpErrorDelay;
            var testResponses = new List<Either<HttpError, string>>()
            {
                new Either<HttpError, string>(new HttpError(HttpStatusCode.BadRequest))
            };

            var initialPollState = BasicInitialPollState(pollingPolicy);
            var mpd = new MessageProcessingData(
                new Dictionary<string, Type>{{TestMessage.MessageName, typeof(TestMessage)}}, 
                GetHandlerForMessageType);

            // When:
            PollState newPollStateTask = await MessageStreamPoller.Execute(
                initialPollState, 
                new TestHttpService(testResponses), 
                new CancellationToken(), 
                mpd);

            // Then
            Assert.NotNull(newPollStateTask);
            Assert.AreEqual(httpErrorDelay, newPollStateTask.DelayMs);
        }
    }
}
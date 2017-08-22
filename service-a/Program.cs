using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using CarrierPidgin.Lib;
using Hdq.Lib;
using Newtonsoft.Json;

namespace CarrierPidgin.ServiceA
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("start");
            var cts = new CancellationTokenSource();
            var ct = cts.Token;
            MainInfinitePollerAsync(ct);
            Console.WriteLine("press enter to stop");
            Console.Read();
            cts.Cancel();
            Console.WriteLine("done");
        }

        public static async Task MainInfinitePollerAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                var pollStatus = new PollState("http://localhost:8080/teststream", 1000);
                while (pollStatus.CanPoll())
                    pollStatus = await Execute(pollStatus, ct);
            }
        }

        public static async Task<PollState> Execute(PollState ps, CancellationToken ct)
        {
            var transportMessage = await Poller.Poll(ps.NextUrl, ct);
            var pollStatus = transportMessage.Match(
                error => TransportProcessor.ProcessPollError(ps, error),
                m => TransportProcessor.ProcessTransportMessage(ps, m));

            if (pollStatus.ShouldDelay())
                await Task.Delay(pollStatus.Delay, ct);

            return pollStatus;
        }
    }

    public static class TransportProcessor
    {
        public static int lastEventNumber = -1;
        public static PollState ProcessTransportMessage(PollState pollStatus, TransportMessage transportMessage)
        {
            // TODO: need to ensure they are sequential (possibly, or not, depending on domain)
            List<DomainEvent> unprocessedMessages = transportMessage
                .Messages
                .Where(x => (int) (x.Header.EventNumber) > lastEventNumber)
                .OrderBy(x => x.Header.EventNumber)
                .ToList();

            unprocessedMessages.ForEach(MessageProcessor.ProcessMessage);

            var nextLink = transportMessage.Header.Links.SingleOrDefault(l => l.Rel.Contains("next"));
            var selfLink = transportMessage.Header.Links.Single(l => l.Rel.Contains("self"));
            return new PollState(
                (nextLink ?? selfLink).Href,
                1000);
        }

        public static PollState ProcessPollError(PollState ps, Poller.PollingError error)
        {
            switch (error)
            {
                case Poller.PollingError.UnableToConnect:
                    {
                        Console.WriteLine($"Error GET {ps.NextUrl}: Unable to connect to api");
                        return new PollState(ps.NextUrl, 5000);
                    }
                case Poller.PollingError.UnknownErrorOnGet:
                    {
                        Console.WriteLine($"Error GET {ps.NextUrl}: Unknown error on get");
                        return new PollState(ps.NextUrl, 10000);
                    }
                case Poller.PollingError.ErrorMakingHttpRequest:
                    {
                        Console.WriteLine($"Error GET {ps.NextUrl}: making request");
                        return new PollState(ps.NextUrl, 10000);
                    }
                case Poller.PollingError.ErrorDeserializingContent:
                    {
                        Console.WriteLine($"Error GET {ps.NextUrl}: This is probably never going to work");
                        return new PollState(ps.NextUrl, 10000);
                    }
                default:
                    return ps;
            }
        }
    }

    public class PollState
    {
        public PollState(string nextUrl, int delay)
        {
            NextUrl = nextUrl;
            Delay = delay;
        }

        public string NextUrl { get; }
        public int Delay { get; }

        public bool CanPoll()
        {
            return !string.IsNullOrEmpty(NextUrl);
        }

        public bool ShouldDelay()
        {
            return Delay >= 0;
        }
    }


    public static class MessageTransform
    {
        public static Either<DeserializeError, TransportMessage> Deserialize(string s)
        {
            try
            {
                var m = JsonConvert.DeserializeObject<TransportMessage>(s);
                return new Either<DeserializeError, TransportMessage>(m);
            }
            catch (JsonException e)
            {
                return new Either<DeserializeError, TransportMessage>(new DeserializeError(e));
            }
        }

        public static async Task<Either<HttpError, string>> GetContent(HttpResponseMessage m)
        {
            if (!m.IsSuccessStatusCode)
                return new Either<HttpError, string>(new HttpError(m.StatusCode));

            using (var stream = await m.Content.ReadAsStreamAsync())
            {
                using (var sr = new StreamReader(stream))
                {
                    var s = sr.ReadToEnd();
                    return new Either<HttpError, string>(s);
                }
            }
        }

        public class DeserializeError
        {
            public DeserializeError(Exception exception)
            {
                Exception = exception;
            }

            public Exception Exception { get; }
        }

        public class HttpError
        {
            public HttpError(HttpStatusCode httpErrorCode)
            {
                HttpErrorCode = httpErrorCode;
            }

            public HttpStatusCode HttpErrorCode { get; }
        }
    }

    public static class Poller
    {
        public enum PollingError
        {
            ErrorMakingHttpRequest,
            ErrorDeserializingContent,
            UnableToConnect,
            UnknownErrorOnGet
        }

        private static readonly HttpClient Client = new HttpClient();

        static Poller()
        {
            Client.BaseAddress = new Uri("http://localhost:8080/");
            Client.DefaultRequestHeaders.Accept.Clear();
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public static async Task<Either<PollingError, TransportMessage>> Poll(string path, CancellationToken ct)
        {
            Console.WriteLine($"Poller.Poll GET {path}");
            try
            {
                using (var response = await Client.GetAsync(path, ct))
                {
                    var content = await MessageTransform.GetContent(response);
                    return content.Match(
                        error => new Either<PollingError, TransportMessage>(PollingError.ErrorMakingHttpRequest),
                        c =>
                        {
                            var m = MessageTransform.Deserialize(c);
                            return m.Match(
                                left => new Either<PollingError, TransportMessage>(PollingError
                                    .ErrorDeserializingContent),
                                right => new Either<PollingError, TransportMessage>(right));
                        }
                    );
                }
            }
            catch (Exception e)
            {
                var allExceptionMessages = ExceptionProcessor.GetAllExceptionMessages(e);
                Console.WriteLine(string.Join(Environment.NewLine, allExceptionMessages));
                if (allExceptionMessages.Any(
                    m => m.Contains("No connection could be made because the target machine actively refused it")))
                    return new Either<PollingError, TransportMessage>(PollingError.UnableToConnect);
                return new Either<PollingError, TransportMessage>(PollingError.UnknownErrorOnGet);
            }
        }
    }

    public static class ExceptionProcessor
    {
        public static List<string> GetAllExceptionMessages(Exception ex)
        {
            if (ex.InnerException == null)
                return new List<string> {ex.Message};
            var otherErrors = GetAllExceptionMessages(ex.InnerException);
            otherErrors.Add(ex.Message);
            return otherErrors;
        }
    }

    public class MessageProcessingContext
    {
        protected MessageProcessingContext()
        {
            ProcessedSuccessfully = new List<DomainEvent>();
            ProcessedUnsuccessfully = new List<DomainEvent>();
        }


        public MessageProcessingContext(
            IEnumerable<DomainEvent> processedSuccessfully, 
            IEnumerable<DomainEvent> processedUnsuccessfully)
        {
            ProcessedSuccessfully = processedSuccessfully.ToList();
            ProcessedUnsuccessfully = processedUnsuccessfully.ToList();
        }

        public static MessageProcessingContext Start()
        {
            return new MessageProcessingContext();
        }

        public  MessageProcessingContext AddSuccess(DomainEvent e)
        {
            return new MessageProcessingContext(
                ProcessedSuccessfully.Concat(new[] {e}),
                ProcessedUnsuccessfully);
        }

        public  MessageProcessingContext AddFailure(DomainEvent e)
        {
            return new MessageProcessingContext(
                ProcessedSuccessfully,
                ProcessedUnsuccessfully.Concat(new[] {e}));
        }

        public List<DomainEvent> ProcessedSuccessfully { get; }
        public List<DomainEvent> ProcessedUnsuccessfully { get; }
        
    }
    public static class MessageProcessor
    {
        public static List<object> GetHandler(Type messageType)
        {
            if (messageType == typeof(SomethingHappenedEvent))
                return new List<object> {new WidgetizeWhenSomethingHappenedEventHandler()};
            return null;
        }

        public static void ProcessMessage(DomainEvent message)
        {
            Debug.WriteLine($"ProcessMessage: {message.Header}");
            var msgTypeStr = message.Header.EventType;
            var msgContent = message.Event;
            var msgType = TransportMessages.messageTypeLookup[msgTypeStr];
            var msg = JsonConvert.DeserializeObject(msgContent, msgType);
            var handlers = GetHandler(msgType);
            handlers.ForEach(h =>
            {
                var methodInfo = h.GetType().GetMethods().First(m => m.Name == "Handle");
                methodInfo.Invoke(h, new[] {msg});
            });
        }
    }
}
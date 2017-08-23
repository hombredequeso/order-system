using System;
using System.Collections.Generic;
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
using NLog;

namespace CarrierPidgin.ServiceA
{

    public class MessageStreamLocation
    {
        public MessageStreamLocation(string scheme, string host, int port, string path)
        {
            Scheme = scheme;
            Host = host;
            Port = port;
            Path = path;
        }

        public string Scheme { get; }
        public string Host { get; }
        public Int32 Port { get; }
        public string Path { get; }
    }

    public static class ServiceLocator
    {
        public static MessageStreamLocation GetMessageStreamLocation(string streamName)
        {
            if (string.IsNullOrWhiteSpace(streamName))
                return null;
            return new MessageStreamLocation("http", "localhost", 8080, streamName);
        }
    }

    internal class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static void Main(string[] args)
        {
            Logger.Trace("Start");
            var cts = new CancellationTokenSource();
            var ct = cts.Token;

            string streamName = "teststream";
            var streamLocation = ServiceLocator.GetMessageStreamLocation(streamName);

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
                client.BaseAddress = new UriBuilder(stream.Scheme, stream.Host, stream.Port).Uri;
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                while (!ct.IsCancellationRequested)
                {
                    var pollStatus = new PollState("http://localhost:8080/teststream", 1000);
                    while (pollStatus.CanPoll())
                        pollStatus = await Execute(pollStatus, client, ct);
                }
            }
        }

        public static async Task<PollState> Execute(PollState ps, HttpClient client, CancellationToken ct)
        {
            var transportMessage = await Poller.Poll(ps.NextUrl, client, ct);
            var pollStatus = transportMessage.Match(
                error => TransportProcessor.ProcessPollError(ps, error),
                m => TransportProcessor.ProcessTransportMessage(ps, m));

            if (pollStatus.ShouldDelay())
                await Task.Delay(pollStatus.Delay, ct);

            return pollStatus;
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

    public static class TransportProcessor
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public static int LastEventNumber = -1;
        public static PollState ProcessTransportMessage(PollState pollStatus, TransportMessage transportMessage)
        {
            List<DomainEvent> unprocessedMessages = transportMessage
                .Messages
                .Where(x => (int) (x.Header.EventNumber) > LastEventNumber)
                .OrderBy(x => x.Header.EventNumber)
                .ToList();

            var prevLink = transportMessage.Header.Links.SingleOrDefault(l => l.Rel.Contains(Link.Previous));
            if (unprocessedMessages.Any() &&
                prevLink != null &&
                unprocessedMessages.Min(x => (int)x.Header.EventNumber) != LastEventNumber + 1)
                return new PollState(prevLink.Href, 0);

            var initialState = MessageProcessingContext.Start();
            var finalState = unprocessedMessages.Aggregate(initialState, TransportProcessor.ProcessNext);
            LastEventNumber = finalState.ProcessedSuccessfully.Select(x => (int) x.Header.EventNumber)
                .Concat(new[] {LastEventNumber})
                .Max();


            var nextLink = transportMessage.Header.Links.SingleOrDefault(l => l.Rel.Contains(Link.Next));
            var selfLink = transportMessage.Header.Links.Single(l => l.Rel.Contains(Link.Self));
            return new PollState(
                (nextLink ?? selfLink).Href,
                1000);
        }

        private static MessageProcessingContext ProcessNext(
            MessageProcessingContext processingContext, 
            DomainEvent domainEvent)
        {
            if (processingContext.ProcessedUnsuccessfully.Any())
                return processingContext.AddUnprocessed(domainEvent);

            var processingResult = MessageProcessor.ProcessMessage(domainEvent);

            if (processingResult.GetType() == typeof(MessageProcessor.ProcessMessageSuccess))
                return processingContext.AddSuccess(domainEvent);
            if (processingResult.GetType() == typeof(MessageProcessor.DeserializationError))
                return processingContext.AddFailure(domainEvent);
            if (processingResult.GetType() == typeof(MessageProcessor.HandlerError))
                return processingContext.AddFailure(domainEvent);
            throw new Exception("Unhandled case");
        }

        public static PollState ProcessPollError(PollState ps, Poller.PollingError error)
        {
            switch (error)
            {
                case Poller.PollingError.UnableToConnect:
                    {
                        Logger.Warn($"Error GET {ps.NextUrl}: Unable to connect to api");
                        return new PollState(ps.NextUrl, 5000);
                    }
                case Poller.PollingError.UnknownErrorOnGet:
                    {
                        Logger.Error($"Error GET {ps.NextUrl}: Unknown error on get");
                        return new PollState(ps.NextUrl, 10000);
                    }
                case Poller.PollingError.ErrorMakingHttpRequest:
                    {
                        Logger.Error($"Error GET {ps.NextUrl}: making request");
                        return new PollState(ps.NextUrl, 10000);
                    }
                case Poller.PollingError.ErrorDeserializingContent:
                    {
                        Logger.Warn($"Error GET {ps.NextUrl}: This is probably never going to work");
                        return new PollState(ps.NextUrl, 10000);
                    }
                default:
                    return ps;
            }
        }
    }


    public static class MessageTransform
    {
        public static Either<DeserializeError, T> Deserialize<T>(Func<T> f)
        {
            try
            {
                var m = f();
                return new Either<DeserializeError, T>(m);
            }
            catch (JsonException e)
            {
                return new Either<DeserializeError, T>(new DeserializeError(e));
            }
        }

        public static Either<DeserializeError, T> Deserialize<T>(string s)
        {
            return Deserialize(() => JsonConvert.DeserializeObject<T>(s));
        }

        public static Either<DeserializeError, object> Deserialize(string s, Type t)
        {
            return Deserialize(() => JsonConvert.DeserializeObject(s, t));
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
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public enum PollingError
        {
            ErrorMakingHttpRequest,
            ErrorDeserializingContent,
            UnableToConnect,
            UnknownErrorOnGet
        }

        public static async Task<Either<PollingError, TransportMessage>> Poll(string path, HttpClient httpClient, CancellationToken ct)
        {
            Logger.Trace($"Poller.Poll GET {path}");
            try
            {
                using (HttpResponseMessage response = await httpClient.GetAsync(path, ct))
                {
                    var content = await MessageTransform.GetContent(response);
                    return content.Match(
                        error => new Either<PollingError, TransportMessage>(PollingError.ErrorMakingHttpRequest),
                        c =>
                        {
                            var m = MessageTransform.Deserialize<TransportMessage>(c);
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
                var allExceptionMessages = string.Join(Environment.NewLine, ExceptionProcessor.GetAllExceptionMessages(e));
                Logger.Error($"Polling Error: {allExceptionMessages}");
                return allExceptionMessages.Contains("No connection could be made because the target machine actively refused it")
                    ? new Either<PollingError, TransportMessage>(PollingError.UnableToConnect)
                    : new Either<PollingError, TransportMessage>(PollingError.UnknownErrorOnGet);
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
            Unprocessed = new List<DomainEvent>();
            ProcessedSuccessfully = new List<DomainEvent>();
            ProcessedUnsuccessfully = new List<DomainEvent>();
        }

        public MessageProcessingContext(
            IEnumerable<DomainEvent> processedSuccessfully, 
            IEnumerable<DomainEvent> processedUnsuccessfully, 
            IEnumerable<DomainEvent> unprocessed)
        {
            Unprocessed = unprocessed.ToList();
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
                ProcessedUnsuccessfully,
                Unprocessed);
        }

        public  MessageProcessingContext AddFailure(DomainEvent e)
        {
            return new MessageProcessingContext(
                ProcessedSuccessfully,
                ProcessedUnsuccessfully.Concat(new[] {e}),
                Unprocessed);
        }

        public  MessageProcessingContext AddUnprocessed(DomainEvent e)
        {
            return new MessageProcessingContext(
                ProcessedSuccessfully,
                ProcessedUnsuccessfully,
                Unprocessed.Concat(new[] {e}));
        }

        public List<DomainEvent> ProcessedSuccessfully { get; }
        public List<DomainEvent> ProcessedUnsuccessfully { get; }
        public List<DomainEvent> Unprocessed { get; }
        
    }

    public static class MessageProcessor
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static List<object> GetHandler(Type messageType)
        {
            if (messageType == typeof(SomethingHappenedEvent))
                return new List<object> {new WidgetizeWhenSomethingHappenedEventHandler()};
            return null;
        }

        public static IProcessMessageResult ProcessMessage(DomainEvent message)
        {
            Logger.Trace($"ProcessMessage: {message.Header}");
            var msgTypeStr = message.Header.EventType;
            var msgContent = message.Event;
            var msgType = TransportMessages.messageTypeLookup[msgTypeStr];

            Either<MessageTransform.DeserializeError, object> msg2 = MessageTransform.Deserialize(msgContent, msgType);

            return msg2.Match<IProcessMessageResult>(
                e => new DeserializationError(e.Exception),
                msg3 =>
                {
                    var handlers = GetHandler(msgType);
                    try
                    {
                        handlers.ForEach(h =>
                        {
                            var methodInfo = h.GetType().GetMethods().First(m => m.Name == "Handle");
                            methodInfo.Invoke(h, new[] {msg3});
                        });
                        return new ProcessMessageSuccess();
                    }
                    catch (Exception e)
                    {
                        return new HandlerError(e);
                    }
                });
        }

        public interface IProcessMessageResult
        {
        }
        public class ProcessMessageSuccess: IProcessMessageResult
        { }

        public class DeserializationError: IProcessMessageResult
        {
            public DeserializationError(Exception exception)
            {
                Exception = exception;
            }

            public Exception Exception { get; }
        }

        public class HandlerError: IProcessMessageResult
        {
            public HandlerError(Exception exception)
            {
                Exception = exception;
            }

            public Exception Exception { get; }
        }
    }
}
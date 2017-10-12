using System;
using Hdq.Lib;
using NLog;

namespace Hdq.RestBus.Subscriber
{
    public static class DomainMessageProcessor
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static readonly int HandlerRetryCount = 3;

        public static IProcessMessageResult ProcessMessage(
            DomainMessage message,
            MessageEndpointName queueName,
            Func<Type, Action<DomainMessageProcessingContext, object>> processors)
        {
            Logger.Trace($"ProcessMessage: {message.Header}");

            var context = new DomainMessageProcessingContext(
                 new Retries(HandlerRetryCount),
                 message.Header,
                 queueName
            );

            Either<DeserializeError, object> msg2 =
                new Either<DeserializeError, object>(message.Message);
            return msg2.Match(
                e => new DeserializationError(e.Exception),
                msg3 => ProcessMsg(msg3, context, processors));
        }

        public class Retries
        {
            public Retries(int maxRetries)
                :this(maxRetries, 1)
            {
            }

            private Retries(int maxRetries, int currentTry)
            {
                MaxRetries = maxRetries;
                CurrentTry = currentTry;
            }

            public int MaxRetries { get; }
            public int CurrentTry { get; }

            public Retries CreateNextRetry()
            {
                return new Retries(this.MaxRetries, this.CurrentTry + 1);
            }

            public bool DoneRetries()
            {
                return CurrentTry >= MaxRetries;
            }
        }

        public static IProcessMessageResult ProcessMsg(
            object msg, 
            DomainMessageProcessingContext messageContext,
            Func<Type, Action<DomainMessageProcessingContext, object>> domainMessageProcessorLookup)
        {
            var msgType = msg.GetType();
            var msgHandler = domainMessageProcessorLookup(msgType);
            try
            {
                msgHandler(messageContext, msg);
                return new ProcessMessageSuccess();
            }
            catch (Exception e)
            {
                if (messageContext.Retries.DoneRetries())
                {
                    Logger.Debug("Handler failed: Retries all used up. Give up");
                    return new HandlerError(e);
                }
                return ProcessMsg(
                    msg,
                    new DomainMessageProcessingContext(
                        messageContext.Retries.CreateNextRetry(),
                        messageContext.MessageHeader,
                        messageContext.SourceQueue
                        ),
                    domainMessageProcessorLookup);
            }
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
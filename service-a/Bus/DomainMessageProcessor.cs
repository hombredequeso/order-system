using System;
using CarrierPidgin.Lib;
using Hdq.Lib;
using Newtonsoft.Json.Linq;
using NLog;

namespace CarrierPidgin.ServiceA.Bus
{
    public static class DomainMessageProcessor
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static IProcessMessageResult ProcessMessage(
            DomainMessage message,
            MessageStreamName queueuName,
            MessageProcessingData mpd)
        {

            Logger.Trace($"ProcessMessage: {message.Header}");
            var msgTypeStr = message.Header.MessageType;
            var msgContent = message.Message;
            var msgType = mpd.MessageTypeLookup[msgTypeStr];
            int handlerRetryCount = 3;

            var context = new DomainMessageProcessingContext(
                 new Retries(handlerRetryCount),
                 message.Header,
                 queueuName
            );

            Either<MessageTransform.DeserializeError, object> msg2 =
                new Either<MessageTransform.DeserializeError, object>(msgContent);

            return msg2.Match(
                e => new DeserializationError(e.Exception),
                msg3 =>
                {
                    var obj = ((JObject) msg3).ToObject(msgType);
                    return ProcessMsg(obj, msgType, context, mpd.DomainMessageProcessorLookup);
                });
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

        public class DomainMessageProcessingContext
        {
            public DomainMessageProcessingContext(
                Retries retries, 
                MessageHeader messageHeader,
                MessageStreamName sourceQueue)
            {
                Retries = retries;
                MessageHeader = messageHeader;
                SourceQueue = sourceQueue;
            }

            public Retries Retries { get; }
            public MessageHeader MessageHeader { get; }
            public MessageStreamName SourceQueue { get; }
        }

        public static IProcessMessageResult ProcessMsg(
            object msg, 
            Type msgType, 
            DomainMessageProcessingContext messageContext,
            Func<Type, Action<DomainMessageProcessor.DomainMessageProcessingContext, object>> domainMessageProcessorLookup)
        {

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
                    msgType,
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
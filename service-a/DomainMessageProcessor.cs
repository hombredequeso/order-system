using System;
using System.Collections.Generic;
using System.Linq;
using CarrierPidgin.Lib;
using CarrierPidgin.OrderService.Messages;
using CarrierPidgin.TestService.Events;
using Hdq.Lib;
using NLog;

namespace CarrierPidgin.ServiceA
{
    public static class DomainMessageProcessor
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public static readonly IDictionary<string, Type> AllDomainMessageTypeLookup;

        static DomainMessageProcessor()
        {
            AllDomainMessageTypeLookup = new Dictionary<string, Type>();
            foreach (var keyValuePair in SomethingHappenedEvent.MessageTypeLookup)
            {
                AllDomainMessageTypeLookup.Add(keyValuePair);
            }
            foreach (var kvp in OrderEvents.OrderEventType)
            {
                AllDomainMessageTypeLookup.Add(kvp.Value, kvp.Key);
            }
        }

        public static IProcessMessageResult ProcessMessage(
            DomainMessage message, 
            string queueuName)
        {
            Logger.Trace($"ProcessMessage: {message.Header}");
            var msgTypeStr = message.Header.EventType;
            var msgContent = message.Message;
            var msgType = AllDomainMessageTypeLookup[msgTypeStr];
            int handlerRetryCount = 3;

            var context = new DomainMessageProcessingContext(
                 new Retries(handlerRetryCount),
                 message.Header,
                 queueuName
            );

            Either<MessageTransform.DeserializeError, object> msg2 = MessageTransform.Deserialize(msgContent, msgType);

            return msg2.Match(
                e => new DeserializationError(e.Exception),
                msg3 => ProcessMsg(msg3, msgType, context));
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
                string sourceQueue)
            {
                Retries = retries;
                MessageHeader = messageHeader;
                SourceQueue = sourceQueue;
            }

            public Retries Retries { get; }
            public MessageHeader MessageHeader { get; }
            public string SourceQueue { get; }
        }

        public static IProcessMessageResult ProcessMsg(
            object msg, 
            Type msgType, 
            DomainMessageProcessingContext messageContext)
        {
            var msgHandler = MessageHandlerLookup.GetMessageHandler(msgType);
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
                        messageContext.SourceQueue));
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
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

        public static IProcessMessageResult ProcessMessage(DomainMessage message)
        {
            Logger.Trace($"ProcessMessage: {message.Header}");
            var msgTypeStr = message.Header.EventType;
            var msgContent = message.Message;
            var msgType = AllDomainMessageTypeLookup[msgTypeStr];
            int handlerRetryCount = 3;

            Either<MessageTransform.DeserializeError, object> msg2 = MessageTransform.Deserialize(msgContent, msgType);

            return msg2.Match(
                e => new DeserializationError(e.Exception),
                msg3 => Process(msg3, msgType, handlerRetryCount));
        }

        public static IProcessMessageResult Process(object msg, Type msgType, int retries)
        {
            var handlers = MessageTypeToHandlerLookup.GetHandler(msgType);
            try
            {
                handlers.ForEach(h =>
                {
                    var methodInfo = h.GetType().GetMethods().First(m => m.Name == "Handle");
                    methodInfo.Invoke(h, new[] {msg});
                });
                return new ProcessMessageSuccess();
            }
            catch (Exception e)
            {
                if (retries == 0)
                {
                    Logger.Debug("Handler failed: Retries all used up. Give up");
                    return new HandlerError(e);
                }
                Logger.Trace($"Handler failed: {retries} remaining");
                return Process(msg, msgType, retries - 1);
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
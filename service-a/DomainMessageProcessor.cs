using System;
using System.Collections.Generic;
using System.Linq;
using CarrierPidgin.Lib;
using Hdq.Lib;
using NLog;

namespace CarrierPidgin.ServiceA
{
    public static class DomainMessageProcessor
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
            int handlerRetryCount = 3;

            Either<MessageTransform.DeserializeError, object> msg2 = MessageTransform.Deserialize(msgContent, msgType);

            return msg2.Match(
                e => new DeserializationError(e.Exception),
                msg3 => Process(msg3, msgType, handlerRetryCount));
        }


        public static IProcessMessageResult Process(object msg, Type msgType, int retries)
        {
            var handlers = GetHandler(msgType);
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
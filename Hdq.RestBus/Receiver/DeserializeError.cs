using System;

namespace Hdq.RestBus.Receiver
{
    public class DeserializeError
    {
        public DeserializeError(Exception exception)
        {
            Exception = exception;
        }

        public Exception Exception { get; }
    }
}
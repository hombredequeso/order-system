using System;

namespace CarrierPidgin.ServiceA.Bus
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
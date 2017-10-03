using System.Collections.Generic;

namespace CarrierPidgin.ServiceA.Bus
{
    public static class PollingPolicy
    {
        public static uint DefaultDelayMs = 1000;

        public static Dictionary<HttpMessagePoller.PollingError, uint> DefaultPollingErrorPolicy = new Dictionary<HttpMessagePoller.PollingError, uint>()
        {
            { HttpMessagePoller.PollingError.UnableToConnect, 5000 },
            { HttpMessagePoller.PollingError.ErrorDeserializingContent, 10000 },
            { HttpMessagePoller.PollingError.ErrorMakingHttpRequest, 10000 },
            { HttpMessagePoller.PollingError.UnknownErrorOnGet, 10000 }
        };
    }
}
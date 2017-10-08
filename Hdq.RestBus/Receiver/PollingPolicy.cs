using System.Collections.Generic;

namespace Hdq.RestBus.Receiver
{
    public static class PollingPolicy
    {
        public static uint DefaultDelayMs = 1000;

        public static Dictionary<HttpChannelPoller.PollingError, uint> DefaultPollingErrorPolicy = new Dictionary<HttpChannelPoller.PollingError, uint>()
        {
            { HttpChannelPoller.PollingError.UnableToConnect, 5000 },
            { HttpChannelPoller.PollingError.ErrorDeserializingContent, 10000 },
            { HttpChannelPoller.PollingError.ErrorMakingHttpRequest, 10000 },
            { HttpChannelPoller.PollingError.UnknownErrorOnGet, 10000 }
        };
    }
}
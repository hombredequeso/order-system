using System.Collections.Generic;

namespace Hdq.RestBus.Subscriber
{
    public static class PollingDelays
    {
        public static uint DefaultDelayMs = 1000;

        public static Dictionary<HttpChannelPoller.PollingError, uint> DefaultPollingErrorDelays = new Dictionary<HttpChannelPoller.PollingError, uint>()
        {
            { HttpChannelPoller.PollingError.UnableToConnect, 5000 },
            { HttpChannelPoller.PollingError.ErrorDeserializingContent, 10000 },
            { HttpChannelPoller.PollingError.ErrorMakingHttpRequest, 10000 },
            { HttpChannelPoller.PollingError.UnknownErrorOnGet, 10000 }
        };
    }
}
using Hdq.RestBus.Receiver;

namespace CarrierPidgin.ServiceA
{
    public static class ServiceLocator
    {
        public static HttpChannel GetMessageChannelLocation(string channelName)
        {
            if (string.IsNullOrWhiteSpace(channelName))
                return null;
            return new HttpChannel("http", "localhost", 8080, channelName);
        }
    }
}
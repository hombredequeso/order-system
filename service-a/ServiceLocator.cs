using Hdq.RestBus.Receiver;

namespace CarrierPidgin.ServiceA
{
    public static class ServiceLocator
    {
        public static MessageChannelLocation GetMessageChannelLocation(string channelName)
        {
            if (string.IsNullOrWhiteSpace(channelName))
                return null;
            return new MessageChannelLocation("http", "localhost", 8080, channelName);
        }
    }
}
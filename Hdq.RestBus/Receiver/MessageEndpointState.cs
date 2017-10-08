namespace Hdq.RestBus.Receiver
{
    public class MessageEndpointState
    {
        public MessageEndpointState(
            MessageChannelLocation channelLocation, 
            long lastSuccessfullyProcessedMessage,
            MessageEndpointName endpointName)
        {
            ChannelLocation = channelLocation;
            LastSuccessfullyProcessedMessage = lastSuccessfullyProcessedMessage;
            EndpointName = endpointName;
        }

        public MessageChannelLocation ChannelLocation { get; }
        public long LastSuccessfullyProcessedMessage { get; }
        public MessageEndpointName EndpointName { get; }
    }
}
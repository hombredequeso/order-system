namespace Hdq.RestBus.Subscriber
{
    public class MessageEndpointState
    {
        public MessageEndpointState(
            HttpChannel channel, 
            long lastSuccessfullyProcessedMessage,
            MessageEndpointName endpointName)
        {
            Channel = channel;
            LastSuccessfullyProcessedMessage = lastSuccessfullyProcessedMessage;
            EndpointName = endpointName;
        }

        public HttpChannel Channel { get; }
        public long LastSuccessfullyProcessedMessage { get; }
        public MessageEndpointName EndpointName { get; }
    }
}
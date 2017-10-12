namespace Hdq.RestBus.Subscriber
{
    public class DomainMessageProcessingContext
    {
        public DomainMessageProcessingContext(
            DomainMessageProcessor.Retries retries, 
            MessageHeader messageHeader,
            MessageEndpointName sourceQueue)
        {
            Retries = retries;
            MessageHeader = messageHeader;
            SourceQueue = sourceQueue;
        }

        public DomainMessageProcessor.Retries Retries { get; }
        public MessageHeader MessageHeader { get; }
        public MessageEndpointName SourceQueue { get; }
    }
}
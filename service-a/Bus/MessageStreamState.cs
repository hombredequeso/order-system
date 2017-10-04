namespace CarrierPidgin.ServiceA.Bus
{
    public class MessageStreamState
    {
        public MessageStreamState(
            MessageStreamLocation streamLocation, 
            long lastSuccessfullyProcessedMessage,
            MessageStreamName streamName)
        {
            StreamLocation = streamLocation;
            LastSuccessfullyProcessedMessage = lastSuccessfullyProcessedMessage;
            StreamName = streamName;
        }

        public MessageStreamLocation StreamLocation { get; }
        public long LastSuccessfullyProcessedMessage { get; }
        public MessageStreamName StreamName { get; }
    }
}
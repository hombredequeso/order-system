namespace CarrierPidgin.ServiceA.Bus
{
    public class MessageStreamState
    {
        public MessageStreamState(
            MessageStreamLocation streamLocation, 
            long lastSuccessfullyProcessedMessage,
            string description)
        {
            StreamLocation = streamLocation;
            LastSuccessfullyProcessedMessage = lastSuccessfullyProcessedMessage;
            Description = description;
        }

        public MessageStreamLocation StreamLocation { get; }
        public long LastSuccessfullyProcessedMessage { get; }
        public string Description { get; }
    }
}
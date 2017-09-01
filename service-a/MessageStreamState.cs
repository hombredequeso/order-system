namespace CarrierPidgin.ServiceA
{
    public class MessageStreamState
    {
        public MessageStreamState(
            MessageStreamLocation streamLocation, 
            int lastSuccessfullyProcessedMessage,
            string description)
        {
            StreamLocation = streamLocation;
            LastSuccessfullyProcessedMessage = lastSuccessfullyProcessedMessage;
            Description = description;
        }

        public MessageStreamLocation StreamLocation { get; }
        public int LastSuccessfullyProcessedMessage { get; }
        public string Description { get; }
    }
}
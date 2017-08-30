using System;

namespace CarrierPidgin.EventStore
{
    public class EventStoreItem
    {
        public Guid Id { get; set; }
        public int Version { get; set; }
        public string MessageType { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string SerializedMessage { get; set; }
    }
}
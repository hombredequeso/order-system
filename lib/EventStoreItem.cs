using System;

namespace Hdq.EventStore.Core
{
    public class EventStoreItem
    {
        public Guid Id { get; set; }
        public long Version { get; set; }
        public string MessageType { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string SerializedMessage { get; set; }
    }
}
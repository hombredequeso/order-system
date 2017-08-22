using System;
using System.Collections.Generic;
using System.Linq;

namespace CarrierPidgin.Lib
{
    public static class TransportMessages
    {
        public static string GetMessageName<T>(T message)
        {
            var result =
                message.GetType()
                    .GetProperties()
                    .Single(x => x.Name == "TransportMessageName")
                    .GetValue(null, null);
            return (string) result;
        }

        // Need to generate this via reflection
        public static IDictionary<string, Type> messageTypeLookup = new Dictionary<string, Type>()
        {
            {SomethingHappenedEvent.TransportMessageName, typeof(SomethingHappenedEvent) }
        };

    }

    public class SomethingHappenedEvent
    {
        public string Description { get; set; }

        public static  string TransportMessageName => "V1.SomethingHappenedEvent";
    }

    public class DomainEvent
    {
        public EventHeader Header { get; set; }
        public string Event { get; set; }
    }

    public class EventHeader
    {
        public ulong EventNumber { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string EventType { get; set; }
        public string AggregateId { get; set; }
        public ulong? VersionNumber { get; set; }

        public override string ToString()
        {
            return
                $"EventHeader: EventNumber={EventNumber}; EventType={EventType}; AggregateId={AggregateId}; VersionNumber={VersionNumber}; Timestamp={Timestamp}";
        }
    }

    public class TransportMessage
    {
        public TransportHeader Header { get; set; }
        public List<DomainEvent> Messages { get; set; }
    }

    public class Link
    {
        public string[] Rel { get; set; }
        public string Href { get; set; }

        public static string Next = "next";
        public static string Previous = "prev";
        public static string Self = "self";
    }

    public class TransportHeader
    {
        public List<Link> Links { get; set; }
    }
}
using System;
using System.Collections.Generic;

namespace CarrierPidgin.Lib
{
    public class DomainMessage
    {
        public MessageHeader Header { get; set; }
        public string Message { get; set; }
    }

    public class MessageHeader
    {
        public long MessageNumber { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string EventType { get; set; }
        public string AggregateId { get; set; }
        public long? VersionNumber { get; set; }

        public override string ToString()
        {
            return
                $"EventHeader: MessageNumber={MessageNumber}; EventType={EventType}; AggregateId={AggregateId}; VersionNumber={VersionNumber}; Timestamp={Timestamp}";
        }
    }

    public class TransportMessage
    {
        public TransportHeader Header { get; set; }
        public List<DomainMessage> Messages { get; set; }
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
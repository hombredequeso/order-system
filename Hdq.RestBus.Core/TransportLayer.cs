using System;
using System.Collections.Generic;
using System.Linq;

namespace Hdq.RestBus
{
    public class DomainMessage
    {
        public DomainMessage(MessageHeader header, object message)
        {
            Header = header;
            Message = message;
        }

        public MessageHeader Header { get; }
        public object Message { get; }
    }

    public class MessageHeader
    {
        public MessageHeader(
            long messageNumber, 
            DateTimeOffset timestamp, 
            string messageType, 
            string aggregateId, 
            long? versionNumber)
        {
            MessageNumber = messageNumber;
            Timestamp = timestamp;
            MessageType = messageType;
            AggregateId = aggregateId;
            VersionNumber = versionNumber;
        }

        public long MessageNumber { get; }
        public DateTimeOffset Timestamp { get; }
        public string MessageType { get; }
        public string AggregateId { get; }
        public long? VersionNumber { get; }

        public override string ToString()
        {
            return
                $"EventHeader: MessageNumber={MessageNumber}; EventType={MessageType}; AggregateId={AggregateId}; VersionNumber={VersionNumber}; Timestamp={Timestamp}";
        }
    }

    public class TransportMessage
    {
        public TransportMessage(TransportHeader header, List<DomainMessage> messages)
        {
            Header = header;
            Messages = messages;
        }

        public TransportHeader Header { get; }
        public List<DomainMessage> Messages { get; }
    }

    public class Link
    {
        public Link(IEnumerable<string> rel, string href)
        {
            Rel = rel.ToArray();
            Href = href;
        }

        public string[] Rel { get; }
        public string Href { get; }

        public static string Next = "next";
        public static string Previous = "prev";
        public static string Self = "self";
    }

    public class TransportHeader
    {
        public TransportHeader(IEnumerable<Link> links)
        {
            Links = links.ToList();
        }

        public List<Link> Links { get; }
    }
}
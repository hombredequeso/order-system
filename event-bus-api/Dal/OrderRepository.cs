using System;
using System.Collections.Generic;
using System.Linq;
using CarrierPidgin.EventBus.Module;
using CarrierPidgin.EventStore;
using CarrierPidgin.Lib;
using Dapper;
using Npgsql;

namespace CarrierPidgin.EventBus.Dal
{
    public static class OrderRepository
    {
        public static readonly string SelectEventStoreRowsQuery =
                $@"SELECT ""Id"", ""Version"", ""MessageType"", ""SerializedMessage"", ""Timestamp"" from ""OrderEvent"" ORDER BY ""dbId"" ASC OFFSET @offset LIMIT @limit";

        public static List<EventStoreItem> GetEvents(
            NpgsqlConnection dbConnection,
            EventRange range)
        {
            var offset = range.Start;
            var count = range.Count;
            var queryResult = dbConnection.Query<EventStoreItem>(
                SelectEventStoreRowsQuery,
                new {offset, limit = count});
            return queryResult.ToList();
        }

        public static TransportMessage GetTransportMessage(
            NpgsqlConnection dbConnection,
            EventRange range)
        {
            List<EventStoreItem> evts = GetEvents(dbConnection, range);
            List<DomainMessage> domainMsgs = evts
                .Select((x,i) => ToDomainEvent(x, range.Start + (long)i))
                .ToList();
            return new TransportMessage()
            {
                Header = new TransportHeader()
                {
                    Links = LinkBuilder.GetLinks(TransportMessageFactory.UriBuilder, "eventstream/orderdomain/order", range, evts.Count)
                },
                Messages = domainMsgs
            };
        }

        private static DomainMessage ToDomainEvent(EventStoreItem item, long messageNumber)
        {
            return new DomainMessage()
            {
                Header = new MessageHeader()
                {
                    AggregateId = item.Id.ToString(),
                    MessageNumber = messageNumber,
                    Timestamp = item.Timestamp.ToUniversalTime(),
                    EventType = item.MessageType,
                    VersionNumber = (long)item.Version
                },
                Message = item.SerializedMessage
            };
        }
    }
}
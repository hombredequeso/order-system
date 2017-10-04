using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Newtonsoft.Json;
using Npgsql;

using CarrierPidgin.OrderService.Domain;
using CarrierPidgin.EventStore;
using CarrierPidgin.OrderService.Messages;

namespace CarrierPidgin.OrderService.Dal
{
    public static class OrderRepository
    {
        public static readonly string InsertOrderEventCommand =
                @"INSERT INTO ""OrderEvent""(""Id"",""Version"",""MessageType"",""SerializedMessage"", ""Timestamp"") values (@Id,@Version,@MessageType,CAST(@SerializedMessage as json),@Timestamp)"; // @"INSERT INTO ""OrderEvent""(""Id"",""Version"",""MessageType"",""SerializedMessage"") values (@Id,@Version,@MessageType,CAST(@SerializedMessage as json))";

        public static void Save(UnitOfWork uow, Order order, DateTimeOffset timestamp)
        {
            List<EventStoreItem> eventStoreItems = order
                .NewEvents()
                .Select(e => ToEventStoreItem(e, timestamp))
                .ToList();
            uow.DbConnection.Execute(
                InsertOrderEventCommand,
                eventStoreItems, 
                uow.Transaction);
        }

        private static EventStoreItem ToEventStoreItem(IOrderEvent e, DateTimeOffset timestamp)
        {
            var serializedEvent = JsonConvert.SerializeObject(e);
            var eventType = OrderEvents.MessageTypeLookup[e.GetType()];
            
            return new EventStoreItem()
            {
                Id = e.OrderNumber,
                MessageType = eventType,
                SerializedMessage = serializedEvent,
                Version = e.Version,
                Timestamp= timestamp
            };
        }

        public static Order GetOrder(NpgsqlConnection dbConnection, Guid id)
        {
            List<IOrderEvent> evts = GetEvents(dbConnection, id);
            return Order.Build(evts);
        }

        private static List<IOrderEvent> GetEvents(NpgsqlConnection dbConnection, Guid id)
        {
            // IEnumerable<EventStoreItem> queryResult = dbConnection.Query<EventStoreItem>(
            //     $@"SELECT ""Id"", ""Version"", ""MessageType"", ""SerializedMessage"" from ""OrderEvent"" where ""Id"" ='{id}'::uuid");
            IEnumerable<EventStoreItem> queryResult = dbConnection.Query<EventStoreItem>(
                $@"SELECT ""Id"", ""Version"", ""MessageType"", ""SerializedMessage"" from ""OrderEvent"" where ""Id"" =@guidId", new {guidId = id});
            return queryResult.Select(GetEvent).OrderBy(x => x.Version).ToList();
        }

        private static IOrderEvent GetEvent(EventStoreItem dbItem)
        {
            Type type = OrderEvents.MessageTypeLookup.Single(x => x.Value == dbItem.MessageType).Key;
            var deserializedMessage = JsonConvert.DeserializeObject(dbItem.SerializedMessage, type);
            return (IOrderEvent) deserializedMessage;
        }
    }
}
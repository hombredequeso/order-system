using System;
using CarrierPidgin.EventBus.Dal;
using CarrierPidgin.Lib;
using Nancy;
using Npgsql;

namespace CarrierPidgin.EventBus.Module
{
    public class OrderModule: NancyModule
    {
        private const string ConnectionString =
            "Host=localhost;Username=postgres;Password=mypassword;Database=mydb;Search Path=order";

        public OrderModule()
        {
            Get["/eventstream/orderdomain/order/{startEventNumber},{endEventNumber}"] = parameters =>
            {
                ulong startEventNumber = ulong.Parse(parameters.startEventNumber);
                ulong endEventNumber = ulong.Parse(parameters.endEventNumber);
                EventRange eventRange = new EventRange(startEventNumber, endEventNumber, TestStreamRepository.EventCount);

                using (NpgsqlConnection dbConnection = new NpgsqlConnection(ConnectionString))
                {
                    TransportMessage m = OrderRepository.GetTransportMessage(dbConnection, eventRange);
                    return Response.AsJson(m);
                }
            };

            Get["/eventstream/orderdomain/order"] = parameters =>
            {
                throw new NotImplementedException("TODO");
            };
            
        }
        
    }
}
using System;
using System.IO;
using System.Linq;
using System.Text;
using CarrierPidgin.EventBus.Dal;
using Hdq.RestBus;
using Nancy;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;

namespace CarrierPidgin.EventBus.Module
{
    public class OrderModule: NancyModule
    {
        private const string ConnectionString =
            "Host=localhost;Username=postgres;Password=mypassword;Database=carrierpidgin;Search Path=order";

        public OrderModule()
        {
            Get["/eventstream/orderdomain/order/{startEventNumber},{endEventNumber}"] = parameters =>
            {
                long startEventNumber = long.Parse(parameters.startEventNumber);
                long endEventNumber = long.Parse(parameters.endEventNumber);
                EventRange eventRange = new EventRange(startEventNumber, endEventNumber, TestStreamRepository.EventCount);

                Console.WriteLine($"/eventstream/orderdomain/order/: {eventRange.Start} to {eventRange.End}");

                using (NpgsqlConnection dbConnection = new NpgsqlConnection(ConnectionString))
                {
                    TransportMessage m = OrderRepository.GetTransportMessage(dbConnection, eventRange);
                    return Response.AsJson(m);
                }
            };

            Get["/eventstream/orderdomain/order"] = parameters =>
            {
                throw new NotImplementedException("Not yet implemented");
            };
            
        }
        
    }
}
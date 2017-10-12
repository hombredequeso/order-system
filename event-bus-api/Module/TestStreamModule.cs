using System;
using Hdq.EventBusApi.Dal;
using Hdq.RestBus;
using Nancy;

namespace Hdq.EventBusApi.Module
{
    public class TestStreamModule: NancyModule
    {
        public TestStreamModule()
        {
            Get["/teststream/{startEventNumber},{endEventNumber}"] = parameters =>
            {
                long startEventNumber = long.Parse(parameters.startEventNumber);
                long endEventNumber = long.Parse(parameters.endEventNumber);
                EventRange eventRange = new EventRange(startEventNumber, endEventNumber, TestStreamRepository.EventCount);

                Console.WriteLine($"teststream: {eventRange.Start} to {eventRange.End}");

                TransportMessage result = TestStreamRepository.GetTransportMessage(eventRange);
                return Response.AsJson(result);
            };

            Get["/teststream"] = parameters =>
            {
                EventRange eventRange = TestStreamRepository.GetCurrent();

                Console.WriteLine($"teststream(url unspecified numbers): {eventRange.Start} to {eventRange.End}");
                var result = TestStreamRepository.GetTransportMessage(eventRange);
                return Response.AsJson(result);
            };
        }
    }
}
using System;
using Nancy;

namespace CarrierPidgin.EventBus.Module
{
    public class TestStreamModule: NancyModule
    {
        public TestStreamModule()
        {
            Get["/teststream/{startEventNumber},{endEventNumber}"] = parameters =>
            {
                ulong startEventNumber = ulong.Parse(parameters.startEventNumber);
                ulong endEventNumber = ulong.Parse(parameters.endEventNumber);
                EventRange eventRange = new EventRange(startEventNumber, endEventNumber, TestStreamRepository.EventCount);

                Console.WriteLine($"teststream: {eventRange.Start} to {eventRange.End}");

                var result = TestStreamRepository.GetTransportMessage(eventRange);
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
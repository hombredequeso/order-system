using System;
using System.Collections.Generic;

namespace Hdq.TestService.Events
{
    public class SomethingHappenedEvent
    {
        public string Description { get; set; }

        public static  string DomainMessageType => "V1.SomethingHappenedEvent";

    }

    public static class TestEvents
    {
        public static Dictionary<Type, string> MessageTypeLookup = new Dictionary<Type, string>()
        {
            {typeof(SomethingHappenedEvent), SomethingHappenedEvent.DomainMessageType }
        };
    }
}

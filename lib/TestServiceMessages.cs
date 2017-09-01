using System;
using System.Collections.Generic;

namespace CarrierPidgin.TestService.Events
{
    public class SomethingHappenedEvent
    {
        public string Description { get; set; }

        public static  string DomainMessageType => "V1.SomethingHappenedEvent";

        public static Dictionary<string, Type> MessageTypeLookup = new Dictionary<string, Type>()
        {
            {SomethingHappenedEvent.DomainMessageType, typeof(SomethingHappenedEvent) }
        };
    }
}

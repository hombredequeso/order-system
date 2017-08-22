using System;

namespace CarrierPidgin.EventBus.Module
{
    public class EventRange
    {
        public EventRange(ulong start, ulong end, ulong standardEventCount)
        {
            if (end - start + 1 != standardEventCount)
                throw new Exception("Invalid range of events");
            if (start%standardEventCount != 0)
                throw new Exception("Invalid starting point");

            Start = start;
            End = end;
        }

        public ulong Start { get; }
        public ulong End { get; }

        public ulong Count => End - Start + 1;
    }
}
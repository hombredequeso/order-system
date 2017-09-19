using System;

namespace CarrierPidgin.EventBus.Module
{
    public class EventRange
    {
        public EventRange(long start, long end, long standardEventCount)
        {
            if (end - start + 1 != standardEventCount)
                throw new Exception("Invalid range of events");
            if (start%standardEventCount != 0)
                throw new Exception("Invalid starting point");

            Start = start;
            End = end;
        }

        public long Start { get; }
        public long End { get; }

        public long Count => End - Start + 1;
    }
}
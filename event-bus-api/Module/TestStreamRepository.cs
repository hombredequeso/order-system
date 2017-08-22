using System;
using System.Collections.Generic;
using System.Linq;
using CarrierPidgin.Lib;
using Newtonsoft.Json;

namespace CarrierPidgin.EventBus.Module
{
    public static class TestStreamRepository
    {
        public static List<DomainEvent> Events;
        public static int EventsInStream = 27;
        public static string streamName = "teststream";
        public static string scheme = "http";
        public static string host = "localhost";
        public static Int32 port = 8080;
        public static UriBuilder UriBuilder;

        public const ulong EventCount = 10;
        // hence, valid event stream will be:
        // 0,9
        // 10,19
        // 20,29

        static TestStreamRepository()
        {
            UriBuilder = new UriBuilder(scheme, host, port);

            DateTimeOffset baseTimestamp = DateTimeOffset.UtcNow;

            Events = Enumerable.Range(0, EventsInStream)
                .Select(x =>
                {
                    var evt = new SomethingHappenedEvent {Description = $"Event{x}"};
                    return new DomainEvent
                        {
                            Event = JsonConvert.SerializeObject(evt),
                            Header = new EventHeader
                            {
                                EventNumber = (ulong) x,
                                Timestamp = baseTimestamp.AddSeconds(x),
                                EventType =  TransportMessages.GetMessageName(evt),
                                AggregateId = null,
                                VersionNumber = null
                            }
                        };
                })
                    .ToList();
        }

        public static DomainEvent AddEvent(SomethingHappenedEvent e)
        {
            var lastEvent = Events.Last();
                    var newEvent = new DomainEvent
                        {
                            Event = JsonConvert.SerializeObject(e),
                            Header = new EventHeader
                            {
                                EventNumber = lastEvent.Header.EventNumber + 1,
                                Timestamp = DateTimeOffset.UtcNow,
                                EventType =  TransportMessages.GetMessageName(e),
                                AggregateId = null,
                                VersionNumber = null
                            }
                        };
            Events.Add(newEvent);
            return newEvent;
        }

        public static EventRange GetCurrent()
        {
            // zero based page.
            int ps = (int) EventCount;
            var page = EventsInStream / ps;
            var firstEvent = page * ps;
            var lastEvent = ((page + 1) * ps) - 1;
            return new EventRange((ulong)firstEvent, (ulong)lastEvent, EventCount);
        }

        public static List<DomainEvent> Get(EventRange range)
        {
            int eventCount = Events.Count;

            if ((int)(range.Start + 1) > eventCount)
                return new List<DomainEvent>();

            var count = (int)range.End + 1 > eventCount
                ? eventCount - (int)range.Start
                : (int)range.Count;

            var subList = Events.GetRange((int) range.Start, count);
            return subList;
        }

        public static ulong GetPageForItem(ulong itemNumber, ulong pageSize)
        {
            return itemNumber / pageSize;
        }

        public static bool PageExists(ulong pageNumber, ulong pageSize)
        {
            return pageNumber * pageSize <= (ulong)Events.Count;
        }

        public static TransportMessage GetTransportMessage(EventRange range)
        {
            var evts = Get(range);
            var links = GetLinks(range);
            var currentPage = GetPageForItem(range.Start, range.Count);
            UriBuilder.Path = $"teststream/{range.Start},{range.End}";
            links.Add(new Link(){Rel = new[]{Link.Self}, Href = UriBuilder.ToString()});
            if (currentPage > 0)
            {
                UriBuilder.Path = $"teststream/{range.Start - range.Count},{range.End - range.Count}";

                links.Add(new Link()
                {
                    Rel = new[] {Link.Previous},
                    Href = UriBuilder.ToString()
                });
            }
            if (PageExists(currentPage + 1, range.Count))
            {
                UriBuilder.Path = $"teststream/{range.Start + range.Count},{range.End + range.Count}";

                links.Add(new Link()
                {
                    Rel = new[] {Link.Next},
                    Href = UriBuilder.ToString()
                });
            }

            return new TransportMessage()
            {
                Messages = evts,
                Header = new TransportHeader
                {
                    Links = links
                }
            };



        }

        private static List<Link> GetLinks(EventRange range)
        {
            var links = new List<Link>();

            return links;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using CarrierPidgin.EventBus.Module;
using CarrierPidgin.Lib;
using CarrierPidgin.TestService.Events;
using Newtonsoft.Json;

namespace CarrierPidgin.EventBus.Dal
{
    public static class TransportMessageFactory
    {
        public static UriBuilder UriBuilder;

        public static string scheme = "http";
        public static string host = "localhost";
        public static Int32 port = 8080;

        static TransportMessageFactory()
        {
            UriBuilder = new UriBuilder(scheme, host, port);
        }
    }

    public static class TestStreamRepository
    {
        public static List<DomainMessage> Events;
        public static int EventsInStream = 27;
        public static string StreamName = "teststream";

        public const ulong EventCount = 10;
        // hence, valid event stream will be:
        // 0,9
        // 10,19
        // 20,29

        static TestStreamRepository()
        {

            DateTimeOffset baseTimestamp = DateTimeOffset.UtcNow;

            Events = Enumerable.Range(0, EventsInStream)
                .Select(x =>
                {
                    var evt = new SomethingHappenedEvent {Description = $"Event{x}"};
                    return new DomainMessage
                        {
                            Message = JsonConvert.SerializeObject(evt),
                            Header = new MessageHeader()
                            {
                                MessageNumber = (ulong) x,
                                Timestamp = baseTimestamp.AddSeconds(x),
                                EventType =  SomethingHappenedEvent.DomainMessageType,
                                AggregateId = null,
                                VersionNumber = null
                            }
                        };
                })
                    .ToList();
        }

        public static DomainMessage AddEvent(SomethingHappenedEvent e)
        {
            var lastEvent = Events.Last();
                    var newEvent = new DomainMessage
                        {
                            Message = JsonConvert.SerializeObject(e),
                            Header = new MessageHeader()
                            {
                                MessageNumber = lastEvent.Header.MessageNumber + 1,
                                Timestamp = DateTimeOffset.UtcNow,
                                EventType =  SomethingHappenedEvent.DomainMessageType,
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

        public static List<DomainMessage> Get(EventRange range)
        {
            int eventCount = Events.Count;

            if ((int)(range.Start + 1) > eventCount)
                return new List<DomainMessage>();

            var count = (int)range.End + 1 > eventCount
                ? eventCount - (int)range.Start
                : (int)range.Count;

            var subList = Events.GetRange((int) range.Start, count);
            return subList;
        }


        public static TransportMessage GetTransportMessage(EventRange range)
        {
            List<DomainMessage> evts = Get(range);
            var links = LinkBuilder.GetLinks(TransportMessageFactory.UriBuilder, StreamName, range, evts.Count);

            return new TransportMessage()
            {
                Messages = evts,
                Header = new TransportHeader
                {
                    Links = links
                }
            };
        }
    }

    public static class LinkBuilder
    {
        public static List<Link> GetLinks(
            UriBuilder uriBuilder, 
            string eventStreamName,
            EventRange range, 
            int itemsOnCurrentPage)
        {
            var links = new List<Link>();
            var currentPage = GetPageForItem(range.Start, range.Count);
            uriBuilder.Path = $"{eventStreamName}/{range.Start},{range.End}";
            links.Add(new Link() {Rel = new[] {Link.Self}, Href = uriBuilder.ToString()});
            if (currentPage > 0)
            {
                uriBuilder.Path = $"{eventStreamName}/{range.Start - range.Count},{range.End - range.Count}";

                links.Add(new Link()
                {
                    Rel = new[] {Link.Previous},
                    Href = uriBuilder.ToString()
                });
            }
            if (NextPageExists((int)range.Count, itemsOnCurrentPage))
            {
                uriBuilder.Path = $"{eventStreamName}/{range.Start + range.Count},{range.End + range.Count}";

                links.Add(new Link()
                {
                    Rel = new[] {Link.Next},
                    Href = uriBuilder.ToString()
                });
            }
            return links;
        }

        public static bool NextPageExists(int itemsPerPage, int itemsOnCurrentPage)
        {
            return itemsOnCurrentPage == itemsPerPage;
        }
        
        public static ulong GetPageForItem(ulong itemNumber, ulong pageSize)
        {
            return itemNumber / pageSize;
        }
    }
}
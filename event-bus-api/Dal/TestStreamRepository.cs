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
        public static int InitialEventsInStream = 27;
        public static string StreamName = "teststream";

        public const long EventCount = 10;
        // hence, valid event stream will be:
        // 0,9
        // 10,19
        // 20,29

        static TestStreamRepository()
        {

            DateTimeOffset baseTimestamp = DateTimeOffset.UtcNow;

            Events = Enumerable.Range(0, InitialEventsInStream)
                .Select(x =>
                {
                    var evt = new SomethingHappenedEvent {Description = $"Event{x}"};
                    return new DomainMessage(
                        new MessageHeader(
                            x,
                            baseTimestamp.AddSeconds(x),
                            SomethingHappenedEvent.DomainMessageType,
                            null,
                            null
                        ),
                        JsonConvert.SerializeObject(
                            evt,
                            Formatting.None,
                            new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore
                            })
                    );
                })
                .ToList();
        }

        public static DomainMessage AddEvent(SomethingHappenedEvent e)
        {
            var lastEvent = Events.Last();
                    var newEvent = new DomainMessage(
                            new MessageHeader(
                                lastEvent.Header.MessageNumber + 1,
                                DateTimeOffset.UtcNow,
                                SomethingHappenedEvent.DomainMessageType,
                                null,
                                null
                            ),
                            JsonConvert.SerializeObject(
                                e,
                                Formatting.None,
                                new JsonSerializerSettings
                                {
                                    NullValueHandling = NullValueHandling.Ignore
                                })
                        );
            Events.Add(newEvent);
            return newEvent;
        }

        public static EventRange GetCurrent()
        {
            // zero based page.
            int ps = (int) EventCount;
            var page = Events.Count / ps;
            var firstEvent = page * ps;
            var lastEvent = ((page + 1) * ps) - 1;
            return new EventRange(firstEvent, lastEvent, EventCount);
        }

        public static List<DomainMessage> Get(EventRange range)
        {
            int eventCount = Events.Count;

            if ((range.Start + 1) > eventCount)
                return new List<DomainMessage>();

            var count = range.End + 1 > eventCount
                ? eventCount - range.Start
                : range.Count;

            var subList = Events.GetRange((int)range.Start, (int)count);
            return subList;
        }


        public static TransportMessage GetTransportMessage(EventRange range)
        {
            List<DomainMessage> evts = Get(range);
            var links = LinkBuilder.GetLinks(TransportMessageFactory.UriBuilder, StreamName, range, evts.Count);

            return new TransportMessage(
                new TransportHeader(links),
                evts
            );
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
            links.Add(new Link(new[] {Link.Self}, uriBuilder.ToString()));
            if (currentPage > 0)
            {
                uriBuilder.Path = $"{eventStreamName}/{range.Start - range.Count},{range.End - range.Count}";

                links.Add(new Link(
                   new[] {Link.Previous},
                    uriBuilder.ToString()
                ));
            }
            if (NextPageExists(range.Count, itemsOnCurrentPage))
            {
                uriBuilder.Path = $"{eventStreamName}/{range.Start + range.Count},{range.End + range.Count}";

                links.Add(new Link(
                    new[] {Link.Next},
                    uriBuilder.ToString()
                ));
            }
            return links;
        }

        public static bool NextPageExists(long itemsPerPage, long itemsOnCurrentPage)
        {
            return itemsOnCurrentPage == itemsPerPage;
        }
        
        public static long GetPageForItem(long itemNumber, long pageSize)
        {
            return itemNumber / pageSize;
        }
    }
}
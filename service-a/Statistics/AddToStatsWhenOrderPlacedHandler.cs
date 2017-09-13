using System;
using System.Collections.Generic;
using System.Linq;
using CarrierPidgin.OrderService.Messages;
using NLog;
using Npgsql;

namespace CarrierPidgin.ServiceA.Statistics
{
    public static class OrderStatisticsModule
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static Dictionary<Guid, int> ItemsOrdered = new Dictionary<Guid, int>();
        public static int TotalOrdersReceived = 0;
        public static void OrderReceived()
        {
            ++TotalOrdersReceived;
            Logger.Trace($"Orders received incremented to {TotalOrdersReceived}");
        }

        public static void AddToItemsOrdered(Guid itemId, int count)
        {
            if (ItemsOrdered.ContainsKey(itemId))
                ItemsOrdered[itemId] = ItemsOrdered[itemId] + count;
            else
                ItemsOrdered.Add(itemId, count);
        }

        public static void AddItemCount(Dictionary<Guid, int> itemCounts)
        {
            foreach (var itemCount in itemCounts)
                AddToItemsOrdered(itemCount.Key, itemCount.Value);
        }

        public static void LogState()
        {
            Logger.Trace($"order count: {TotalOrdersReceived}");
            var logMessage = string.Join(
                Environment.NewLine, 
                ItemsOrdered
                    .Take(20)
                    .Select(i => $"item {i.Key} = {i.Value}"));
            Logger.Trace("Items ordered:" + Environment.NewLine + logMessage);
        }
    }

    public class AddToStatsWhenOrderPlacedHandler
    {
        public void Handle(OrderPlacedEvent orderPlacedEvent)
        {
            OrderStatisticsModule.OrderReceived();
            OrderStatisticsModule.AddItemCount(orderPlacedEvent
                .Lines
                .ToDictionary(x => x.ItemId, x => x.Quantity));
            OrderStatisticsModule.LogState();
        }
    }
}
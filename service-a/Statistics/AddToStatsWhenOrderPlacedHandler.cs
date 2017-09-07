using CarrierPidgin.OrderService.Messages;
using NLog;

namespace CarrierPidgin.ServiceA.Statistics
{
    public static class OrderStatistics
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static int TotalOrdersReceived = 0;
        public static void OrderReceived()
        {
            ++TotalOrdersReceived;
            Logger.Trace($"Orders received incremented to {TotalOrdersReceived}");
        }
    }

    public class AddToStatsWhenOrderPlacedHandler
    {
        public void Handle(OrderPlacedEvent orderPlacedEvent)
        {
            OrderStatistics.OrderReceived();
        }
    }
}
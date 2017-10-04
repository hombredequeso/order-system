using System;
using CarrierPidgin.ServiceA.Entities;

namespace CarrierPidgin.ServiceA.Dal
{
    public class OrderStatisticsRow
    {
        public Guid Id { get; set; }
        public int TotalOrders { get; set; }
        public int Version { get; set; }
        public DateTimeOffset UpdatedTimestamp { get; set; }

        public OrderStatistics toDomainEntity()
        {
            return new OrderStatistics(){TotalOrders = TotalOrders};
        }
        public static Guid TotalOrdersId = Guid.Parse("7bf6db94-90e9-4019-850d-d6ff75d0d01e");
    }
}
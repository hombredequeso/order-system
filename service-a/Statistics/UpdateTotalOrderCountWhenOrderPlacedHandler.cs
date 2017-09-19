using System;
using CarrierPidgin.Lib;
using CarrierPidgin.OrderService.Messages;
using CarrierPidgin.ServiceA.Dal;
using Optional;
using Optional.Linq;

namespace CarrierPidgin.ServiceA.Statistics
{
    public class UpdateTotalOrderCountWhenOrderPlacedHandler
    {
        private readonly UnitOfWork _uow;

        public UpdateTotalOrderCountWhenOrderPlacedHandler(UnitOfWork uow)
        {
            _uow = uow;
        }

        public void Handle(OrderPlacedEvent orderPlacedEvent)
        {
            Option<Tuple<OrderStatistics, OrderStatisticsRow>> repositoryResult = 
                OrderStatisticsRepository.Get(OrderStatisticsRow.TotalOrdersId, _uow);

            var stats = repositoryResult.Map(x => x.Item1).ValueOr(new OrderStatistics());
            stats.TotalOrders++;

            var endResult = new Tuple<OrderStatistics, Option<OrderStatisticsRow>>(
                stats, 
                repositoryResult.Select(r => r.Item2));
            OrderStatisticsRepository.UpdateOrInsert(
                _uow, 
                endResult,
                OrderStatisticsRow.TotalOrdersId);
        }
    }
}
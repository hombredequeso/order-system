using CarrierPidgin.Lib;
using CarrierPidgin.OrderService.Messages;
using CarrierPidgin.ServiceA.Dal;

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
            var repositoryResult = OrderStatisticsRepository.Get(_uow, OrderStatisticsRow.TotalOrdersId);
            OrderStatistics totalOrderStatistics = repositoryResult.Item1;

            totalOrderStatistics.TotalOrders++;

            OrderStatisticsRepository.UpdateOrInsert(_uow, totalOrderStatistics, repositoryResult.Item2, OrderStatisticsRow.TotalOrdersId);
        }
    }
}
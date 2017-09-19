using System;
using CarrierPidgin.Lib;
using CarrierPidgin.ServiceA.Dal;
using CarrierPidgin.ServiceA.Statistics;
using NUnit.Framework;
using Optional;
using Optional.Linq;

namespace tests
{
    [TestFixture]
    public class DbTests
    {
        public static string domain = "statistics";


        [Test]
        public void Can_Insert_OrderStatistics_Row()
        {
            Guid id = Guid.NewGuid();
            using (var uow = new UnitOfWork(TestConfig.GetConnectionString(domain)))
            {
                OrderStatistics totalOrderStatistics = new OrderStatistics() { TotalOrders = 123 };
                OrderStatisticsRepository.Insert(uow, totalOrderStatistics, id);
                uow.Commit();
            }

            using (var uow = new UnitOfWork(TestConfig.GetConnectionString(domain)))
            {
                var repositoryResult = OrderStatisticsRepository.GetDbRow(id, uow);
                Assert.IsTrue(repositoryResult.HasValue);
            }
        }

        [Test]
        public void Can_Create_New_OrderStatisticsRow()
        {
            using (var uow = new UnitOfWork(TestConfig.GetConnectionString(domain)))
            {
                Guid id = Guid.NewGuid();
                Option<Tuple<OrderStatistics, OrderStatisticsRow>> repositoryResult =
                    OrderStatisticsRepository.Get(id, uow);

                var stats = repositoryResult.Map(x => x.Item1).ValueOr(new OrderStatistics());
                stats.TotalOrders++;

                var endResult = new Tuple<OrderStatistics, Option<OrderStatisticsRow>>(
                    stats,
                    repositoryResult.Select(r => r.Item2));
                OrderStatisticsRepository.UpdateOrInsert(
                    uow,
                    endResult,
                    id);
                uow.Commit();
            }
        }
    }
}
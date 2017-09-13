using System;
using CarrierPidgin.Lib;
using CarrierPidgin.ServiceA.Dal;
using CarrierPidgin.ServiceA.Statistics;
using NUnit.Framework;

namespace tests
{
    [TestFixture]
    public class DbTests
    {
        private const string ConnectionString =
            "Host=localhost;Username=postgres;Password=mypassword;Database=carrierpidgin;Search Path=statistics";


        [Test]
        public void Can_Insert_OrderStatistics_Row()
        {
            Guid id = Guid.NewGuid();
            using (var uow = new UnitOfWork(ConnectionString))
            {
                OrderStatistics totalOrderStatistics = new OrderStatistics() { TotalOrders = 123 };
                OrderStatisticsRepository.Insert(uow, totalOrderStatistics, id);
                uow.Commit();
            }

            using (var uow = new UnitOfWork(ConnectionString))
            {
                var repositoryResult = OrderStatisticsRepository.Get(uow, id);
                var dbRow = repositoryResult.Item2;
                Assert.NotNull(dbRow);
               
            }
        }

        [Test]
        public void Version_Test()
        {
                using (var uow = new UnitOfWork(ConnectionString))
                {
                    Guid id = OrderStatisticsRow.TotalOrdersId;
                    var repositoryResult = OrderStatisticsRepository.Get(uow, id);
                    OrderStatistics totalOrderStatistics = repositoryResult.Item1;
                    totalOrderStatistics.TotalOrders++;
                    OrderStatisticsRepository.UpdateOrInsert(uow, totalOrderStatistics, repositoryResult.Item2, id);
                    uow.Commit();
                }


        }
    }
}
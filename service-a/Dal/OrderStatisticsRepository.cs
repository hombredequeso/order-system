using System;
using System.Collections.Generic;
using System.Linq;
using CarrierPidgin.Lib;
using CarrierPidgin.ServiceA.Statistics;
using Dapper;

namespace CarrierPidgin.ServiceA.Dal
{
    public static class OrderStatisticsRepository
    {
        public static Tuple<OrderStatistics, OrderStatisticsRow>  Get(UnitOfWork uow, Guid id)
        {
            IEnumerable<OrderStatisticsRow> queryResult = uow.DbConnection.Query<OrderStatisticsRow>(
                $@"SELECT ""Id"", ""TotalOrders"", ""Version"", ""UpdatedTimestamp"" from ""OrderStatistics"" where ""Id"" = @guidId", new {guidId = id});
            var dataRow = queryResult.SingleOrDefault();
            var orderStatistics = dataRow != null
                ? dataRow.toDomainEntity()
                : new OrderStatistics();
            return new Tuple<OrderStatistics, OrderStatisticsRow>(orderStatistics, dataRow);
        }


        public static void UpdateOrInsert(
            UnitOfWork uow, 
            OrderStatistics orderStatistics, 
            OrderStatisticsRow orderStatisticsRow, 
            Guid id)
        {
            if (orderStatisticsRow != null)
            {
                Update(uow, orderStatistics, orderStatisticsRow);
            }
            else 
                Insert(uow, orderStatistics, id);
        }

        public static readonly string InsertOrderStatisticsQuery =
            @"INSERT INTO ""OrderStatistics"" ( ""Id"", ""TotalOrders"", ""Version"", ""UpdatedTimestamp"") VALUES (@Id, @TotalOrders, @Version, @UpdatedTimestamp)";

        public static void Insert(UnitOfWork uow, OrderStatistics orderStatistics, Guid id)
        {

            int rowCountUpdated = uow.DbConnection.Execute(
                InsertOrderStatisticsQuery,
                new
                {
                    Id = id,
                    TotalOrders = orderStatistics.TotalOrders,
                    Version = 1,
                    UpdatedTimestamp = DateTimeOffset.UtcNow
                },
                uow.Transaction);
        }

        public static readonly string UpdateOrderStatisticsQuery =
            @"UPDATE ""OrderStatistics"" SET ( ""TotalOrders"", ""Version"", ""UpdatedTimestamp"") = (@TotalOrders, @NewVersionNumber, @UpdatedTimestamp) WHERE ""Id"" = @Id AND ""Version"" = @ExistingVersionNumber";

        public static void Update(
            UnitOfWork uow,
            OrderStatistics orderStatistics,
            OrderStatisticsRow orderStatisticsRow)
        {
            int rowCountUpdated = uow.DbConnection.Execute(
                UpdateOrderStatisticsQuery,
                new
                {
                    Id = orderStatisticsRow.Id,
                    TotalOrders = orderStatistics.TotalOrders,
                    NewVersionNumber = orderStatisticsRow.Version + 1,
                    UpdatedTimestamp = DateTimeOffset.UtcNow,
                    ExistingVersionNumber = orderStatisticsRow.Version
                },
                uow.Transaction);

            if (rowCountUpdated != 1)
            {
                throw new Exception("Concurrency Exception, (probably!)");
            }
        }
    }
}
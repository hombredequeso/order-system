using System;
using System.Collections.Generic;
using System.Linq;
using CarrierPidgin.Lib;
using CarrierPidgin.ServiceA.Entities;
using Dapper;
using Optional;

namespace CarrierPidgin.ServiceA.Dal
{
    public static class DbRepository
    {
        public static Option<TDbRow> GetSingle<TDbRow>(
            string getQuery,
            object queryParameters,
            UnitOfWork uow)
        {
            IEnumerable<TDbRow> queryResult = uow.DbConnection.Query<TDbRow>(
                getQuery,
                queryParameters);
            TDbRow dataRow = queryResult.SingleOrDefault();
            return dataRow == null
                ? Option.None<TDbRow>()
                : Option.Some(dataRow);
        }

        public static Tuple<TEntity, TDbRow> GetById<TEntity, TDbRow>(
            string getQuery, 
            Func<TDbRow, TEntity> toEntity, 
            TEntity defaultEntityValue, 
            object queryParameters, 
            UnitOfWork uow)
        {
            IEnumerable<TDbRow> queryResult = uow.DbConnection.Query<TDbRow>(
                getQuery,
                queryParameters);
            TDbRow dataRow = queryResult.SingleOrDefault();
            var orderStatistics = dataRow != null
                ? toEntity(dataRow)
                : defaultEntityValue;
            return new Tuple<TEntity, TDbRow>(orderStatistics, dataRow);
        }
    }


    public static class OrderStatisticsRepository
    {
        public static string getQuery =
                $@"SELECT ""Id"", ""TotalOrders"", ""Version"", ""UpdatedTimestamp"" from ""OrderStatistics"" where ""Id"" = @guidId";

        public static Option<OrderStatisticsRow> GetDbRow(Guid id, UnitOfWork uow)
        {
            return DbRepository.GetSingle<OrderStatisticsRow>(getQuery, new {guidId = id}, uow);
        }

        public static Option<Tuple<OrderStatistics, OrderStatisticsRow>> Get(Guid id, UnitOfWork uow)
        {
            return GetDbRow(id, uow)
                .Map(r => new Tuple<OrderStatistics, OrderStatisticsRow>(r.toDomainEntity(), r));
        }


        public static void UpdateOrInsert(
            UnitOfWork uow, 
            Tuple<OrderStatistics, Option<OrderStatisticsRow>> x,
            Guid id)
        {
            var dbEntity = x.Item2;
            var entity = x.Item1;
            dbEntity.Match(
                r => Update(uow, entity, r),
                () => Insert(uow, entity, id));
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
                throw new Exception("OrderStatistics: Concurrency Exception, (probably!)");
            }
        }
    }
}
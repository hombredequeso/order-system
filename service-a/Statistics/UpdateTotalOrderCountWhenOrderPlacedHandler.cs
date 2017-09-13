using System;
using CarrierPidgin.Lib;
using CarrierPidgin.OrderService.Messages;
using CarrierPidgin.ServiceA.Dal;

namespace CarrierPidgin.ServiceA.Statistics
{
    public static class HandleConstruction
    {
        private const string ConnectionString =
            "Host=localhost;Username=postgres;Password=mypassword;Database=carrierpidgin;Search Path=statistics";

        public static object GetHandler()
        {
            var transactionHander = new TransactionHandler<OrderPlacedEvent>(ConnectionString);
            transactionHander.Next = e =>
            {
                var innerHandler = new UpdateTotalOrderCountWhenOrderPlacedHandler(transactionHander.Uow);
                innerHandler.Handle(e);
            };
            return transactionHander;
        }


        public static Action<OrderPlacedEvent> GetHandlerWithDeDup2()
        {
            UnitOfWork unitOfWorkContainer = null;
            Action<OrderPlacedEvent> businessHandlerAction = e =>
            {
                var businessHandler = new UpdateTotalOrderCountWhenOrderPlacedHandler(unitOfWorkContainer);
                businessHandler.Handle(e);
            };

            Action<OrderPlacedEvent> deDupAction = e =>
            {
                var handler = new DeDupHandler<OrderPlacedEvent>(unitOfWorkContainer);
                handler._next = businessHandlerAction;
                handler.Handle(e);
            };

            Action<OrderPlacedEvent> transHandlerAction = e =>
            {
                var handler = new TransactionHandler<OrderPlacedEvent>(
                    ConnectionString,
                    uow => unitOfWorkContainer = uow);
                handler.Next = deDupAction;
                handler.Handle(e);
            };
            return transHandlerAction;
        }

        public static object GetHandlerWithDeDup()
        {
            UnitOfWork unitOfWorkContainer = null;
            var transactionHander = new TransactionHandler<OrderPlacedEvent>(
                ConnectionString,
                uow => unitOfWorkContainer = uow);

            transactionHander.Next = e =>
            {
                var handler = new DeDupHandler<OrderPlacedEvent>(unitOfWorkContainer);
                handler._next = f =>
                {
                    var businessHandler = new UpdateTotalOrderCountWhenOrderPlacedHandler(unitOfWorkContainer);
                    businessHandler.Handle(f);
                };
            };

            return transactionHander;
        }
    }

    public class TransactionHandler<T>
    {
        private readonly Action<UnitOfWork> _registerUnitOfWork;

        public TransactionHandler(
            string connectionString,
            Action<UnitOfWork> registerUnitOfWork = null)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

            ConnectionString = connectionString;
            _registerUnitOfWork = registerUnitOfWork;
        }

        public string ConnectionString { get; private set; }
        public Action<T> Next {get; set; }
        public UnitOfWork Uow { get; private set; }

        public void Handle(T evt)
        {
            using (Uow = new UnitOfWork(ConnectionString))
            {
                _registerUnitOfWork?.Invoke(Uow);
                Next(evt);
                Uow.Commit();
            }
        }
    }

    public class DeDupHandler<T>
    {
        private readonly UnitOfWork _uow;
        public Action<T> _next;

        public DeDupHandler(UnitOfWork uow)
        {
            _uow = uow;
        }

        public void Handle(T evt)
        {
            if (!IsDuplicate(evt))
                _next(evt);
        }

        private bool IsDuplicate(T evt)
        {
            return false;
        }
    }

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
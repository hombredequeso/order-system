using System;
using CarrierPidgin.Lib;
using CarrierPidgin.OrderService.Messages;

namespace CarrierPidgin.ServiceA.Statistics
{
    public static class HandleConstruction
    {
        private const string ConnectionString =
            "Host=localhost;Username=postgres;Password=mypassword;Database=carrierpidgin;Search Path=statistics";

        public static Action<OrderPlacedEvent> GetOrderPlacedHandler()
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
    }
}
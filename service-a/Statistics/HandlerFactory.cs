using System;
using CarrierPidgin.Lib;
using CarrierPidgin.OrderService.Messages;
using CarrierPidgin.ServiceA.Bus;

namespace CarrierPidgin.ServiceA.Statistics
{
    public static class HandlerFactory
    {
        public static Action<DomainMessageProcessor.DomainMessageProcessingContext, OrderPlacedEvent> GetOrderPlacedHandler()
        {
            UnitOfWork unitOfWorkContainer = null;
            Action<OrderPlacedEvent> businessHandlerAction = e =>
            {
                var businessHandler = new UpdateTotalOrderCountWhenOrderPlacedHandler(unitOfWorkContainer);
                businessHandler.Handle(e);
            };

            Action<DomainMessageProcessor.DomainMessageProcessingContext, OrderPlacedEvent> deDupAction = (c,e) =>
            {
                var handler = new DeDupHandler<OrderPlacedEvent>(unitOfWorkContainer);
                handler._next = businessHandlerAction;
                string queueNamme = null;
                handler.Handle(e, c.SourceQueue, c.MessageHeader.MessageNumber);
            };

            Action<DomainMessageProcessor.DomainMessageProcessingContext, OrderPlacedEvent> transHandlerAction = (c,e) =>
            {
                var handler = new TransactionHandler<OrderPlacedEvent>(
                    Dal.Database.ConnectionString,
                    uow => unitOfWorkContainer = uow);
                handler.Next = deDupAction;
                handler.Handle(c, e);
            };
            return transHandlerAction;
        }
    }
}
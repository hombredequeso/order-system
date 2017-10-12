using System;
using Hdq.RestBus;
using Hdq.OrderApi.Messages;
using Hdq.RestBus.Subscriber;

namespace Hdq.Statistics.Handlers
{
    public static class HandlerFactory
    {
        public static Action<DomainMessageProcessingContext, OrderPlacedEvent> GetOrderPlacedHandler()
        {
            UnitOfWork unitOfWorkContainer = null;
            Action<OrderPlacedEvent> businessHandlerAction = e =>
            {
                var businessHandler = new UpdateTotalOrderCountWhenOrderPlacedHandler(unitOfWorkContainer);
                businessHandler.Handle(e);
            };

            Action<DomainMessageProcessingContext, OrderPlacedEvent> deDupAction = (c,e) =>
            {
                var handler = new DeDupHandler<OrderPlacedEvent>(unitOfWorkContainer);
                handler._next = businessHandlerAction;
                string queueNamme = null;
                handler.Handle(e, c.SourceQueue, c.MessageHeader.MessageNumber);
            };

            Action<DomainMessageProcessingContext, OrderPlacedEvent> transHandlerAction = (c,e) =>
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
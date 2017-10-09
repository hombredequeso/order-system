using System;
using Hdq.RestBus;
using Hdq.RestBus.Receiver;

namespace CarrierPidgin.ServiceA.Handlers
{
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
        public Action<DomainMessageProcessingContext, T> Next {get; set; }
        public UnitOfWork Uow { get; private set; }

        public void Handle(DomainMessageProcessingContext ctx, T evt)
        {
            using (Uow = new UnitOfWork(ConnectionString))
            {
                _registerUnitOfWork?.Invoke(Uow);
                Next(ctx, evt);
                Uow.Commit();
            }
        }
    }
}
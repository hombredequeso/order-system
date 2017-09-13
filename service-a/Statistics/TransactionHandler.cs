using System;
using CarrierPidgin.Lib;

namespace CarrierPidgin.ServiceA.Statistics
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
}
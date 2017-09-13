using System;
using CarrierPidgin.Lib;

namespace CarrierPidgin.ServiceA.Statistics
{
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
}
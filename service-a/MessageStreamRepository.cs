using System.Collections.Generic;
using System.Linq;
using CarrierPidgin.Lib;
using CarrierPidgin.OrderService.Messages;
using CarrierPidgin.ServiceA.Bus;
using CarrierPidgin.ServiceA.TestDomain;

namespace CarrierPidgin.ServiceA
{
    public static class MessageStreamRepository
    {
        public static List<MessageStream> Get(UnitOfWork uow)
        {
            return TestStreamRepository.Get()
                .Concat(OrderStreamRepository.Get(uow))
                .ToList();
        }
    }
}
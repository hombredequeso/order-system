using System;
using System.Collections.Generic;

namespace CarrierPidgin.OrderService.Messages
{
    public interface IOrderEvent
    {
        Guid OrderNumber { get; set; }
        int Version { get; set; }
    }

    public class OrderPlacedEvent: IOrderEvent
    {
        public Guid OrderNumber { get; set; }
        public int Version { get; set; }
        public string Description { get; set; }
    }

    public class OrderCancelledEvent: IOrderEvent
    {
        public Guid OrderNumber { get; set; }
        public int Version { get; set; }
    }

    public class OrderActivatedEvent: IOrderEvent
    {
        public Guid OrderNumber { get; set; }
        public int Version { get; set; }
    }

    public static class OrderEvents
    {
        public static IDictionary<Type, string> OrderEventType = new Dictionary<Type, string>()
        {
            {typeof(OrderPlacedEvent) , "Product.OrderService.OrderPlacedEvent"},
            {typeof(OrderCancelledEvent) , "Product.OrderService.OrderCancelledEvent"},
            {typeof(OrderActivatedEvent) , "Product.OrderService.OrderActivatedEvent"},
        }; 
    }
}
using System;
using System.Collections.Generic;

namespace Hdq.OrderApi.Messages
{
    public interface IOrderEvent
    {
        Guid OrderNumber { get; set; }
        int Version { get; set; }
    }

    public class OrderLine
    {
        public OrderLine(Guid itemId, int quantity, decimal pricePerItem)
        {
            if (quantity == 0)
                throw new ArgumentException("Cannot be 0", nameof(quantity));
            if (itemId == Guid.Empty)
                throw new ArgumentException("Cannot be empty", nameof(itemId));
            ItemId = itemId;
            Quantity = quantity;
            PricePerItem = pricePerItem;
        }

        public Guid ItemId { get; }
        public int Quantity { get; }
        public decimal PricePerItem { get; }
    }

    public class OrderPlacedEvent: IOrderEvent
    {
        public Guid OrderNumber { get; set; }
        public int Version { get; set; }
        public string Description { get; set; }
        public List<OrderLine> Lines { get; set; }
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
        public static IDictionary<Type, string> MessageTypeLookup = new Dictionary<Type, string>()
        {
            {typeof(OrderPlacedEvent) , "Product.OrderService.OrderPlacedEvent"},
            {typeof(OrderCancelledEvent) , "Product.OrderService.OrderCancelledEvent"},
            {typeof(OrderActivatedEvent) , "Product.OrderService.OrderActivatedEvent"},
        }; 
    }
}
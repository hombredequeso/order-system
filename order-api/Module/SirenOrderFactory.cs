using System;
using System.Collections.Generic;
using System.Linq;
using CarrierPidgin.OrderService.ApiEntity;
using CarrierPidgin.OrderService.Domain;
using Action = CarrierPidgin.OrderService.ApiEntity.Action;
using OrderLine = CarrierPidgin.OrderService.ApiEntity.OrderLine;

namespace CarrierPidgin.OrderService.Module
{

    public static class SirenOrderFactory
    {


        public static string GetStatus(Order.State state)
        {
            switch (state)
            {
                case Order.State.Active: return "active";
                case Order.State.Cancelled: return "cancelled";
                default: throw new Exception("Unknown order state");
            }
        }


        public static SirenEntity Get(Order order, string siteBase)
        {
            var resourceUrl = new Uri(new Uri(siteBase), $"order/{order.OrderNumber}");
            var links = new List<Link>()
            {
                new Link() {Href = $"{resourceUrl}", Rel = new[] {"self"}}
            };

            var actions = new List<Action>();

            if (order.Status == Order.State.Active)
            {
                actions.Add(new Action(){Name="cancel-order", HRef = $"{resourceUrl}/cancellation", Method= "POST"});
            }

            if (order.Status == Order.State.Cancelled)
            {
                actions.Add(new Action(){Name="activate-order", HRef = $"{resourceUrl}/activation", Method= "POST"});
            }

            var orderProperties = new OrderSirenProperties()
            {
                Description = order.Description,
                Id = order.OrderNumber,
                Status = GetStatus(order.Status),
                Lines = order.Lines.Select(GetLine).ToList()
            };

            var result = new SirenEntity()
            {
                Class = new List<string>() {"order"},
                Links = links,
                Actions = actions,
                Properties = orderProperties
            };
            return result;
        }


        public static OrderLine GetLine(Domain.OrderLine domainLine)
        {
            return new OrderLine()
            {
                ItemId = domainLine.ItemId,
                PricePerItem = domainLine.PricePerItem,
                Quantity = domainLine.Quantity
            };
        }
    }
}
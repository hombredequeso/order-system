using System;
using System.Collections.Generic;

namespace Hdq.OrderApi.ApiEntity
{
    public class OrderSirenProperties
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public List<OrderLine> Lines { get; set; }
    }
}
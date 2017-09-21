using System.Collections.Generic;

namespace CarrierPidgin.OrderService.ApiEntity
{
    public class PostOrderBody
    {
        public PostOrderBody()
        {
            Lines = new List<OrderLine>();
        }

        public string Description { get; set; }
        public List<OrderLine> Lines { get; set; }
    }
}
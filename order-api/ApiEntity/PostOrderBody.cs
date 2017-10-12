using System.Collections.Generic;

namespace Hdq.OrderApi.ApiEntity
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
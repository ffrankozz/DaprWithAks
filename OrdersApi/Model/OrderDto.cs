using System.Collections.Generic;

namespace OrdersApi.Model
{
    public class OrderDto
    {

        public string CustomerCode { get; set; }

        public List<OrderItem> Items { get; set; }
    }
}

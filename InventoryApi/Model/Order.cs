using System;
using System.Collections.Generic;

namespace InventoryApi.Model
{
    public class Order
    {

        public string CustomerCode { get; set; }
        public Guid OrderId { get; set; }
        public DateTime CreatedOn { get; set; }
        public List<OrderItem> Items { get; set; }

    }
}

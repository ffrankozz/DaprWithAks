using System;

using System.Collections.Generic;
using InventoryApi.Model;

namespace InventoryApi.State
{
    public class InventoryState
    {
        public Guid OrderId { get; set; }
        public List<OrderItem> OrderedItems { get; set; }
        public string CustomerCode { get; set; }
        public DateTime CreatedOn { get; set; }


    }
}

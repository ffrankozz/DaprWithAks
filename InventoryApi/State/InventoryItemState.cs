using System.Collections.Generic;
 

namespace InventoryApi.State
{
    public class InventoryItemState
    {
        public string SKU { get; set; }
        public int BalanceQuantity { get; set; }

        public List<InventoryItemAdjustment> Changes { get; set; }


    }
}

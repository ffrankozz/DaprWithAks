using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SharedLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InventoryApi.Model;
using InventoryApi.State;
using System.Text.Json;



namespace InventoryApi.Controllers
{
    [ApiController]
    
    public class InventoryController : ControllerBase
    {
        

        private readonly ILogger<InventoryController> _logger;

        public const string InventoryStoreName = "inventorystore";
        public const string InventoryItemStoreName = "inventoryitemstore";
        public const string PubSub = "messagebus";


        public InventoryController(ILogger<InventoryController> logger)
        {
            _logger = logger;
        }

        [Topic(PubSub,CommonPubSubTopics.OrderCreatedTopicName)]
        [HttpPost(CommonPubSubTopics.OrderCreatedTopicName)]
        public async Task<ActionResult> InventoryAdjustment(Order  order, [FromServices] DaprClient daprClient )
        {
            //let uscheck if exists any order in the store using the OrderId as key
            var inventoryState = await daprClient.GetStateEntryAsync<InventoryState>(InventoryStoreName, order.OrderId.ToString());
            inventoryState.Value ??= new InventoryState() {OrderId = order.OrderId, OrderedItems = order.Items, 
                CustomerCode = order.CustomerCode, CreatedOn = order.CreatedOn };
            //Let us iterate over the order items and get thier code and quantity
            foreach(var item in order.Items.ToList())
            {
                var SKU = item.ItemCode;
                var quantity = item.Quantity;
                var itemState = await daprClient.GetStateEntryAsync<InventoryItemState>(InventoryItemStoreName, SKU);
                itemState.Value ??= new InventoryItemState() { SKU = SKU, Changes = new List<InventoryItemAdjustment>() ,
                    BalanceQuantity = 100} ;
                //Update the balance(since it is an order and not refill, it will be a substraction
                itemState.Value.BalanceQuantity -= quantity;
                InventoryItemAdjustment change = new InventoryItemAdjustment() { SKU = SKU, Quantity = quantity, 
                    Action = "Sold", AdjustedOn = DateTime.UtcNow };
                itemState.Value.Changes.Add(change);
                if(itemState.Value.Changes.Count > 5) { itemState.Value.Changes.RemoveAt(0); }
                //save item state with its changes
                await itemState.SaveAsync();
                //orderState.Value.OrderedItems.Add(new OrderItem { ItemCode =SKU, Quantity = quantity});
                Console.WriteLine($"Reservation in {order.OrderId} of {SKU}  for  { quantity}, balance {itemState.Value.BalanceQuantity}");

            }
            await inventoryState.SaveAsync();
            await daprClient.PublishEventAsync<Order>(PubSub, CommonPubSubTopics.OrderPreparedTopicName, order);
            Console.WriteLine($"Preparation for {order.OrderId} completed");
            return Ok();
        }

        [HttpGet("balance/{state}")]
        public ActionResult<InventoryItemState> Get([FromState(InventoryItemStoreName)] StateEntry<InventoryItemState> state,
            [FromServices] DaprClient daprClient)
        {
            Console.WriteLine("Entered Item State retrieval");
            if(state.Value == null)
            {
                return NotFound();

            }

            var result = new InventoryItemState() { SKU = state.Value.SKU, BalanceQuantity =state.Value.BalanceQuantity, Changes = state.Value.Changes};
            Console.WriteLine($"Retrieved {result.SKU} is {result.BalanceQuantity}");
            return result;

        }

        [HttpPost("/stockrefill")]
        public async Task<ActionResult> Refill([FromServices] DaprClient daprClient)
        {

            using (var reader = new System.IO.StreamReader(Request.Body))
            {
                var body = await reader.ReadToEndAsync();
                var item = JsonSerializer.Deserialize<dynamic>(body);
                var SKU = item.GetProperty("SKU").GetString();
                var Quantity = item.GetProperty("Quantity").GetInt32();
                var itemState = await daprClient.GetStateEntryAsync<InventoryItemState>(InventoryItemStoreName, SKU);
                itemState.Value ??= new InventoryItemState()
                {
                    SKU = SKU,
                    Changes = new List<InventoryItemAdjustment>()
                };

                itemState.Value.Changes.Add(new InventoryItemAdjustment()
                {
                    SKU = SKU,
                    Quantity = Quantity,
                    Action = "Refill",
                    AdjustedOn = DateTime.UtcNow
                });
                //We can update the balance now
                itemState.Value.BalanceQuantity += Quantity;
                // save the itemstate balance and its changes collection
                await itemState.SaveAsync();
                Console.WriteLine($"Refill of {SKU} for quantity {Quantity}, new balance {itemState.Value.BalanceQuantity}");


            }
            return Ok();


        }
     }

 } 

 

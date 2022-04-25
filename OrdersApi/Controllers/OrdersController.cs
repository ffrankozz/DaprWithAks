using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrdersApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapr;
using Dapr.Client;
using SharedLib;
using System.Text.Json;

namespace OrdersApi.Controllers
{
    [ApiController]
   
    public class OrdersController : ControllerBase
    {

        public const string StoreName = "orderstore";
        public const string PubSub = "messagebus";

        private readonly ILogger<OrdersController> _logger;

        public OrdersController(ILogger<OrdersController> logger)
        {
            _logger = logger;
        }

        [HttpPost("order")]
        public async Task<ActionResult<Guid>> CreateOrder(OrderDto orderDto, [FromServices] DaprClient daprClient)  
        {
            if (!Validate(orderDto)) { return BadRequest(); }
            //Create a proper order object from orderDto object
            //Create the Order model class
            Order order = new Order
            {
                OrderId = Guid.NewGuid(),
                OrderStatus ="Created",
                CustomerCode = orderDto.CustomerCode,
                OrderCreatedOn = DateTime.UtcNow,
                Items = orderDto.Items

            };

            //Save the order to state store using the OrderId as key
            //Use daprClient and its SaveState method
            await daprClient.SaveStateAsync<Order>(StoreName, order.OrderId.ToString(), order);
            await daprClient.PublishEventAsync<Order>(PubSub, CommonPubSubTopics.OrderCreatedTopicName, order);
            Console.WriteLine($"Created order {order.OrderId}");
            return  order.OrderId;


        }

        [HttpGet("order/{state}")]
        public ActionResult<Order> Get([FromState(StoreName)] StateEntry<Order> state)
        {
            if (state.Value == null)
            {
                return this.NotFound();
            }
            var result = state.Value;

            Console.WriteLine($"Retrieved order {result.OrderId} ");

            return result;
        }


        private static bool Validate(OrderDto orderDto)
       {

            var summedItem =
                from item in orderDto.Items
                group item by item.ItemCode into items
                select new
                {
                    ProductCode = items.Key,
                    Quantity = items.Sum(x => x.Quantity)
                };
            return true;


        }

        



    }
}

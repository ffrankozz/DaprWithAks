using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using System.Text.Json;
using System.Text;
namespace Supplier
{
    internal class Program
    {
        static async Task Main(string[] args)
        {



            Console.WriteLine("Started sending!");

            string connString = "Endpoint=sb://daprbindingeventhub.servicebus.windows.net/;SharedAccessKeyName=sendlisten;SharedAccessKey=EOFbVd3nMcGWU0K+oaq8S9+EbsJf9GG31eLvI3SvoYA=;EntityPath=stockrefill";
            var products = new List<string> { "Shoes111","Jacket222","Jacket223", "Shoes112"};
            var rnd = new  Random();
            await using (var producerClient = new EventHubProducerClient(connString)) {
                do
                {
                    using(EventDataBatch eventBatch = await producerClient.CreateBatchAsync())
                    {
                        for(int i = 0; i < 5; i++)
                        {
                            var item = new
                            {
                                SKU= products[rnd.Next(products.Count)],
                                Quantity =1
                            };
                            var message = JsonSerializer.Serialize(item);
                            eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes(message)));
                        }

                        await  producerClient.SendAsync(eventBatch);
                        Console.WriteLine($"Sent batch @ {DateTime.Now.ToLongTimeString()}");
                    }

                    System.Threading.Thread.Sleep(rnd.Next(200, 3000));
                }

                while (true);
            }

        }
    }
}

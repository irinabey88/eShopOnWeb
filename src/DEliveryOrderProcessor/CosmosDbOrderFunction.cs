using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using DeliveryOrderProcessor;
using Microsoft.Azure.Cosmos;

namespace DEliveryOrderProcessor
{
    public static class CosmosDbOrderFunction
    {
        // The Azure Cosmos DB endpoint for running this sample.
        private static readonly string EndpointUri = "https://orders-cosmos-db.documents.azure.com:443/";
        // The primary key for the Azure Cosmos account.
        private static readonly string PrimaryKey = "su9dq4KnqhbwGQB8ym7NoTXuZXcu4Kg8D9ajBt3NACK2FbEwPG2SqR5PERr6qex9F9jPCXZ9kpXSLBQe3lNWyA==";
        // The name of the database and container we will create
        private static string databaseId = "OrdersDb";
        private static string containerId = "Orders";

        [FunctionName("OrderCosmosDBProcessing")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
             ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");


            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject<CreateOrderRequest>(requestBody);
            var orderDetailsData = data?.OrderDetails;

            await AddItemsToContainerAsync(orderDetailsData);

            string responseMessage = string.IsNullOrEmpty(orderDetailsData)
                ? "This HTTP triggered function executed successfully."
                : $"Hello, {orderDetailsData}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }

        private static async Task AddItemsToContainerAsync(string order)
        {
            var cosmosClient = new CosmosClient(EndpointUri, PrimaryKey);

            Database database = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
            Container container = await database.CreateContainerIfNotExistsAsync(containerId, "/id");

            var orderDetails = new OrderDetails
            { 
                Id = Guid.NewGuid().ToString(),
                Order = order
            };

            try
            {
                // Read the item to see if it exists.  
                ItemResponse<OrderDetails> andersenFamilyResponse = await container.CreateItemAsync<OrderDetails>(orderDetails, new PartitionKey(orderDetails.Id));
            }
            catch (CosmosException ex)
            {
            }

        }
    }
}

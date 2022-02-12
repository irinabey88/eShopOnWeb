using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Microsoft.Azure.WebJobs;

using Microsoft.Extensions.Logging;

namespace OrderItemsReserver
{
    public static class OrderToBlobFunction
    {
        private const string ConnectionStringEmail = "Endpoint=sb://azfinaltaskordersservicebus.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=XaJzStsH3gZmIm7pou+j4RR3fTJ7qQKOIc9RQoQMBHA=";
        private const string QueueName = "emailsqueue";

        private const string connectionStringBlob = "DefaultEndpointsProtocol=https;AccountName=orderstorageacc;AccountKey=qonC5PUpSIaK0X8TuL57/XMkybYnxFwhdaQ6pQxFJD8IDGj5R6NFnwNLnwVC6P0xZ2RJ554QDXmi+AStEzGoCQ==;EndpointSuffix=core.windows.net";
        private const string containerName = "orders";

        [FunctionName("OrderToBlobFunction")]
        public static void Run([ServiceBusTrigger("ordersqueue", Connection = "ServiceBusConnection")]string myQueueItem, int deliveryCount, ILogger log)
        {
            log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");

            try
            {
                WriteToBlob(myQueueItem);
            }
            catch (Exception ex)
            {
                SendMessageAsync(myQueueItem).GetAwaiter();
                log.LogError(ex.Message);
            }

        }

        private static void WriteToBlob(string data)
        {
            if (!string.IsNullOrWhiteSpace(data))
            {
                var cloudFilePath = @$"{DateTime.Now.ToString("yyyy-MM-dd")}-{Guid.NewGuid()}";

                BlobServiceClient blobServiceClient = new BlobServiceClient(connectionStringBlob);
                // Get the container (folder) the file will be saved in
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                // Get the Blob Client used to interact with (including create) the blob
                BlobClient blobClient = containerClient.GetBlobClient(cloudFilePath);

                var content = Encoding.UTF8.GetBytes(data);
                using (var memoryStream = new MemoryStream(content))
                {
                    blobClient.Upload(memoryStream);
                }
            }
        }

        public static async Task SendMessageAsync(string orderDetails)
        {
            // since ServiceBusClient implements IAsyncDisposable we create it with "await using"
            await using var client = new ServiceBusClient(ConnectionStringEmail);

            // create the sender
            ServiceBusSender sender = client.CreateSender(QueueName);


            // create a message that we can send. UTF-8 encoding is used when providing a string.
            ServiceBusMessage message = new ServiceBusMessage(orderDetails);

            try
            {
                // send the message
                await sender.SendMessageAsync(message);
            }
            finally
            {
                await sender.DisposeAsync();
                await client.DisposeAsync();
            }
        }
    }
}

using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

namespace Microsoft.eShopWeb.Web.Features.MessageSender;

public class QueueMessageSender
{
    private const string ConnectionString = "Endpoint=sb://azfinaltaskordersservicebus.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=XaJzStsH3gZmIm7pou+j4RR3fTJ7qQKOIc9RQoQMBHA=";
    private const string QueueName = "ordersqueue";

    public async Task SendMessageAsync(string orderDetails)
    {
        // since ServiceBusClient implements IAsyncDisposable we create it with "await using"
        await using var client = new ServiceBusClient(ConnectionString);

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

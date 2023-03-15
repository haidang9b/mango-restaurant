using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mango.MessageBus
{
    public class AzureServiceMessageBus : IMessageBus
    {
        private string connectionString = "Endpoint=sb://phanhaidang.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=ZJDz7qu8AattaOq6O0GwBPqtUHejg8SLV+ASbMVX4ng=";

        public async Task PublishMessage(BaseMessage message, string topicName)
        {
            // ISenderClient senderClient = new TopicClient(connectionString, topicName);
            await using var client = new ServiceBusClient(connectionString);
            ServiceBusSender sender = client.CreateSender(topicName);
            var jsonMessage = JsonConvert.SerializeObject(message);
            var finalMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes(jsonMessage))
            {
                CorrelationId = Guid.NewGuid().ToString(),
            };

            await sender.SendMessageAsync(finalMessage);
            await sender.DisposeAsync();
        }
    }
}

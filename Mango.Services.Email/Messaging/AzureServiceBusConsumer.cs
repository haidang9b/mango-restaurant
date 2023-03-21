using Azure.Messaging.ServiceBus;
using Mango.MessageBus;
using Mango.Services.Email.Messages;
using Mango.Services.Email.Repositories;
using Newtonsoft.Json;
using System.Text;

namespace Mango.Services.Email.Messaging
{
    public class AzureServiceBusConsumer: IAzureServiceBusConsumer
    {
        private readonly string _serviceBusConnectionString;
        private readonly string _subscriptionEmail;
        private readonly string _orderUpdatePaymentResultTopic;
        private readonly IConfiguration _configuration;
        private ServiceBusProcessor _emailProcessor;
        private readonly EmailRepository _emailRepository;
        private readonly IMessageBus _messageBus;
        public AzureServiceBusConsumer(EmailRepository emailRepository, IConfiguration configuration, IMessageBus messageBus)
        {
            _configuration = configuration;
            _messageBus = messageBus;
            _emailRepository = emailRepository;
            _serviceBusConnectionString = _configuration.GetValue<string>("ServiceBusConnectionString");
            _subscriptionEmail = _configuration.GetValue<string>("SubscriptionEmail");
            _orderUpdatePaymentResultTopic = _configuration.GetValue<string>("OrderUpdatePaymentResultTopic");
            var client = new ServiceBusClient(_serviceBusConnectionString);
            _emailProcessor = client.CreateProcessor(_orderUpdatePaymentResultTopic, _subscriptionEmail);
        }

        public async Task Start()
        {
            _emailProcessor.ProcessMessageAsync += OnOderUpdatePaymentReceived;
            _emailProcessor.ProcessErrorAsync += ErrorHandler;
            await _emailProcessor.StartProcessingAsync();
        }

        private async Task OnOderUpdatePaymentReceived(ProcessMessageEventArgs args)
        {
            var message = args.Message;
            var body = Encoding.UTF8.GetString(message.Body);

            UpdatePaymentResultMessage updatePaymentResultMessage = JsonConvert.DeserializeObject<UpdatePaymentResultMessage>(body);

            try
            {
                await _emailRepository.SendAndLogEmail(updatePaymentResultMessage);
                await args.CompleteMessageAsync(args.Message);
            }
            catch(Exception ex)
            {
                throw;
            }
        }

        public async Task Stop()
        {
            await _emailProcessor.StopProcessingAsync();
            await _emailProcessor.DisposeAsync();
        }

        Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }
    }
}

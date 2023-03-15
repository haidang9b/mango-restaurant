using Azure.Messaging.ServiceBus;
using Mango.MessageBus;
using Mango.Services.PaymentAPI.Messages;
using Newtonsoft.Json;
using PaymentProcessor;
using System.Text;

namespace Mango.Services.PaymentAPI.Messaging
{
    public class AzureServiceBusConsumer: IAzureServiceBusConsumer
    {
        private readonly string _serviceBusConnectionString;
        private readonly string _subscriptionPayment;
        private readonly string _orderPaymentProcessTopic;
        private readonly string _orderUpdatePaymentResultTopic;
        private readonly IConfiguration _configuration;

        private ServiceBusProcessor _orderPaymentProcessor;
        private readonly IProcessPayment _processPayment;
        private readonly IMessageBus _messageBus;
        public AzureServiceBusConsumer(IProcessPayment processPayment, IConfiguration configuration, IMessageBus messageBus)
        {
            _configuration = configuration;
            _messageBus = messageBus;
            _processPayment = processPayment;

            _serviceBusConnectionString = _configuration.GetValue<string>("ServiceBusConnectionString");
            _orderPaymentProcessTopic = _configuration.GetValue<string>("OrderPaymentProcessTopic");
            _subscriptionPayment = _configuration.GetValue<string>("OrderPaymentProcessSubscription");
            _orderUpdatePaymentResultTopic = _configuration.GetValue<string>("OrderUpdatePaymentResultTopic");
            var client = new ServiceBusClient(_serviceBusConnectionString);
            _orderPaymentProcessor = client.CreateProcessor(_orderPaymentProcessTopic, _subscriptionPayment);
        }

        public async Task Start()
        {
            _orderPaymentProcessor.ProcessMessageAsync += OnOrderPaymentReceived;
            _orderPaymentProcessor.ProcessErrorAsync += ErrorHandler;
            await _orderPaymentProcessor.StartProcessingAsync();
        }

        public async Task Stop()
        {
            await _orderPaymentProcessor.StopProcessingAsync();
            await _orderPaymentProcessor.DisposeAsync();
        }

        Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }

        private async Task OnOrderPaymentReceived(ProcessMessageEventArgs args)
        {
            var message = args.Message;
            var body = Encoding.UTF8.GetString(message.Body);
            
            PaymentRequestMessage paymentRequestMessage = JsonConvert.DeserializeObject<PaymentRequestMessage>(body);

            var result = _processPayment.PaymentProcessor();

            UpdatePaymentResultMessage updatePaymentResultMessage = new()
            {
                Status = result,
                OrderId = paymentRequestMessage.OrderId
            };

            try
            {
                await _messageBus.PublishMessage(updatePaymentResultMessage, _orderUpdatePaymentResultTopic);
            }
            catch(Exception ex)
            {
                throw;
            }
        }
    }
}

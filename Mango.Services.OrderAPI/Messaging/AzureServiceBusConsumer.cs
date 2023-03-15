using Azure.Messaging.ServiceBus;
using Mango.MessageBus;
using Mango.Services.OrderAPI.Messages;
using Mango.Services.OrderAPI.Models;
using Mango.Services.OrderAPI.Repositories;
using Newtonsoft.Json;
using System.Text;

namespace Mango.Services.OrderAPI.Messaging
{
    public class AzureServiceBusConsumer: IAzureServiceBusConsumer
    {
        private readonly string _serviceBusConnectionString;
        private readonly string _subscriptionName;
        private readonly string _checkoutMessageTopic;
        private readonly string _orderPaymentProcessTopic;
        private readonly OrderRepository _orderRepository;
        private readonly IConfiguration _configuration;
        private ServiceBusProcessor _checkoutProcessor;
        private readonly IMessageBus _messageBus;
        public AzureServiceBusConsumer(OrderRepository orderRepository, IConfiguration configuration, IMessageBus messageBus)
        {
            _orderRepository = orderRepository;
            _configuration = configuration;
            _messageBus = messageBus;

            _serviceBusConnectionString = _configuration.GetValue<string>("ServiceBusConnectionString");
            _subscriptionName = _configuration.GetValue<string>("SubscriptionName");
            _checkoutMessageTopic = _configuration.GetValue<string>("CheckoutMessageTopic");
            _orderPaymentProcessTopic = _configuration.GetValue<string>("OrderPaymentProcessTopic");

            var client = new ServiceBusClient(_serviceBusConnectionString);
            _checkoutProcessor = client.CreateProcessor(_checkoutMessageTopic, _subscriptionName);
        }

        public async Task Start()
        {
            _checkoutProcessor.ProcessMessageAsync += OnCheckoutMessageReceived;
            _checkoutProcessor.ProcessErrorAsync += ErrorHandler;
            await _checkoutProcessor.StartProcessingAsync();
        }

        public async Task Stop()
        {
            await _checkoutProcessor.StopProcessingAsync();
            await _checkoutProcessor.DisposeAsync();
        }

        Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }

        private async Task OnCheckoutMessageReceived(ProcessMessageEventArgs args)
        {
            var message = args.Message;
            var body = Encoding.UTF8.GetString(message.Body);

            var checkoutHeaderDto = JsonConvert.DeserializeObject<CheckoutHeaderDto>(body);

            OrderHeader orderHeader = new()
            {
                UserId = checkoutHeaderDto.UserId,
                FirstName = checkoutHeaderDto.FirstName,
                LastName = checkoutHeaderDto.LastName,
                OrderDetails = new List<OrderDetails>(),
                CardNumber = checkoutHeaderDto.CardNumber,
                CouponCode = checkoutHeaderDto.CouponCode,
                CVV = checkoutHeaderDto.CVV,
                DiscountTotal = checkoutHeaderDto.DiscountTotal,
                Email = checkoutHeaderDto.Email,
                ExpiryMonthYear = checkoutHeaderDto.ExpiryMonthYear,
                OrderTime = DateTime.Now,
                OrderTotal = checkoutHeaderDto.OrderTotal,
                PaymentStatus = false,
                Phone = checkoutHeaderDto.Phone,
                PickupDateTime = checkoutHeaderDto.PickupDateTime,
            };

            foreach(var detailList in checkoutHeaderDto.CartDetails)
            {
                OrderDetails orderDetails = new OrderDetails
                {
                    ProductId = detailList.ProductId,
                    ProductName = detailList.Product.Name,
                    Price = detailList.Product.Price,
                    Count = detailList.Count,
                };
                orderHeader.CartTotalItems += detailList.Count;
                orderHeader.OrderDetails.Add(orderDetails);
            }

            await _orderRepository.AddOrder(orderHeader);
            PaymentRequestMessage paymentRequestMessage = new ()
            {
                Name = orderHeader.FirstName + " " + orderHeader.LastName,
                CardNumber = orderHeader.CardNumber,
                CVV = checkoutHeaderDto.CVV,
                ExpiryMonthYear = orderHeader.ExpiryMonthYear,
                OrderId = orderHeader.OrderHeaderId,
                OrderTotal = orderHeader.OrderTotal,
            };
            try
            {
                await _messageBus.PublishMessage(paymentRequestMessage, _orderPaymentProcessTopic);
                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}

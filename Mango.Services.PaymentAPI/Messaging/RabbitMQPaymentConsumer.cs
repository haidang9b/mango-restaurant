using Mango.MessageBus;
using Mango.Services.PaymentAPI.Messages;
using Mango.Services.PaymentAPI.RabbitMQSender;
using Newtonsoft.Json;
using PaymentProcessor;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading.Channels;

namespace Mango.Services.PaymentAPI.Messaging
{
    public class RabbitMQPaymentConsumer : BackgroundService
    {
        private IConnection _connection;
        private IModel _channel;
        private readonly IRabbitMQPaymentMessageSender _rabbitMQPaymentMessageSender;
        private readonly IProcessPayment _processPayment;
        private const string ExchangeName = "PublishSubscribePaymentUpdate_Exchange";

        public RabbitMQPaymentConsumer(IRabbitMQPaymentMessageSender rabbitMQPaymentMessageSender, IProcessPayment processPayment)
        {
            _rabbitMQPaymentMessageSender = rabbitMQPaymentMessageSender;
            _processPayment = processPayment;
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest",
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: "orderpaymentprocesstopic", false, false, arguments: null);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (ch, ea) =>
            {
                var body = Encoding.UTF8.GetString(ea.Body.ToArray());
                PaymentRequestMessage paymentRequestMessage = JsonConvert.DeserializeObject<PaymentRequestMessage>(body);
                HandleMessage(paymentRequestMessage).GetAwaiter().GetResult();
                
                _channel.BasicAck(ea.DeliveryTag, false);

            }; // ch is channel, ea is event args

            _channel.BasicConsume("orderpaymentprocesstopic", false, consumer);
            return Task.CompletedTask;
        }

        private async Task HandleMessage(PaymentRequestMessage paymentRequestMessage)
        {
            var result = _processPayment.PaymentProcessor();

            UpdatePaymentResultMessage updatePaymentResultMessage = new()
            {
                Status = result,
                OrderId = paymentRequestMessage.OrderId,
                Email = paymentRequestMessage.Email,
            };

            try
            {
                // await _messageBus.PublishMessage(updatePaymentResultMessage, _orderUpdatePaymentResultTopic);
                // using rabbit mq
                _rabbitMQPaymentMessageSender.SendMessage(updatePaymentResultMessage);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}

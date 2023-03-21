using Mango.MessageBus;
using Mango.Services.Email.Messages;
using Mango.Services.Email.Repositories;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading.Channels;

namespace Mango.Services.Email.Messaging
{
    public class RabbitMQEmailConsumer : BackgroundService
    {
        private IConnection _connection;
        private IModel _channel;
        private const string ExchangeName = "PublishSubscribePaymentUpdate_Exchange";
        private readonly EmailRepository _emailRepository;
        string queueName = "";

        public RabbitMQEmailConsumer(EmailRepository emailRepository)
        {
            _emailRepository = emailRepository;
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest",
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(ExchangeName, ExchangeType.Fanout);
            queueName = _channel.QueueDeclare().QueueName;
            _channel.QueueBind(queueName, ExchangeName, "");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (ch, ea) =>
            {
                var body = Encoding.UTF8.GetString(ea.Body.ToArray());
                UpdatePaymentResultMessage updatePaymentResultMessage = JsonConvert.DeserializeObject<UpdatePaymentResultMessage>(body);
                HandleMessage(updatePaymentResultMessage).GetAwaiter().GetResult();

                _channel.BasicAck(ea.DeliveryTag, false);

            }; // ch is channel, ea is event args

            _channel.BasicConsume(queueName, false, consumer);
            return Task.CompletedTask;
        }

        private async Task HandleMessage(UpdatePaymentResultMessage updatePaymentResultMessage)
        {
            try
            {
                await _emailRepository.SendAndLogEmail(updatePaymentResultMessage);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}

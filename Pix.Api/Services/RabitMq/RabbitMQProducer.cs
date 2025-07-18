using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace Pix.Api.Services.RabitMq
{
    public class RabbitMQProducer
    {
        private readonly IConfiguration _config;

        public RabbitMQProducer(IConfiguration config)
        {
            _config = config;
        }

        public void EnviarMensagem<T>(T mensagem, string queueName)
        {
            var factory = new ConnectionFactory() 
            { 
                HostName = "rabbitmq" 
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false);

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(mensagem));

            channel.BasicPublish(exchange: "", routingKey: queueName, body: body);
        }
    }
}

using Bank.Api.DTOS;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Shared.Messages.DTOs;

namespace Bank.Api.Services.RabbitMQServices
{
    public class RabbitMQPublisher : IRabbitMQPublisher
    {
        public void PublishUserCreated(UserCreatedEvent user)
        {
            var factory = new ConnectionFactory() { HostName = "rabbitmq" }; 
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: "user-created", durable: false, exclusive: false, autoDelete: false, arguments: null);

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(user));

            channel.BasicPublish(exchange: "", routingKey: "user-created", basicProperties: null, body: body);
        }


    }
}

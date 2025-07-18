using Bank.Api.DTOS;
using Shared.Messages.DTOs;

namespace Bank.Api.Services.RabbitMQServices
{
    public interface IRabbitMQPublisher
    {
        void PublishUserCreated(UserCreatedEvent user);
    }
}

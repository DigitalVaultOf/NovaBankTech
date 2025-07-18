namespace Payments.Api.DTOS;

public class PaySlipDto
{
    public Guid PaymentId { get; set; }
    public required string UserPassword {get; set;}
}
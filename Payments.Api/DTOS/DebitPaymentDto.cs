namespace Payments.Api.DTOS;

public class DebitPaymentDto
{
    public string? AccountNumber { get; set; }
    public decimal Value { get; set; }
}
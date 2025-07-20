namespace Bank.Api.DTOS;

public class PaymentDto
{
    public string? AccountNumber { get; set; }
    public decimal Value { get; set; }
    public required string UserPassword { get; set; }
}
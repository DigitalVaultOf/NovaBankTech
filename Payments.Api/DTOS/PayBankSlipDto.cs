namespace Payments.Api.DTOS;

public class PayBankSlipDto
{
    public string BankSlipNumber { get; set; } = string.Empty;
    public required string UserPassword {get; set;}
}
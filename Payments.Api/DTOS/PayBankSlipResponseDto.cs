namespace Payments.Api.DTOS;

public class PayBankSlipResponseDto
{
    public string BankSlipNumber { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public decimal OriginalAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal RemainingAmount { get; set; }
    public bool IsFullyPaid { get; set; }
    public DateTime PaymentDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Customer { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public string? Message { get; set; } = string.Empty;
}
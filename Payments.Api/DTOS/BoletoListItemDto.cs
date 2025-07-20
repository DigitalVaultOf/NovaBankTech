namespace Payments.Api.DTOS;

public class BoletoListItemDto
{
    public Guid PaymentId { get; set; }
    public string BankSlipNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime DueDate { get; set; }
    public bool IsPaid { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string? Description { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string Customer { get; set; } = string.Empty;
}
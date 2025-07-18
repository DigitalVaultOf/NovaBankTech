namespace Payments.Api.DTOS;

public class AddPaymentDto
{
    public Guid UserId { get; set; }
    public string AccountNumber { get; set; }
    public decimal Amount { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public DateTime DueDate { get; set; }
}
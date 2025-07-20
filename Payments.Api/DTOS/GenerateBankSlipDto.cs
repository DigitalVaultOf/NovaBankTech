namespace Payments.Api.DTOS;

public class GenerateBankSlipDto
{
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
}
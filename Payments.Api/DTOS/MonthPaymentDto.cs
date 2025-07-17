namespace Payments.Api.DTOS;

public class MonthPaymentDto
{
    public Guid UserId { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
}
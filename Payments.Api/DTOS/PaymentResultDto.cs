namespace Payments.Api.DTOS;

public class PaymentResultDto
{
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}
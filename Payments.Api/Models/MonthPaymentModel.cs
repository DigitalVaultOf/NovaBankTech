using System.ComponentModel.DataAnnotations;

namespace Payments.Api.Models;

public class MonthPaymentModel
{
    [Key] public Guid PaymentId { get; init; } = Guid.NewGuid();

    [Required(ErrorMessage = "ID do usuário é obrigatório.")]
    public Guid UserId { get; init; }

    [Required(ErrorMessage = "O número da conta é obrigatório.")]
    public required string AccountNumber { get; init; }

    public long? BankSlipNumber { get; init; }
    public bool IsPaid { get; set; }
    public DateTime? PaymentDate { get; set; }
    public int Month { get; init; }
    public int Year { get; init; }
    public DateTime DueDate { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public decimal Amount { get; init; }
}
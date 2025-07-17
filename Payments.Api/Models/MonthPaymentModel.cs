using System.ComponentModel.DataAnnotations;

namespace Payments.Api.Models;

public class MonthPaymentModel
{
    [Key] public Guid PaymentId { get; set; } = Guid.NewGuid();

    [Required(ErrorMessage = "ID do usuário é obrigatório")]
    public Guid UserId { get; set; }

    public int? BankSlipNumber { get; set; }
    public bool BankSlipIsPaid { get; set; }
    public DateTime? PaymentDate { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    public decimal Amount { get; set; }
}
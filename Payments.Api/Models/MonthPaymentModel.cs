using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Payments.Api.Models;

[Table("MonthPayments")]
[Index(nameof(UserId))]
[Index(nameof(IsPaid))]
[Index(nameof(BankSlipNumber))] // ← Índice simples, sem unique
public class MonthPaymentModel
{
    [Key] 
    public Guid PaymentId { get; init; } = Guid.NewGuid();

    [Required(ErrorMessage = "ID do usuário é obrigatório.")]
    public Guid UserId { get; init; }

    [Required(ErrorMessage = "O número da conta é obrigatório.")]
    [StringLength(20)]
    public required string AccountNumber { get; init; }

    public long? BankSlipNumber { get; init; } // ← MANTÉM como estava - pode ser null

    public bool IsPaid { get; set; } = false; // ← MELHORIA: Default false

    public DateTime? PaymentDate { get; set; }

    [Range(1, 12, ErrorMessage = "Mês deve estar entre 1 e 12.")]
    public int Month { get; init; }

    [Range(2020, 2100, ErrorMessage = "Ano inválido.")]
    public int Year { get; init; }

    [Required(ErrorMessage = "Data de vencimento é obrigatória.")]
    public DateTime DueDate { get; init; }

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Required(ErrorMessage = "Valor é obrigatório.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Valor deve ser maior que zero.")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    // ← NOVO CAMPO
    [StringLength(500)]
    public string? Description { get; init; }

    // ← NOVA PROPRIEDADE COMPUTADA
    [NotMapped]
    public bool IsOverdue => !IsPaid && DateTime.UtcNow > DueDate;

    // ← NOVA PROPRIEDADE COMPUTADA
    [NotMapped]
    public int DaysUntilDue => (DueDate - DateTime.UtcNow).Days;
}
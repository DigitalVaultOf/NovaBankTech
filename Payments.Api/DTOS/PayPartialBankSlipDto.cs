using System.ComponentModel.DataAnnotations;

namespace Payments.Api.DTOS;

public class PayPartialBankSlipDto
{
    [Required(ErrorMessage = "Número do boleto é obrigatório.")]
    public string BankSlipNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Senha do usuário é obrigatória.")]
    public string UserPassword { get; set; } = string.Empty;

    // ✅ NOVO: Pagamento parcial
    [Range(0.01, double.MaxValue, ErrorMessage = "Valor a pagar deve ser maior que zero.")]
    public decimal? AmountToPay { get; set; }
}
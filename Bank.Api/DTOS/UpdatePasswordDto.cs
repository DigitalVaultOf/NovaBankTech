using System.ComponentModel.DataAnnotations;

namespace Bank.Api.DTOS;

public class UpdatePasswordDto
{
    // 1. Nome da propriedade corrigido para "CurrentPassword"
    [Required(ErrorMessage = "A senha atual é obrigatória.")]
    [DataType(DataType.Password)]
    public string CurrentPassword { get; set; }

    [Required(ErrorMessage = "A nova senha é obrigatória.")]
    // 2. Validação de tamanho mínimo para a nova senha
    [StringLength(100, ErrorMessage = "A nova senha deve ter no mínimo {2} caracteres.", MinimumLength = 8)]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; }

    [Required(ErrorMessage = "A confirmação da nova senha é obrigatória.")]
    [DataType(DataType.Password)]
    // 3. Validação para garantir que a confirmação é igual à nova senha
    [Compare("NewPassword", ErrorMessage = "A nova senha e a confirmação não são iguais.")]
    public string ConfirmNewPassword { get; set; }
}
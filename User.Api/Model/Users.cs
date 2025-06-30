using System.ComponentModel.DataAnnotations;

namespace User.Api.Model
{
    public class Users
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required(ErrorMessage = "Cpf é obrigatório")]
        [StringLength(14, MinimumLength = 11, ErrorMessage = "Cpf deve ter de 11 a 14 caracteres")]
        public string Cpf { get; set; }

        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(100, ErrorMessage = "Nome deve ter no máximo 100 caracteres")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [StringLength(100, ErrorMessage = "Email deve ter no máximo 100 caracteres")]
        public string Email { get; set; }

        public ICollection<Account> Accounts { get; set; }
    }
}

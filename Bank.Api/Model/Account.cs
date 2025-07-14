using Bank.Api.Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using User.Api.CamposEnum;

namespace User.Api.Model
{
    public class Account
    {
        [Key]
        public string AccountNumber { get; set; }

        [Required(ErrorMessage = "ID do usuário é obrigatório")]
        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        public Users User { get; set; }

        [Required(ErrorMessage = "Tipo de conta é obrigatório")]
        public AccountTypeEnum AccountType { get; set; }

        [Required(ErrorMessage = "Saldo é obrigatório")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; } = 0;

        [Required(ErrorMessage = "Data de criação é obrigatória")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required]
        [StringLength(255)]
        public string SenhaHash { get; set; }

        public string? Token { get; set; }

        public ICollection<Transfer> TransfersSent { get; set; }
        public ICollection<Transfer> TransfersReceived { get; set; }
    }
}

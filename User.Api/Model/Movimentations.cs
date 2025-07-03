using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Bank.Api.CamposEnum;

namespace Bank.Api.Model
{
    public class Movimentations
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "É obrigatorio escolher um valor de deposito/saque")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }


        [Required(ErrorMessage = "Numero de conta obrigatorio")]
        [ForeignKey("AccountNumber")]
        public string accountNumber { get; set; }

        public MovimentTypeEnum MovimentTypeEnum { get; set; }

        [Required(ErrorMessage = "Não foi possivel localizar a data")]
        public DateTime DateTimeMoviment { get; set; } = DateTime.Now;
    }
}

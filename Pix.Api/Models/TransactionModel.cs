using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pix.Api.Models
{
    public class TransactionModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [ForeignKey("Going")]
        public string Going { get; set; }

        [ForeignKey("Coming")]
        public string Coming { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Required(ErrorMessage = "Precisa de Um valor")]
        public decimal Amount { get; set; }
    }
}

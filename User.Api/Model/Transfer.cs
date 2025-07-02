using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using User.Api.Model;

namespace Bank.Api.Model
{
    public class Transfer
    {
        public int Id { get; set; }

        [Required]
        public string AccountNumberFrom { get; set; }

        [Required]
        public string AccountNumberTo { get; set; }

        [ForeignKey("AccountNumberFrom")]
        public Account AccountFrom { get; set; }

        [ForeignKey("AccountNumberTo")]
        public Account AccountTo { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public DateTime TransferDate { get; set; } = DateTime.Now;

        public string? Description { get; set; }
    }

}

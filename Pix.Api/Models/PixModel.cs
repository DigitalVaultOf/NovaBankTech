using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Pix.Api.Models
{
    [Index(nameof(PixKey), IsUnique = true)]
    public class PixModel
    {
        [Key]
        public Guid IdPix { get; set; } =  Guid.NewGuid();

        [Required(ErrorMessage = "É obrigatorio um Nome")]
        public string NameUser { get; set; }

        [Required(ErrorMessage = "É obrigatorio uma Chave")]
        public string PixKey { get; set; }

        [Required(ErrorMessage = "É obrigatorio ter um Banco")]
        public string Bank { get; set; }

        [Required(ErrorMessage = "É obrigatorio ter um numero de conta")]
        public string AccountNumber { get; set; }
    }
}

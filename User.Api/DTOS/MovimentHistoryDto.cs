using System.ComponentModel.DataAnnotations.Schema;
using Bank.Api.CamposEnum;
using User.Api.Model;

namespace Bank.Api.DTOS
{
    public class MovimentHistoryDto
    {
        public string MovimentTypeEnum { get; set; }
        public DateTime DateTimeMoviment { get; set; }
        public decimal Amount { get; set; }
        public string? acountNumber { get; set; }

    }
}

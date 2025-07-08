using System.Text.Json.Serialization;
using Bank.Api.CamposEnum;

namespace Bank.Api.DTOS
{
    public class MovimentationDto
    {
        [JsonIgnore]
        public string? acountNumber { get; set; }
        public decimal value { get; set; }
        [JsonIgnore]
        public MovimentTypeEnum type { get; set; }
        public string Password { get; set; }
    }
}

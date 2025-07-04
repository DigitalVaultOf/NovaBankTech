using Bank.Api.CamposEnum;

namespace Bank.Api.DTOS
{
    public class MovimentationDto
    {
        public string? acountNumber { get; set; }
        public decimal value { get; set; }
        public MovimentTypeEnum type { get; set; }
    }
}

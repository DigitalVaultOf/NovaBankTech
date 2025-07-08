using System.Text.Json.Serialization;

namespace Bank.Api.DTOS
{
    public class TransferRequestDTO
    {
        [JsonIgnore]
        public string? AccountNumberFrom { get; set; }
        public  string AccountNumberTo { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public string Password { get; set; }
    }
}

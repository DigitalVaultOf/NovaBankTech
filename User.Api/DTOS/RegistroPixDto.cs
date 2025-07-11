using System.Text.Json.Serialization;

namespace Bank.Api.DTOS
{
    public class RegistroPixDto
    {
        public string Name { get; set; }

        public string PixKey { get; set; }

        public string Bank { get; set; }

        [JsonIgnore]
        public string? Account { get; set; }
    }
}

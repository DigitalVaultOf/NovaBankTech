namespace Auth.Api.Dtos
{
    public class LoginRequestDto
    {
        public string? AccountNumber { get; set; }
        public string? Cpf { get; set; }
        public string? Email { get; set; }
        public string Password { get; set; }
    }
}

namespace User.Api.DTOS
{
    public class AccountLoginDto
    {
        public string AccountNumber { get; set; }
        public string Cpf { get; set; }
        public string Email { get; set; }
        public string SenhaHash { get; set; }
        public Guid UserId { get; set; }
        public bool Status { get; set; } 
    }
}

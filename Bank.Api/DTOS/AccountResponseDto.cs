namespace User.Api.DTOS
{
    public class AccountResponseDto
    {
        public string AccountNumber { get; set; }
        public string AccountType { get; set; }
        public decimal Balance { get; set; }
        public string Name { get; set; }
    }
}

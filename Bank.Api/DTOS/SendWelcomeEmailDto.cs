namespace Bank.Api.DTOS
{
    public class SendWelcomeEmailDto
    {
        public string nome { get; set; }
        public string email { get; set; }
        public string contaCorrente { get; set; }
        public string contaPoupanca { get; set; }
    }
}

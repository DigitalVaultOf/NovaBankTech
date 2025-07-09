namespace Email.Api.Services.RegisterEmailServices
{
    public interface IRegisterEmailService
    {
        Task<bool> RegisterEmail(string destinatario, string assunto, string corpoHtml);
        Task<bool> EnviarEmailBoasVindas(string destinatario, string nomeUsuario, string contaCorrente, string contaPoupanca);
    }
}

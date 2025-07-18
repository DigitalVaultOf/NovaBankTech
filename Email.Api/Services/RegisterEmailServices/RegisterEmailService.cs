using System.Net.Mail;
using System.Net;
using Email.Api.Model;
using Microsoft.Extensions.Options;
using MimeKit;
using Email.Api.Templates;

namespace Email.Api.Services.RegisterEmailServices
{
    public class RegisterEmailService : IRegisterEmailService
    {
        private readonly SmtpSettings _smtp;

        public RegisterEmailService(IOptions<SmtpSettings> smtp)
        {
            _smtp = smtp.Value;
        }

        public async Task<bool> RegisterEmail(string destinatario, string assunto, string corpoHtml)
        {

            try
            {
                using var client = new SmtpClient(_smtp.Server, _smtp.Port)
                {
                    Credentials = new NetworkCredential(_smtp.Username, _smtp.Password),
                    EnableSsl = _smtp.EnableSSL
                };

                var message = new MailMessage
                {
                    From = new MailAddress(_smtp.Username),
                    Subject = assunto,
                    Body = corpoHtml,
                    IsBodyHtml = true
                };

                message.To.Add(destinatario);

                await client.SendMailAsync(message);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao enviar e-mail: {ex.Message}");
                return false;
            }

        }

        public async Task<bool> EnviarEmailBoasVindas(string destinatario, string nomeUsuario, string contaCorrente, string contaPoupanca)
        {
            var html = EmailTemplates.GerarEmailBoasVindas(nomeUsuario, contaCorrente, contaPoupanca);
            return await RegisterEmail(destinatario, "Bem-vindo a nossa familia!", html);
        }

    }
}

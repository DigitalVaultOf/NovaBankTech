namespace Email.Api.Templates
{
    public static class EmailTemplates
    {
        public static string GerarEmailBoasVindas(string nomeUsuario, string contaCorrente, string contaPoupanca)
        {
            return $@"
            <html>
            <body>
                <h2>Olá {nomeUsuario},</h2>
                <p>Seja bem-vindo ao nosso banco! Sua conta foi criada com sucesso.</p>
                <p>Informações da conta:</p>
                <ul>
                    <li><strong>Conta Corrente:</strong> {contaCorrente}</li>
                    <li><strong>Conta Poupança:</strong> {contaPoupanca}</li>
                </ul>
                <p>Atenciosamente,<br/>Equipe NovaBankTech</p>
            </body>
            </html>";
        }
    }
}

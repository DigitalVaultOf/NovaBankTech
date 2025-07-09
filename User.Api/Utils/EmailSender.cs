namespace Bank.Api.Utils
{
    public class EmailSender
    {
        private readonly HttpClient _http;

        public EmailSender(HttpClient http)
        {
            _http = http;
        }

        public async Task<bool> EnviarEmail(string to, string subject, string html)
        {
            var payload = new
            {
                To = to,
                Subject = subject,
                Body = html
            };

            var response = await _http.PostAsJsonAsync("http://localhost:5000/email/send", payload);
            return response.IsSuccessStatusCode;
        }
    }
}

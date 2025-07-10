using Bank.Api.DTOS;

namespace Bank.Api.Utils
{
    public class PixClient
    {
        private readonly HttpClient _httpClient;

        public PixClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task CriarPixAsync(RegistroPixDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync("https://localhost:7250/Pix/registrar", dto);
            response.EnsureSuccessStatusCode();
        }
    }
}

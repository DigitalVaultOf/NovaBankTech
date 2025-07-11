using Bank.Api.DTOS;

namespace Bank.Api.Utils
{
    public class PixClient
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PixClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        private void AddAuthorizationHeader()
        {
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();

            if (!string.IsNullOrEmpty(token) && !_httpClient.DefaultRequestHeaders.Contains("Authorization"))
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", token);
            }
        }

        public async Task CriarPixAsync(RegistroPixDto dto)
        {
            AddAuthorizationHeader();

            var response = await _httpClient.PostAsJsonAsync("https://localhost:7250/api/Pix/registrar", dto);
            response.EnsureSuccessStatusCode();
        }
    }
}

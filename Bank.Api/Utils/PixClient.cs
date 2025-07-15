using System;
using Bank.Api.DTOS;
using User.Api.Model;
using Newtonsoft.Json;

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

            var response = await _httpClient.PostAsJsonAsync("http://apigateway:8080/pix/api/registrar"
, dto);
            response.EnsureSuccessStatusCode();
        }

        public async Task<ResponseModel<string>> Pesquisar(string chave)
        {
            AddAuthorizationHeader();
            var resposes = new ResponseModel<string>();
            var response = await _httpClient.PostAsJsonAsync("http://apigateway:8080/pix/api/pix/get", chave);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var resultadoInterno = JsonConvert.DeserializeObject<ResponseModel<string>>(json);
                resposes.Data = resultadoInterno.Data;
            }
            else
            {
                resposes.Message = $"Erro ao registrar chave Pix: {response.StatusCode}";
            }

            return resposes;
        }
        public async Task MandarPixAsync(MakePixDto dto)
        {
            AddAuthorizationHeader();

            var response = await _httpClient.PostAsJsonAsync("http://apigateway:8080/pix/api/mandar", dto);
            response.EnsureSuccessStatusCode();
        }
    }
}

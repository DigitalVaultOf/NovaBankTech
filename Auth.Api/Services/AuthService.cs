using Auth.Api.Dtos;
using Auth.Api.Models;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Auth.Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<ResponseModel<LoginResponseDto>> AuthenticateAsync(LoginRequestDto dto)
        {
            var response = new ResponseModel<LoginResponseDto>();
            try
            {
                var client = _httpClientFactory.CreateClient();

                string baseUrl = $"{_configuration["UserApi:BaseUrl"]}"; // Deve ser "http://apigateway:8080/user/api/User"

                string url;

                if (!string.IsNullOrWhiteSpace(dto.AccountNumber))
                {
                    url = $"{baseUrl}/GetAccountByLogin/{Uri.EscapeDataString(dto.AccountNumber.Trim())}";
                }
                else if (!string.IsNullOrWhiteSpace(dto.Cpf))
                {
                    url = $"{baseUrl}/GetAccountByCpf/{Uri.EscapeDataString(dto.Cpf.Trim())}";
                }
                else if (!string.IsNullOrWhiteSpace(dto.Email))
                {
                    url = $"{baseUrl}/GetAccountByEmail/{Uri.EscapeDataString(dto.Email.Trim())}";
                }
                else
                {
                    throw new Exception("É necessário informar AccountNumber, CPF ou Email.");
                }

                var userResponse = await client.GetAsync(url);

                if (!userResponse.IsSuccessStatusCode)
                {
                    throw new Exception($"Erro ao autenticar usuário. Status: {userResponse.StatusCode}");
                }

                var accountJson = await userResponse.Content.ReadAsStringAsync();
                dynamic result = JsonConvert.DeserializeObject(accountJson);

                string userIdStr = result.data.userId;
                string senhaHash = result.data.senhaHash;

                bool senhaValida = BCrypt.Net.BCrypt.Verify(dto.Password, senhaHash);
                if (!senhaValida)
                {
                    throw new Exception("Senha inválida.");
                }

                string accountNumber = result.data.accountNumber;

                var claims = new List<Claim>
        {
            new Claim("AccountNumber", accountNumber),
            new Claim("UserId", userIdStr)
        };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(30),
                    signingCredentials: creds
                );

                string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                // Atualiza token no User API
                var updateUrl = $"{baseUrl}/update-token/{accountNumber}";
                var updateDto = new { token = tokenString };
                var json = JsonConvert.SerializeObject(updateDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var saveTokenResponse = await client.PutAsync(updateUrl, content);

                if (!saveTokenResponse.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Erro ao salvar token no User API. Status: {saveTokenResponse.StatusCode}");
                }

                response.Data = new LoginResponseDto { Token = tokenString };
                response.Message = "Usuário autenticado com sucesso!";
                return response;
            }
            catch (Exception ex)
            {
                response.Message = $"Erro ao autenticar usuário: {ex.Message}";
                return response;
            }
        }

    }
}

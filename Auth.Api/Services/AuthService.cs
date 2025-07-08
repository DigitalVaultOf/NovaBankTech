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
            ResponseModel<LoginResponseDto> response = new ResponseModel<LoginResponseDto>();
            try
            {
                var cliente = _httpClientFactory.CreateClient();
                var url = $"{_configuration["UserApi:BaseUrl"]}/api/User/GetAccountByLogin/{dto.AccountNumber}";

                var userResponse = cliente.GetAsync(url);
                if (!userResponse.Result.IsSuccessStatusCode)
                {
                    throw new Exception("Erro ao autenticar usuário.");
                }

                var accountJson = await userResponse.Result.Content.ReadAsStringAsync();
                dynamic result = JsonConvert.DeserializeObject(accountJson);

                string userIdStr = result.data.userId;

                string senhaHash = result.data.senhaHash;

                bool senhaValida = BCrypt.Net.BCrypt.Verify(dto.Password, senhaHash);

                if (!senhaValida)
                {
                    throw new Exception("Senha inválida.");
                }

                var claims = new List<Claim>
            {
                new Claim("AccountNumber", dto.AccountNumber),
                new Claim("UserId", userIdStr)
            };

                var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(30),
                    signingCredentials: creds
                );

                LoginResponseDto loginResponseDto = new LoginResponseDto
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(token)
                };

                var updateUrl = $"{_configuration["UserApi:BaseUrl"]}/api/User/update-token/{dto.AccountNumber}";
                var updateDto = new { token = loginResponseDto.Token };
                var json = JsonConvert.SerializeObject(updateDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var saveTokenResponse = await cliente.PutAsync(updateUrl, content);

                if (!saveTokenResponse.IsSuccessStatusCode)
                {
                    Console.WriteLine("Erro ao salvar token no User API.");
                }

                response.Data = loginResponseDto;
                response.Message = "Usuário autenticado com sucesso!";
                return response;
            }
            catch (Exception ex)
            {

                response.Message = $"Erro ao autenticar usuário: {ex.Message} | Inner: {ex.InnerException?.Message}";
                return response;
            }
            
        }
    }
}

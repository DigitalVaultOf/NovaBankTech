using Auth.Api.Dtos;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

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


        public async Task<LoginResponseDto> AuthenticateAsync(LoginRequestDto dto)
        {
            var cliente = _httpClientFactory.CreateClient();
            var url = $"{_configuration["UserApi:BaseUrl"]}/api/User/GetAccountByLogin/{dto.AccountNumber}";

            var response = cliente.GetAsync(url);
            if (!response.Result.IsSuccessStatusCode)
            {
                throw new Exception("Erro ao autenticar usuário.");
            }

            var accountJson = await response.Result.Content.ReadAsStringAsync();
            dynamic account = JsonConvert.DeserializeObject(accountJson);

            string senhaHash = account.senhaHash;

            bool senhaValida = BCrypt.Net.BCrypt.Verify(dto.Password, senhaHash);

            if (!senhaValida)
            {
                throw new Exception("Senha inválida.");
            }

            var claims = new List<Claim>
            {
                new Claim("AccountNumber", dto.AccountNumber)
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

            return new LoginResponseDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token)
            };
        }
    }
}
